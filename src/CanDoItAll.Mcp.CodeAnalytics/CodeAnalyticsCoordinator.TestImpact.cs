using CanDoItAll.CodeAnalytics.Abstractions.Queries;
using CanDoItAll.Mcp.Core.Contracts;

namespace CanDoItAll.Mcp.CodeAnalytics;

public sealed partial class CodeAnalyticsCoordinator
{
    public async Task<CodeAnalyticsImpactedTestsResult> GetImpactedTestsAsync(
        CodeAnalyticsImpactedTestsInput request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var repositoryRootPath = ResolveRepositoryRoot(request.RepositoryRootPath);
        var testWorkspaces = NormalizeTestWorkspaces(request.TestWorkspaces, repositoryRootPath);
        var changes = NormalizeChanges(request.Changes, repositoryRootPath);
        var contextOnlyPaths = NormalizeContextOnlyPaths(request.ContextOnlyPaths, repositoryRootPath);

        var changedPaths = changes
            .Select(static change => change.Path)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var conflictingPath = contextOnlyPaths.FirstOrDefault(changedPaths.Contains);
        if (conflictingPath is not null)
        {
            throw new ToolInvocationException(
                "ValidationError",
                $"Path '{conflictingPath}' cannot be both an actual change and context-only input.");
        }

        ValidateLimit(request.MaxVisitedMembers, 1, 100_000, nameof(request.MaxVisitedMembers));
        ValidateLimit(request.MaxSelectors, 1, 500, nameof(request.MaxSelectors));
        ValidateLimit(request.MaxReasonPaths, 1, 200, nameof(request.MaxReasonPaths));

        var query = new TestImpactQuery(
            repositoryRootPath,
            testWorkspaces,
            changes,
            request.MaxVisitedMembers,
            request.MaxSelectors,
            request.MaxReasonPaths);
        var response = await testImpactAnalysisService.AnalyzeAsync(query, cancellationToken);

        return new CodeAnalyticsImpactedTestsResult(response, contextOnlyPaths);
    }

    private string ResolveRepositoryRoot(string? candidatePath)
    {
        var repositoryRootPath = string.IsNullOrWhiteSpace(candidatePath)
            ? runtimeConfiguration.RepositoryRoot
            : ResolvePath(candidatePath, runtimeConfiguration.RepositoryRoot);

        if (!Directory.Exists(repositoryRootPath))
        {
            throw new ToolInvocationException(
                "RepositoryRootNotFound",
                $"Repository root '{repositoryRootPath}' was not found.");
        }

        return repositoryRootPath;
    }

    private static IReadOnlyList<TestWorkspaceInput> NormalizeTestWorkspaces(
        IReadOnlyList<CodeAnalyticsTestWorkspaceInput>? workspaces,
        string repositoryRootPath)
    {
        if (workspaces is null || workspaces.Count == 0)
        {
            throw new ToolInvocationException(
                "ValidationError",
                "Impacted-test analysis requires at least one test workspace. Supply each relevant test .sln, .slnx, or .csproj, including suites outside the production solution.");
        }

        List<TestWorkspaceInput> normalized = [];
        HashSet<string> seenPaths = new(StringComparer.OrdinalIgnoreCase);
        foreach (var workspace in workspaces)
        {
            if (workspace is null)
            {
                throw new ToolInvocationException("ValidationError", "Test workspace entries cannot be null.");
            }

            var path = ResolvePath(RequireValue(workspace.Path, "TestWorkspaces.Path"), repositoryRootPath);
            if (!File.Exists(path))
            {
                throw new ToolInvocationException(
                    "TestWorkspacePathNotFound",
                    $"Test workspace '{path}' was not found.");
            }

            var extension = Path.GetExtension(path);
            if (!string.Equals(extension, ".sln", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(extension, ".slnx", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(extension, ".csproj", StringComparison.OrdinalIgnoreCase))
            {
                throw new ToolInvocationException(
                    "ValidationError",
                    $"Test workspace '{path}' must be a .sln, .slnx, or .csproj file.");
            }

            if (seenPaths.Add(path))
            {
                normalized.Add(new TestWorkspaceInput(path, NormalizeOptionalValue(workspace.Name)));
            }
        }

        return normalized;
    }

    private static IReadOnlyList<ChangedCodeInput> NormalizeChanges(
        IReadOnlyList<CodeAnalyticsChangedCodeInput>? changes,
        string repositoryRootPath)
    {
        if (changes is null || changes.Count == 0)
        {
            throw new ToolInvocationException(
                "ValidationError",
                "Impacted-test analysis requires at least one actual changed file from the diff. Context-only helpers are not change seeds.");
        }

        List<ChangedCodeInput> normalized = [];
        foreach (var change in changes)
        {
            if (change is null)
            {
                throw new ToolInvocationException("ValidationError", "Changed-code entries cannot be null.");
            }

            var path = ResolvePath(RequireValue(change.Path, "Changes.Path"), repositoryRootPath);
            var lineRanges = NormalizeLineRanges(change.LineRanges, path);
            normalized.Add(new ChangedCodeInput(path, lineRanges, change.BehaviorIntent));
        }

        return normalized;
    }

    private static IReadOnlyList<ChangedLineRange> NormalizeLineRanges(
        IReadOnlyList<CodeAnalyticsChangedLineRangeInput>? lineRanges,
        string changedPath)
    {
        if (lineRanges is null || lineRanges.Count == 0)
        {
            return [];
        }

        List<ChangedLineRange> normalized = [];
        foreach (var lineRange in lineRanges)
        {
            if (lineRange is null || lineRange.StartLine < 1 || lineRange.EndLine < lineRange.StartLine)
            {
                throw new ToolInvocationException(
                    "ValidationError",
                    $"Changed line ranges for '{changedPath}' must use one-based inclusive lines with EndLine greater than or equal to StartLine.");
            }

            normalized.Add(new ChangedLineRange(lineRange.StartLine, lineRange.EndLine));
        }

        return normalized;
    }

    private static IReadOnlyList<string> NormalizeContextOnlyPaths(
        IReadOnlyList<string>? paths,
        string repositoryRootPath)
    {
        if (paths is null || paths.Count == 0)
        {
            return [];
        }

        return paths
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .Select(path => ResolvePath(path.Trim(), repositoryRootPath))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string ResolvePath(string path, string repositoryRootPath)
    {
        return Path.GetFullPath(
            Path.IsPathRooted(path)
                ? path
                : Path.Combine(repositoryRootPath, path));
    }

    private static void ValidateLimit(int value, int minimum, int maximum, string parameterName)
    {
        if (value < minimum || value > maximum)
        {
            throw new ToolInvocationException(
                "ValidationError",
                $"Parameter '{parameterName}' must be between {minimum} and {maximum}.");
        }
    }
}
