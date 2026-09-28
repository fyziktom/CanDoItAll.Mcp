using System.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using CanDoItAll.Mcp.Core.Identity;
using CanDoItAll.Mcp.Core.Observability;
using CanDoItAll.Mcp.DotNetWatch.Configuration;
using CanDoItAll.Mcp.DotNetWatch.Runtime;
using CanDoItAll.Mcp.LocalRuntime.Persistence;
using CanDoItAll.Mcp.LocalRuntime.Processes;

namespace CanDoItAll.Mcp.DotNetWatch.Tests;

public sealed class TailwindCompanionTests
{
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(20);

    [Fact]
    public void BuildDefaultEnvironment_DoesNotSuppressBrowserRefresh_WhenDisabled()
    {
        var template = new AppStartTemplate(
            @"C:\repo\App.csproj",
            @"C:\repo",
            AppRunMode.WatchRun,
            "Debug",
            Framework: null,
            LaunchProfile: null,
            Arguments: [],
            EnvironmentOverlay: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            Urls: ["http://127.0.0.1:5032"]);

        var environment = AppRuntimeManager.BuildDefaultEnvironment(template, usePollingWatcher: false, suppressBrowserRefresh: false);

        Assert.DoesNotContain("DOTNET_WATCH_SUPPRESS_BROWSER_REFRESH", environment.Keys, StringComparer.OrdinalIgnoreCase);
        Assert.Equal("http://127.0.0.1:5032", environment["ASPNETCORE_URLS"]);
    }

    [Fact]
    public void BuildDefaultEnvironment_SuppressesBrowserRefresh_WhenEnabled()
    {
        var template = new AppStartTemplate(
            @"C:\repo\App.csproj",
            @"C:\repo",
            AppRunMode.WatchRun,
            "Debug",
            Framework: null,
            LaunchProfile: null,
            Arguments: [],
            EnvironmentOverlay: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            Urls: []);

        var environment = AppRuntimeManager.BuildDefaultEnvironment(template, usePollingWatcher: true, suppressBrowserRefresh: true);

        Assert.Equal("1", environment["DOTNET_WATCH_SUPPRESS_BROWSER_REFRESH"]);
        Assert.Equal("1", environment["DOTNET_USE_POLLING_FILE_WATCHER"]);
    }

    [Fact]
    public void TailwindWorkspaceDetector_SelectsTheConfiguredWatchScript()
    {
        using var workspace = CreateSampleWorkspace(
            """
            {
              "build": "tailwindcss -i ./input.css -o ../src/SampleApp/wwwroot/css/output.css",
              "watch": "tailwindcss -i ./input.css -o ../src/SampleApp/wwwroot/css/output.css --watch"
            }
            """);

        var plan = DetectForSampleApp(workspace);

        Assert.NotNull(plan);
        Assert.Equal(Path.Combine(workspace.RootPath, "Tailwind"), plan!.PackageDirectory);
        Assert.Equal("watch", plan.ScriptName);
        Assert.Contains("--watch", plan.ScriptCommand, StringComparison.Ordinal);
    }

    [Fact]
    public void TailwindWorkspaceDetector_AcceptsAWatchScriptThatRunsTailwindThroughOtherScripts()
    {
        using var workspace = CreateSampleWorkspace(
            """
            {
              "build": "npm run build:baselib",
              "build:baselib": "tailwindcss -i ./input-base.css -o ../src/BaseLib/wwwroot/css/output.css",
              "watch": "concurrently 'npm run watch:baselib' 'npm run watch:sandbox'",
              "watch:baselib": "tailwindcss  --watch=always -i ./input-base.css -o ../src/BaseLib/wwwroot/css/output.css",
              "watch:sandbox": "tailwindcss  --watch=always -i ./input-sandbox.css -o ../src/SampleApp/wwwroot/css/output.css"
            }
            """);

        var plan = DetectForSampleApp(workspace);

        Assert.NotNull(plan);
        Assert.Equal("watch", plan!.ScriptName);
    }

