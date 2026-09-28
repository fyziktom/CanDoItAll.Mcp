using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using CanDoItAll.Mcp.DotNetWatch.Configuration;

namespace CanDoItAll.Mcp.DotNetWatch.Runtime;

internal sealed record TailwindWorkspacePlan(
    string RepoRoot,
    string PackageDirectory,
    string PackageJsonPath,
    string ScriptName,
    string ScriptCommand);

internal sealed record NpmCommand(string Command, IReadOnlyList<string> Arguments);

public sealed class TailwindCompanionCoordinator(
    RuntimeConfiguration configuration,
    ProcessSupervisor processSupervisor,
    ILogger<TailwindCompanionCoordinator> logger)
{
    internal TailwindSessionCompanion? TryStart(AppSession session, AppStartTemplate template)
    {
        if (!configuration.TailwindAutoDetect ||
            template.LaneKind != RuntimeLaneKind.SourceWatch ||
            template.LaunchType != AppLaunchType.Project)
        {
            return null;
        }

        var plan = TailwindWorkspaceDetector.TryDetect(
            template.ProjectPath,
            template.WorkingDirectory,
            configuration.TailwindWatchScript);
        if (plan is null)
        {
            return null;
        }

        var companion = new TailwindSessionCompanion(
            session,
            plan,
            processSupervisor,
            NpmCommandResolver.Resolve,
            TailwindWatchRestartPolicy.Default,
            logger);
        companion.Start();
        return companion;
    }
}

// Runs the package's own npm watch script next to a dotnet watch session. Tailwind rebuilds on its own;
// this companion only keeps the script running, holds its stdin open (Tailwind's --watch exits on stdin
// EOF) and reports whether builds keep completing.
internal sealed class TailwindSessionCompanion : IAsyncDisposable
{
    private const string OwnerKind = "tailwind";
    private readonly AppSession session;
    private readonly TailwindWorkspacePlan plan;
    private readonly ProcessSupervisor processSupervisor;
    private readonly Func<IReadOnlyList<string>, NpmCommand> resolveNpmCommand;
    private readonly TailwindWatchRestartPolicy restartPolicy;
    private readonly ILogger logger;
    private readonly CancellationTokenSource stopping = new();
    private readonly RingLogBuffer processLog = new(200);
    private readonly object gate = new();
    private ManagedProcess? activeProcess;
    private Task? supervisionTask;
    private TailwindWatchStatusData status;
    private int disposed;

    public TailwindSessionCompanion(
        AppSession session,
        TailwindWorkspacePlan plan,
        ProcessSupervisor processSupervisor,
        Func<IReadOnlyList<string>, NpmCommand> resolveNpmCommand,
        TailwindWatchRestartPolicy restartPolicy,
        ILogger logger)
    {
        this.session = session;
        this.plan = plan;
        this.processSupervisor = processSupervisor;
        this.resolveNpmCommand = resolveNpmCommand;
        this.restartPolicy = restartPolicy;
        this.logger = logger;
        status = new TailwindWatchStatusData(
            TailwindWatchState.Starting,
            $"Detected Tailwind package at {plan.PackageDirectory}; starting '{WatchCommand}'.",
            plan.PackageDirectory,
            plan.ScriptName,
            ProcessId: null,
            StartedUtc: null,
            LastBuildUtc: null,
            LastBuildDuration: null,
            BuildCount: 0,
            RestartCount: 0,
            LastError: null);
    }

    public TailwindWatchStatusData Status
    {
        get
        {
            lock (gate)
            {
                return status;
            }
        }
    }

    private string WatchCommand => $"npm run {plan.ScriptName}";

    public void Start()
    {
        var cliPath = TailwindWorkspaceDetector.ResolveTailwindCliPath(plan.PackageDirectory);
        if (!File.Exists(cliPath))
        {
            SetStatus(current => current with
            {
                State = TailwindWatchState.Unavailable,
                Summary = $"Tailwind dependencies are not installed: {cliPath} is missing. Run 'npm install' in {plan.PackageDirectory}, then restart the app session."
            });
            return;
        }

        SetStatus(current => current);
        supervisionTask = Task.Run(() => SuperviseUntilStoppedAsync(stopping.Token), CancellationToken.None);
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0)
        {
            return;
        }