    [Fact]
    public void TailwindWorkspaceDetector_SkipsAWatchScriptWithoutTailwind_AndPrefersOutputInTheProject()
    {
        using var workspace = CreateSampleWorkspace(
            """
            {
              "watch": "dotnet watch --project ../src/SampleApp",
              "css:watch": "tailwindcss -i ./other.css -o ../src/OtherApp/wwwroot/css/output.css --watch",
              "sample:watch": "tailwindcss -i ./input.css -o ../src/SampleApp/wwwroot/css/sample.css -w"
            }
            """);

        var plan = DetectForSampleApp(workspace);

        Assert.NotNull(plan);
        Assert.Equal("sample:watch", plan!.ScriptName);
    }

    [Fact]
    public void TailwindWorkspaceDetector_ReturnsNull_WhenTheTailwindPackageHasNoWatchScript()
    {
        using var workspace = CreateSampleWorkspace(
            """
            {
              "build": "tailwindcss -i ./input.css -o ../src/SampleApp/wwwroot/css/output.css"
            }
            """);

        Assert.Null(DetectForSampleApp(workspace));
    }

    [Fact]
    public void TailwindWorkspaceDetector_ReturnsNull_WhenNoTailwindWorkspaceExists()
    {
        using var workspace = new TailwindWorkspace();
        workspace.WriteFile("CanDoItAll.slnx", "<Solution />");
        workspace.WriteFile(Path.Combine("src", "SampleApp", "SampleApp.csproj"), "<Project />");
        workspace.WriteFile(
            "package.json",
            """
            {
              "name": "plain-app",
              "private": true,
              "scripts": {
                "build": "vite build",
                "watch": "vite build --watch"
              }
            }
            """);

        Assert.Null(DetectForSampleApp(workspace));
    }

    [Theory]
    [InlineData("watch", true)]
    [InlineData("catalog:watch", true)]
    [InlineData("watch.css_fast-1", true)]
    [InlineData("", false)]
    [InlineData("watch & calc", false)]
    [InlineData("watch|more", false)]
    [InlineData("-watch", false)]
    public void TailwindWatchScriptName_AcceptsOnlyShellNeutralNames(string scriptName, bool expected)
    {
        Assert.Equal(expected, TailwindWatchScriptName.IsSupported(scriptName));
    }

    [Theory]
    [InlineData("\u001b[3m\u001b[1m\u001b[34m≈\u001b[39m\u001b[22m\u001b[23m tailwindcss \u001b[34mv4.3.3\u001b[39m", "Informational", "tailwindcss v4.3.3")]
    [InlineData("\r\u001b[2K\u001b[2mPolling for changes…\u001b[22m", "Informational", "Polling for changes…")]
    [InlineData("Done in \u001b[34m113ms\u001b[39m", "BuildCompleted", "Done in 113ms")]
    [InlineData("[0] Done in 5ms", "BuildCompleted", "[0] Done in 5ms")]
    [InlineData("\u001b[31mError:\u001b[39m", "Error", "Error:")]
    [InlineData("\u001b[2m│\u001b[22m CssSyntaxError: input.css:3:1: Missing closing } at .broken", "Error", "CssSyntaxError: input.css:3:1: Missing closing } at .broken")]
    [InlineData("Ôöé CssSyntaxError: input.css:3:1: Missing closing } at .broken", "Error", "CssSyntaxError: input.css:3:1: Missing closing } at .broken")]
    [InlineData("npm error Missing script: \"watch\"", "Error", "npm error Missing script: \"watch\"")]
    [InlineData("'tailwindcss' is not recognized as an internal or external command,", "Error", "'tailwindcss' is not recognized as an internal or external command,")]
    [InlineData("sh: 1: tailwindcss: not found", "Error", "sh: 1: tailwindcss: not found")]
    public void TailwindWatchOutputParser_ClassifiesTailwindWatchOutput(string rawLine, string expectedKind, string expectedText)
    {
        var line = TailwindWatchOutputParser.Parse(rawLine);

        Assert.Equal(expectedKind, line.Kind.ToString());
        Assert.Equal(expectedText, line.Text);
    }

    [Fact]
    public void TailwindBuildCycleTracker_FailsACycleThatReportedAnErrorBeforeDone()
    {
        var tracker = new TailwindBuildCycleTracker();

        Assert.True(tracker.ObserveError(TailwindWatchOutputParser.Parse("Error:")));
        Assert.True(tracker.ObserveError(TailwindWatchOutputParser.Parse("│ CssSyntaxError: broken")));
        Assert.Equal(TailwindBuildCycleOutcome.Failed, tracker.CompleteCycle());
        Assert.Equal("CssSyntaxError: broken", tracker.LastError);
        Assert.Equal(TailwindBuildCycleOutcome.Succeeded, tracker.CompleteCycle());
        Assert.Null(tracker.LastError);
    }

    [Fact]
    public void TailwindWatchRestartPolicy_BacksOffQuickFailuresAndResetsAfterAStableRun()
    {
        var policy = new TailwindWatchRestartPolicy(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(1));

        var first = policy.NextDelay(null, TimeSpan.FromSeconds(1));
        var second = policy.NextDelay(first, TimeSpan.FromSeconds(1));
        var capped = policy.NextDelay(second, TimeSpan.FromSeconds(1));

        Assert.Equal(TimeSpan.FromSeconds(2), first);
        Assert.Equal(TimeSpan.FromSeconds(4), second);
        Assert.Equal(TimeSpan.FromSeconds(5), capped);
        Assert.Equal(TimeSpan.FromSeconds(2), policy.NextDelay(capped, TimeSpan.FromMinutes(2)));
    }

    [Fact]
    public void NpmCommandResolver_RunsTheNpmCliThroughNode()
    {
        using var workspace = new TailwindWorkspace();
        var binDirectory = Path.Combine(workspace.RootPath, "nodejs");
        workspace.WriteFile(Path.Combine("nodejs", OperatingSystem.IsWindows() ? "npm.cmd" : "npm"), string.Empty);
        workspace.WriteFile(Path.Combine("nodejs", OperatingSystem.IsWindows() ? "node.exe" : "node"), string.Empty);
        workspace.WriteFile(Path.Combine("nodejs", "node_modules", "npm", "bin", "npm-cli.js"), string.Empty);

        var command = NpmCommandResolver.Resolve(["run", "watch"], binDirectory);

        Assert.Equal(Path.Combine(binDirectory, OperatingSystem.IsWindows() ? "node.exe" : "node"), command.Command);
        Assert.Equal([Path.Combine(binDirectory, "node_modules", "npm", "bin", "npm-cli.js"), "run", "watch"], command.Arguments);
    }

    [Fact]
    public void NpmCommandResolver_RunsNpmItselfWhenNodeOrItsCliCannotBeFound()
    {
        using var workspace = new TailwindWorkspace();
        var binDirectory = Path.Combine(workspace.RootPath, "nodejs");
        var npmPath = Path.Combine(binDirectory, OperatingSystem.IsWindows() ? "npm.cmd" : "npm");
        workspace.WriteFile(Path.Combine("nodejs", OperatingSystem.IsWindows() ? "npm.cmd" : "npm"), string.Empty);

        var command = NpmCommandResolver.Resolve(["run", "watch"], binDirectory);

        Assert.Equal(npmPath, command.Command);
        Assert.Equal(["run", "watch"], command.Arguments);
        Assert.Throws<InvalidOperationException>(() => NpmCommandResolver.Resolve(["run", "watch"], Path.Combine(workspace.RootPath, "missing")));
    }