        await stopping.CancelAsync();
        if (supervisionTask is not null)
        {
            await supervisionTask;
        }

        ManagedProcess? process;
        lock (gate)
        {
            process = activeProcess;
            activeProcess = null;
        }

        if (process is not null)
        {
            try
            {
                await process.StopAsync(force: true, CancellationToken.None);
            }
            catch (Exception exception) when (exception is InvalidOperationException or Win32Exception)
            {
                logger.LogWarning(exception, "Tailwind watch for session {SessionId} could not be stopped cleanly.", session.SessionId);
            }
        }

        SetStatus(current => current with
        {
            State = TailwindWatchState.Stopped,
            Summary = $"'{WatchCommand}' stopped with its app session.",
            ProcessId = null
        });
        stopping.Dispose();
    }

    // The companion must never take its app session down: an unexpected failure is logged and reported
    // as Unavailable, and stopping the session still stops the watch process.
    private async Task SuperviseUntilStoppedAsync(CancellationToken cancellationToken)
    {
        try
        {
            await SuperviseAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Tailwind watch supervision failed for session {SessionId}.", session.SessionId);
            SetStatus(current => current with
            {
                State = TailwindWatchState.Unavailable,
                Summary = $"Tailwind watch supervision failed: {exception.Message}"
            });
        }
    }

    private async Task SuperviseAsync(CancellationToken cancellationToken)
    {
        TimeSpan? restartDelay = null;
        var restartCount = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            var tracker = new TailwindBuildCycleTracker();
            var startedUtc = DateTimeOffset.UtcNow;
            SetStatus(current => current with
            {
                State = TailwindWatchState.Starting,
                Summary = $"Starting '{WatchCommand}' in {plan.PackageDirectory}; waiting for the first Tailwind build.",
                ProcessId = null,
                StartedUtc = startedUtc,
                LastBuildUtc = null,
                LastBuildDuration = null,
                BuildCount = 0,
                RestartCount = restartCount
            });
            ManagedProcess process;
            try
            {
                var command = resolveNpmCommand(["run", plan.ScriptName]);
                process = await processSupervisor.StartAsync(
                    new ManagedProcessStartInfo(
                        OwnerKind,
                        session.SessionId,
                        command.Command,
                        command.Arguments,
                        plan.PackageDirectory,
                        new Dictionary<string, string>
                        {
                            ["NO_COLOR"] = "1",
                            ["npm_config_update_notifier"] = "false"
                        },
                        session.CorrelationId,
                        session.SessionVersion),
                    processLog,
                    entry => ObserveProcessLogAsync(entry, tracker),
                    onExitAsync: null,
                    CancellationToken.None);
            }
            catch (Exception exception) when (exception is InvalidOperationException or Win32Exception or IOException or ToolInvocationException)
            {
                logger.LogWarning(exception, "Tailwind watch for session {SessionId} could not be started.", session.SessionId);
                SetStatus(current => current with
                {
                    State = TailwindWatchState.Unavailable,
                    Summary = $"'{WatchCommand}' could not be started in {plan.PackageDirectory}: {exception.Message}",
                    ProcessId = null
                });
                return;
            }

            lock (gate)
            {
                activeProcess = process;
                SetStatusLocked(status with { ProcessId = process.Pid });
            }

            int? exitCode;
            try
            {
                exitCode = await process.Completion.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            restartDelay = restartPolicy.NextDelay(restartDelay, DateTimeOffset.UtcNow - startedUtc);
            restartCount++;
            lock (gate)
            {
                activeProcess = null;
                var exitDescription = exitCode?.ToString(CultureInfo.InvariantCulture) ?? "unknown";
                SetStatusLocked(status with
                {
                    State = TailwindWatchState.Restarting,
                    Summary = $"{WithDetail($"'{WatchCommand}' exited with code {exitDescription}", tracker.LastError)}. Restarting in {FormatDuration(restartDelay.Value)}.",
                    ProcessId = null,
                    LastError = tracker.LastError ?? status.LastError
                });
            }

            try
            {
                await Task.Delay(restartDelay.Value, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private Task ObserveProcessLogAsync(LogEntry entry, TailwindBuildCycleTracker tracker)
    {
        var line = TailwindWatchOutputParser.Parse(entry.Text);
        if (line.Text.Length == 0)
        {
            return Task.CompletedTask;
        }

        session.LogBuffer.Append("Tailwind", "tailwind", session.SessionVersion, session.CorrelationId, $"[tailwind] {line.Text}");
        if (entry.Source is not ("ProcessStdOut" or "ProcessStdErr"))
        {
            return Task.CompletedTask;
        }

        lock (gate)
        {
            switch (line.Kind)
            {
                case TailwindWatchLineKind.Error:
                    if (tracker.ObserveError(line))
                    {
                        SetStatusLocked(status with
                        {
                            State = TailwindWatchState.BuildFailed,
                            Summary = $"{WithDetail("Tailwind reported an error", tracker.LastError)}.",
                            LastError = tracker.LastError
                        });
                    }

                    break;
                case TailwindWatchLineKind.BuildCompleted:
                    var failedWith = tracker.LastError;
                    SetStatusLocked(tracker.CompleteCycle() == TailwindBuildCycleOutcome.Failed
                        ? status with
                        {
                            State = TailwindWatchState.BuildFailed,
                            Summary = $"{WithDetail("Tailwind build failed", failedWith)}. '{WatchCommand}' keeps running and recovers on the next successful build.",
                            LastError = failedWith
                        }
                        : status with
                        {
                            State = TailwindWatchState.Ready,
                            Summary = $"'{WatchCommand}' is running; last build finished in {line.Duration}.",
                            LastBuildUtc = entry.TimestampUtc,
                            LastBuildDuration = line.Duration,
                            BuildCount = status.BuildCount + 1,
                            LastError = null
                        });
                    break;
            }
        }

        return Task.CompletedTask;
    }

    private void SetStatus(Func<TailwindWatchStatusData, TailwindWatchStatusData> update)
    {
        lock (gate)
        {
            SetStatusLocked(update(status));
        }
    }

    private void SetStatusLocked(TailwindWatchStatusData next)
    {
        if (next.State != status.State)
        {
            logger.LogInformation(
                "[tailwind:{SessionId}] {State}: {Summary}",
                session.SessionId,
                next.State,
                next.Summary);
        }

        status = next;
        session.UpdateTailwindWatch(next);
    }

    private static string WithDetail(string message, string? detail)
        => string.IsNullOrWhiteSpace(detail) ? message : $"{message}: {detail.TrimEnd('.')}";

    private static string FormatDuration(TimeSpan duration)
        => duration.TotalSeconds < 60
            ? $"{duration.TotalSeconds.ToString("0.#", CultureInfo.InvariantCulture)} s"
            : $"{duration.TotalMinutes.ToString("0.#", CultureInfo.InvariantCulture)} min";
}

internal static partial class TailwindWorkspaceDetector
{
    private static readonly HashSet<string> IgnoredDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        ".artifacts",
        ".git",
        ".idea",
        ".mcp-state",
        ".vs",
        "bin",
        "node_modules",
        "obj"
    };

    public static TailwindWorkspacePlan? TryDetect(string projectPath, string workingDirectory, string preferredScriptName)
    {
        var projectDirectory = Path.GetDirectoryName(projectPath);
        if (string.IsNullOrWhiteSpace(projectDirectory))
        {
            projectDirectory = workingDirectory;
        }

        if (string.IsNullOrWhiteSpace(projectDirectory) || !Directory.Exists(projectDirectory))
        {
            return null;
        }

        var repoRoot = ResolveSearchRoot(projectDirectory);
        return EnumeratePackageJsonFiles(repoRoot)
            .Select(packageJsonPath => TryCreatePlan(packageJsonPath, repoRoot, projectDirectory, preferredScriptName))
            .Where(static plan => plan is not null)
            .Cast<TailwindWorkspacePlan>()
            .OrderBy(plan => MeasurePathDistance(projectDirectory, plan.PackageDirectory))
            .ThenBy(plan => plan.PackageDirectory.Length)
            .FirstOrDefault();
    }

    public static string ResolveTailwindCliPath(string packageDirectory)
    {
        return Path.Combine(
            packageDirectory,
            "node_modules",
            ".bin",
            OperatingSystem.IsWindows() ? "tailwindcss.cmd" : "tailwindcss");
    }

    // The configured script (conventionally "watch") wins when it runs a Tailwind watch directly or
    // through other scripts of the same package, for example via concurrently. Otherwise another
    // Tailwind watch script is chosen, preferring one whose output lands in the project being run.
    internal static bool TrySelectWatchScript(
        IReadOnlyDictionary<string, string> scripts,
        string packageDirectory,
        string projectDirectory,
        string preferredScriptName,
        out string scriptName)
    {
        scriptName = string.Empty;
        if (TailwindWatchScriptName.IsSupported(preferredScriptName) &&
            RunsTailwindWatch(preferredScriptName, scripts, new HashSet<string>(StringComparer.Ordinal)))
        {
            scriptName = preferredScriptName;
            return true;
        }

        var candidate = scripts.Keys
            .Where(TailwindWatchScriptName.IsSupported)
            .Where(name => RunsTailwindWatch(name, scripts, new HashSet<string>(StringComparer.Ordinal)))
            .OrderByDescending(name => WritesUnder(scripts[name], packageDirectory, projectDirectory))
            .ThenByDescending(name => name.Contains("watch", StringComparison.OrdinalIgnoreCase))
            .ThenBy(name => name, StringComparer.Ordinal)
            .FirstOrDefault();
        if (candidate is null)
        {
            return false;
        }

        scriptName = candidate;
        return true;
    }

    private static TailwindWorkspacePlan? TryCreatePlan(
        string packageJsonPath,
        string repoRoot,
        string projectDirectory,
        string preferredScriptName)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(File.ReadAllText(packageJsonPath));
        }
        catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
        {
            return null;
        }

        using (document)
        {
            var packageDirectory = Path.GetDirectoryName(packageJsonPath);
            if (string.IsNullOrWhiteSpace(packageDirectory) ||
                !HasTailwindDependency(document.RootElement) ||
                !TryReadScripts(document.RootElement, out var scripts) ||
                !TrySelectWatchScript(scripts, packageDirectory, projectDirectory, preferredScriptName, out var scriptName))
            {
                return null;
            }

            return new TailwindWorkspacePlan(
                repoRoot,
                packageDirectory,
                packageJsonPath,
                scriptName,
                scripts[scriptName]);
        }
    }

    private static bool RunsTailwindWatch(
        string scriptName,
        IReadOnlyDictionary<string, string> scripts,
        HashSet<string> visited)
    {
        if (!visited.Add(scriptName) || !scripts.TryGetValue(scriptName, out var command))
        {
            return false;
        }

        if (InvokesTailwind(command) && WatchFlagRegex().IsMatch(command))
        {
            return true;
        }

        return ScriptReferenceRegex()
            .Matches(command)
            .Any(match => RunsTailwindWatch(match.Groups["name"].Value, scripts, visited));
    }

    private static bool InvokesTailwind(string command)
        => command.Contains("tailwindcss", StringComparison.OrdinalIgnoreCase) ||
           command.Contains("@tailwindcss/cli", StringComparison.OrdinalIgnoreCase);

    private static bool WritesUnder(string command, string packageDirectory, string projectDirectory)
    {
        var output = OutputArgumentRegex().Match(command);
        if (!output.Success)
        {
            return false;
        }

        var outputPath = output.Groups["value"].Value.Trim().Trim('"', '\'');
        return !string.IsNullOrWhiteSpace(outputPath) &&
               IsPathUnderRoot(ResolvePath(packageDirectory, outputPath), projectDirectory);
    }

    private static bool TryReadScripts(JsonElement root, out IReadOnlyDictionary<string, string> scripts)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        scripts = values;
        if (!root.TryGetProperty("scripts", out var scriptElement) || scriptElement.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        foreach (var property in scriptElement.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(property.Value.GetString()))
            {
                values[property.Name] = property.Value.GetString()!;
            }
        }

        return values.Count > 0;
    }

    private static bool HasTailwindDependency(JsonElement root)
    {
        return DependencySetContains(root, "dependencies", "@tailwindcss/cli") ||
               DependencySetContains(root, "dependencies", "tailwindcss") ||
               DependencySetContains(root, "devDependencies", "@tailwindcss/cli") ||
               DependencySetContains(root, "devDependencies", "tailwindcss");
    }

    private static bool DependencySetContains(JsonElement root, string propertyName, string dependencyName)
    {
        return root.TryGetProperty(propertyName, out var dependencies) &&
               dependencies.ValueKind == JsonValueKind.Object &&
               dependencies.TryGetProperty(dependencyName, out _);
    }

    private static IEnumerable<string> EnumeratePackageJsonFiles(string repoRoot)
    {
        var pending = new Stack<string>();
        pending.Push(repoRoot);

        while (pending.Count > 0)
        {
            var directory = pending.Pop();
            IEnumerable<string> packageJsonFiles;
            try
            {
                packageJsonFiles = Directory.EnumerateFiles(directory, "package.json", SearchOption.TopDirectoryOnly);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                continue;
            }

            foreach (var packageJsonFile in packageJsonFiles)
            {
                yield return packageJsonFile;
            }

            IEnumerable<string> childDirectories;
            try
            {
                childDirectories = Directory.EnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                continue;
            }

            foreach (var childDirectory in childDirectories)
            {
                var directoryName = Path.GetFileName(childDirectory);
                if (!string.IsNullOrWhiteSpace(directoryName) && !IgnoredDirectories.Contains(directoryName))
                {
                    pending.Push(childDirectory);
                }
            }
        }
    }

    private static string ResolveSearchRoot(string projectDirectory)
    {
        var current = Path.GetFullPath(projectDirectory);
        var bestMarkerDirectory = current;

        while (!string.IsNullOrWhiteSpace(current))
        {
            if (HasRepoMarker(current))
            {
                bestMarkerDirectory = current;
            }

            var parent = Directory.GetParent(current)?.FullName;
            if (string.IsNullOrWhiteSpace(parent) || string.Equals(parent, current, StringComparison.OrdinalIgnoreCase))
            {
                return bestMarkerDirectory;
            }

            current = parent;
        }

        return bestMarkerDirectory;
    }

    private static bool HasRepoMarker(string directory)
    {
        if (Directory.Exists(Path.Combine(directory, ".git")) ||
            File.Exists(Path.Combine(directory, "Directory.Build.props")) ||
            File.Exists(Path.Combine(directory, "Directory.Build.targets")) ||
            File.Exists(Path.Combine(directory, "package.json")))
        {
            return true;
        }

        try
        {
            return Directory.EnumerateFiles(directory, "*.sln", SearchOption.TopDirectoryOnly).Any() ||
                   Directory.EnumerateFiles(directory, "*.slnx", SearchOption.TopDirectoryOnly).Any();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static int MeasurePathDistance(string fromDirectory, string toDirectory)
    {
        var relativePath = Path.GetRelativePath(fromDirectory, toDirectory);
        return relativePath.Split(
                [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                StringSplitOptions.RemoveEmptyEntries)
            .Count(segment => !string.Equals(segment, ".", StringComparison.Ordinal));
    }

    private static bool IsPathUnderRoot(string path, string root)
    {
        var normalizedRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root)) + Path.DirectorySeparatorChar;
        var normalizedPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path)) + Path.DirectorySeparatorChar;
        return normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolvePath(string baseDirectory, string path)
    {
        return Path.IsPathRooted(path)
            ? Path.GetFullPath(path)
            : Path.GetFullPath(Path.Combine(baseDirectory, path));
    }

    [GeneratedRegex(@"(?:^|\s)(?:--watch|-w)(?:=\S*)?(?=\s|$)")]
    private static partial Regex WatchFlagRegex();

    [GeneratedRegex(@"\b(?:npm|pnpm|yarn)\s+run(?:-script)?\s+(?<name>[A-Za-z0-9:._-]+)")]
    private static partial Regex ScriptReferenceRegex();

    [GeneratedRegex(@"(?:^|\s)(?:-o|--output)(?:\s+|=)(?<value>""[^""]+""|'[^']+'|\S+)")]
    private static partial Regex OutputArgumentRegex();
}