    [Fact]
    public async Task Companion_KeepsTheWatchRunningAndRecoversFromAFailedBuild()
    {
        using var workspace = CreateSampleWorkspace(
            """
            {
              "watch": "tailwindcss -i ./input.css -o ../src/SampleApp/wwwroot/css/output.css --watch"
            }
            """);
        InstallTailwindCli(workspace);
        var session = CreateSession(workspace);
        var watch = CreateWatcherCommand(
            ["Done in 100ms", "Error:", "CssSyntaxError: input.css:3:1: Missing closing } at .broken", "Done in 4s", "Done in 5ms"],
            exitCode: null);
        await using var companion = CreateCompanion(workspace, session, _ => watch);

        companion.Start();
        var ready = await WaitForTailwindAsync(companion, status => status.State == TailwindWatchState.Ready && status.BuildCount == 2);
        await Task.Delay(TimeSpan.FromSeconds(1));
        var stillRunning = companion.Status;

        Assert.Equal("watch", ready.Script);
        Assert.NotNull(ready.ProcessId);
        Assert.Null(ready.LastError);
        Assert.Equal(TailwindWatchState.Ready, stillRunning.State);
        Assert.Equal(ready.ProcessId, stillRunning.ProcessId);
        Assert.True(IsRunning(ready.ProcessId!.Value));
        Assert.Contains(
            session.ToStatusData().RecentEvents,
            text => text.StartsWith("Tailwind watch BuildFailed", StringComparison.Ordinal) &&
                    text.Contains("CssSyntaxError", StringComparison.Ordinal));
        Assert.Contains(
            session.LogBuffer.GetAfter(0),
            entry => entry.Source == "Tailwind" && entry.Text.Contains("CssSyntaxError", StringComparison.Ordinal));
        Assert.Equal(TailwindWatchState.Ready, session.ToStatusData().Tailwind?.State);

        await companion.DisposeAsync();

        Assert.Equal(TailwindWatchState.Stopped, companion.Status.State);
        Assert.True(await WaitUntilAsync(() => !IsRunning(ready.ProcessId.Value)));
    }

    [Fact]
    public async Task Companion_RestartsTheWatchAfterItExitsAndReportsTheCause()
    {
        using var workspace = CreateSampleWorkspace(
            """
            {
              "watch": "tailwindcss -i ./input.css -o ../src/SampleApp/wwwroot/css/output.css --watch"
            }
            """);
        InstallTailwindCli(workspace);
        var session = CreateSession(workspace);
        var launches = 0;
        var failing = CreateWatcherCommand(["npm error Missing script: watch"], exitCode: 1);
        var healthy = CreateWatcherCommand(["Done in 90ms"], exitCode: null);
        await using var companion = CreateCompanion(
            workspace,
            session,
            _ => Interlocked.Increment(ref launches) == 1 ? failing : healthy,
            new TailwindWatchRestartPolicy(TimeSpan.FromMilliseconds(1500), TimeSpan.FromSeconds(2), TimeSpan.FromMinutes(1)));

        companion.Start();
        var restarting = await WaitForTailwindAsync(companion, status => status.State == TailwindWatchState.Restarting);
        var ready = await WaitForTailwindAsync(companion, status => status.State == TailwindWatchState.Ready);

        Assert.Contains("code 1", restarting.Summary, StringComparison.Ordinal);
        Assert.Contains("npm error Missing script: watch", restarting.Summary, StringComparison.Ordinal);
        Assert.Contains("Restarting in 1.5 s", restarting.Summary, StringComparison.Ordinal);
        Assert.Equal(1, ready.RestartCount);
        Assert.Equal(2, launches);
    }

    [Fact]
    public async Task Companion_ReportsMissingDependenciesWithoutStartingNpm()
    {
        using var workspace = CreateSampleWorkspace(
            """
            {
              "watch": "tailwindcss -i ./input.css -o ../src/SampleApp/wwwroot/css/output.css --watch"
            }
            """);
        var session = CreateSession(workspace);
        var launches = 0;
        await using var companion = CreateCompanion(
            workspace,
            session,
            _ =>
            {
                Interlocked.Increment(ref launches);
                return CreateWatcherCommand(["Done in 1ms"], exitCode: null);
            });

        companion.Start();

        Assert.Equal(TailwindWatchState.Unavailable, companion.Status.State);
        Assert.Contains("npm install", companion.Status.Summary, StringComparison.Ordinal);
        Assert.Equal(0, launches);
        Assert.Equal(TailwindWatchState.Unavailable, session.ToStatusData().Tailwind?.State);
    }

    private static TailwindWorkspace CreateSampleWorkspace(string scriptsJson)
    {
        var workspace = new TailwindWorkspace();
        workspace.WriteFile("CanDoItAll.slnx", "<Solution />");
        workspace.WriteFile(Path.Combine("src", "SampleApp", "SampleApp.csproj"), "<Project />");
        workspace.WriteFile(
            Path.Combine("Tailwind", "package.json"),
            $$"""
            {
              "name": "tailwind-app",
              "private": true,
              "devDependencies": {
                "@tailwindcss/cli": "^4.1.7",
                "tailwindcss": "^4.1.7"
              },
              "scripts": {{scriptsJson}}
            }
            """);
        workspace.WriteFile(Path.Combine("Tailwind", "input.css"), "@import \"tailwindcss\";");
        return workspace;
    }

    private static TailwindWorkspacePlan? DetectForSampleApp(TailwindWorkspace workspace)
        => TailwindWorkspaceDetector.TryDetect(
            Path.Combine(workspace.RootPath, "src", "SampleApp", "SampleApp.csproj"),
            Path.Combine(workspace.RootPath, "src", "SampleApp"),
            "watch");

    private static void InstallTailwindCli(TailwindWorkspace workspace)
        => workspace.WriteFile(
            Path.GetRelativePath(
                workspace.RootPath,
                TailwindWorkspaceDetector.ResolveTailwindCliPath(Path.Combine(workspace.RootPath, "Tailwind"))),
            string.Empty);

    private static AppSession CreateSession(TailwindWorkspace workspace)
    {
        var template = new AppStartTemplate(
            Path.Combine(workspace.RootPath, "src", "SampleApp", "SampleApp.csproj"),
            Path.Combine(workspace.RootPath, "src", "SampleApp"),
            AppRunMode.WatchRun,
            "Debug",
            Framework: null,
            LaunchProfile: null,
            Arguments: [],
            EnvironmentOverlay: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            Urls: []);
        return new AppSession($"app_{Guid.NewGuid():N}", template, $"corr_{Guid.NewGuid():N}", new RingLogBuffer(500), healthEnabled: false);
    }

    private static TailwindSessionCompanion CreateCompanion(
        TailwindWorkspace workspace,
        AppSession session,
        Func<IReadOnlyList<string>, NpmCommand> resolveNpmCommand,
        TailwindWatchRestartPolicy? restartPolicy = null)
    {
        var plan = DetectForSampleApp(workspace) ?? throw new InvalidOperationException("The sample workspace has no Tailwind watch script.");
        return new TailwindSessionCompanion(
            session,
            plan,
            CreateProcessSupervisor(workspace),
            resolveNpmCommand,
            restartPolicy ?? TailwindWatchRestartPolicy.Default,
            NullLogger.Instance);
    }