internal static class NpmCommandResolver
{
    public static NpmCommand Resolve(IReadOnlyList<string> npmArguments)
        => Resolve(npmArguments, Environment.GetEnvironmentVariable("PATH"));

    // npm runs through node and its JavaScript entry point when both are available, so no batch
    // launcher sits between the backend and npm; npm still runs the script through its script shell.
    internal static NpmCommand Resolve(IReadOnlyList<string> npmArguments, string? pathVariable)
    {
        var npmPath = FindOnPath(pathVariable, OperatingSystem.IsWindows() ? "npm.cmd" : "npm")
            ?? throw new InvalidOperationException("npm was not found on the backend PATH.");
        var nodePath = FindOnPath(pathVariable, OperatingSystem.IsWindows() ? "node.exe" : "node");
        var npmCliPath = TryResolveNpmCliScriptPath(npmPath);
        return nodePath is not null && npmCliPath is not null
            ? new NpmCommand(nodePath, [npmCliPath, .. npmArguments])
            : new NpmCommand(npmPath, npmArguments);
    }

    private static string? FindOnPath(string? pathVariable, string fileName)
    {
        if (string.IsNullOrWhiteSpace(pathVariable))
        {
            return null;
        }

        foreach (var entry in pathVariable.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var directory = entry.Trim('"');
            if (directory.Length == 0 || !Path.IsPathFullyQualified(directory))
            {
                continue;
            }

            var candidate = Path.Combine(directory, fileName);
            if (File.Exists(candidate))
            {
                return Path.GetFullPath(candidate);
            }
        }

        return null;
    }

    private static string? TryResolveNpmCliScriptPath(string npmPath)
    {
        if (IsJavaScriptFile(npmPath))
        {
            return npmPath;
        }

        var linkTarget = TryResolveFinalLinkTarget(npmPath);
        if (linkTarget is not null && IsJavaScriptFile(linkTarget) && File.Exists(linkTarget))
        {
            return linkTarget;
        }

        var npmDirectory = Path.GetDirectoryName(npmPath) ?? string.Empty;
        string[] candidates =
        [
            Path.Combine(npmDirectory, "node_modules", "npm", "bin", "npm-cli.js"),
            Path.Combine(npmDirectory, "..", "lib", "node_modules", "npm", "bin", "npm-cli.js")
        ];
        return candidates
            .Select(Path.GetFullPath)
            .FirstOrDefault(File.Exists);
    }

    private static string? TryResolveFinalLinkTarget(string path)
    {
        try
        {
            return File.ResolveLinkTarget(path, returnFinalTarget: true)?.FullName;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static bool IsJavaScriptFile(string path)
        => string.Equals(Path.GetExtension(path), ".js", StringComparison.OrdinalIgnoreCase);
}