    private static ProcessSupervisor CreateProcessSupervisor(TailwindWorkspace workspace)
    {
        var options = new McpServerOptions
        {
            Server = new ServerOptions
            {
                WorkspaceRoot = workspace.RootPath,
                SolutionPath = "CanDoItAll.slnx"
            },
            DefaultApp = new DefaultAppOptions
            {
                ProjectPath = Path.Combine("src", "SampleApp", "SampleApp.csproj"),
                WorkingDirectory = Path.Combine("src", "SampleApp")
            },
            Build = new BuildOptions
            {
                DefaultTargetPath = "CanDoItAll.slnx"
            },
            Tests = new TestOptions
            {
                DefaultTargetPath = Path.Combine("src", "SampleApp", "SampleApp.csproj")
            },
            Logs = new LogOptions
            {
                Folder = ".mcp-state/logs"
            },
            Process = new ProcessOptions
            {
                RegistryPath = ".mcp-state/process-registry.json"
            },
            Security = new SecurityOptions
            {
                AllowedProjectRoots = ["src"],
                AllowedExternalProjectRoots = []
            }
        };
        var configuration = new RuntimeConfiguration(Options.Create(options));
        var runtimeOptions = configuration.CreateLocalProcessRuntimeOptions();
        var identity = new ServerInstanceIdentity();
        var commandRunner = new ProcessCommandRunner();
        IPlatformProcessTreeTerminator platformTerminator = OperatingSystem.IsWindows()
            ? new WindowsProcessTreeTerminator(commandRunner, NullLogger<WindowsProcessTreeTerminator>.Instance)
            : new UnixProcessTreeTerminator(commandRunner, NullLogger<UnixProcessTreeTerminator>.Instance);
        return new ProcessSupervisor(
            runtimeOptions,
            identity,
            new StaleProcessRegistry(
                runtimeOptions,
                identity,
                new ServerInstanceRegistry(runtimeOptions, identity, NullLogger<ServerInstanceRegistry>.Instance),
                commandRunner,
                NullLogger<StaleProcessRegistry>.Instance),
            new ProcessTreeTerminator(runtimeOptions, platformTerminator, NullLogger<ProcessTreeTerminator>.Instance),
            new FileLogStore(configuration.CreateFileLogStoreOptions()),
            new SecretRedactor(configuration.CreateSecretRedactionOptions()),
            NullLogger<ProcessSupervisor>.Instance);
    }

    // Stands in for "npm run watch": prints Tailwind-style lines to stderr, then either exits or, like
    // tailwindcss --watch, keeps running until its standard input is closed.
    private static NpmCommand CreateWatcherCommand(IReadOnlyList<string> standardErrorLines, int? exitCode)
    {
        if (OperatingSystem.IsWindows())
        {
            var echoes = string.Join(" & ", standardErrorLines.Select(line => $"echo {line} 1>&2"));
            var ending = exitCode is { } code ? $"exit {code}" : "findstr unmatched-line-from-closed-input";
            return new NpmCommand("cmd.exe", ["/d", "/c", $"{echoes} & {ending}"]);
        }

        var prints = string.Join("; ", standardErrorLines.Select(line => $"printf '%s\\n' '{line.Replace("'", "'\\''", StringComparison.Ordinal)}' >&2"));
        var unixEnding = exitCode is { } unixCode ? $"exit {unixCode}" : "cat > /dev/null";
        return new NpmCommand("/bin/sh", ["-c", $"{prints}; {unixEnding}"]);
    }

    private static async Task<TailwindWatchStatusData> WaitForTailwindAsync(
        TailwindSessionCompanion companion,
        Func<TailwindWatchStatusData, bool> predicate)
    {
        var deadline = DateTimeOffset.UtcNow.Add(WaitTimeout);
        while (DateTimeOffset.UtcNow < deadline)
        {
            var status = companion.Status;
            if (predicate(status))
            {
                return status;
            }

            await Task.Delay(20);
        }

        var last = companion.Status;
        throw new TimeoutException($"Tailwind companion did not reach the expected state. Last state: {last.State}; summary: {last.Summary}");
    }

    private static async Task<bool> WaitUntilAsync(Func<bool> condition)
    {
        var deadline = DateTimeOffset.UtcNow.Add(WaitTimeout);
        while (DateTimeOffset.UtcNow < deadline)
        {
            if (condition())
            {
                return true;
            }

            await Task.Delay(50);
        }

        return condition();
    }

    private static bool IsRunning(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            return !process.HasExited;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private sealed class TailwindWorkspace : IDisposable
    {
        public TailwindWorkspace()
        {
            RootPath = Path.Combine(Path.GetTempPath(), "CanDoItAll.Mcp.DotNetWatch.TailwindTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(RootPath);
            Directory.CreateDirectory(Path.Combine(RootPath, ".git"));
        }

        public string RootPath { get; }

        public void Dispose()
        {
            if (Directory.Exists(RootPath))
            {
                Directory.Delete(RootPath, recursive: true);
            }
        }

        public void WriteFile(string relativePath, string content)
        {
            var fullPath = Path.Combine(RootPath, relativePath);
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(fullPath, content);
        }
    }
}
