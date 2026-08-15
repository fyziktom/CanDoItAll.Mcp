using CanDoItAll.CodeAnalytics.Abstractions;
using CanDoItAll.CodeAnalytics.Abstractions.Queries;
using CanDoItAll.CodeAnalytics.Abstractions.Responses;
using CanDoItAll.CodeAnalytics.Domain.TestImpact;
using CanDoItAll.Mcp.Core.Contracts;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CanDoItAll.Mcp.CodeAnalytics.Tests;

public sealed class CodeAnalyticsImpactedTestsTests
{
    [Fact]
    public async Task Tool_Uses_Only_Actual_Changes_And_Acknowledges_Context_Only_Helpers()
    {
        using var workspace = new TemporaryWorkspace();
        var testWorkspacePath = workspace.WriteFile("tests/Unit.slnx", "<Solution />");
        var changedPath = workspace.WriteFile("src/Calculator.cs", "namespace Sample; public static class Calculator { public static int Add(int left, int right) => left + right; }");
        var contextPath = workspace.WriteFile("src/MathHelper.cs", "namespace Sample; public static class MathHelper { public static int Identity(int value) => value; }");
        var impactService = new CapturingTestImpactAnalysisService();
        var tools = CreateTools(workspace.RootPath, impactService);

        var result = await tools.CodeAnalyticsImpactedTestsGetAsync(new CodeAnalyticsImpactedTestsInput
        {
            RepositoryRootPath = workspace.RootPath,
            TestWorkspaces =
            [
                new CodeAnalyticsTestWorkspaceInput
                {
                    Path = "tests/Unit.slnx",
                    Name = "Unit"
                }
            ],
            Changes =
            [
                new CodeAnalyticsChangedCodeInput
                {
                    Path = "src/Calculator.cs",
                    LineRanges =
                    [
                        new CodeAnalyticsChangedLineRangeInput
                        {
                            StartLine = 1,
                            EndLine = 1
                        }
                    ],
                    BehaviorIntent = ChangeBehaviorIntent.BehaviorPreservingImplementation
                }
            ],
            ContextOnlyPaths = ["src/MathHelper.cs"]
        });

        Assert.True(result.Ok);
        Assert.NotNull(result.Data);
        var query = Assert.IsType<TestImpactQuery>(impactService.CapturedQuery);
        var change = Assert.Single(query.Changes);
        Assert.Equal(changedPath, change.Path);
        Assert.Equal(ChangeBehaviorIntent.BehaviorPreservingImplementation, change.BehaviorIntent);
        var lineRange = Assert.Single(change.LineRanges);
        Assert.Equal(1, lineRange.StartLine);
        Assert.Equal(1, lineRange.EndLine);
        var testWorkspace = Assert.Single(query.TestWorkspaces);
        Assert.Equal(testWorkspacePath, testWorkspace.Path);
        Assert.Equal("Unit", testWorkspace.Name);
        Assert.Equal(contextPath, Assert.Single(result.Data!.IgnoredContextOnlyPaths));
        Assert.DoesNotContain(query.Changes, candidate => string.Equals(candidate.Path, contextPath, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Tool_Rejects_A_Path_That_Is_Both_Changed_And_Context_Only()
    {
        using var workspace = new TemporaryWorkspace();
        workspace.WriteFile("tests/Unit.slnx", "<Solution />");
        workspace.WriteFile("src/Calculator.cs", "namespace Sample; public static class Calculator { }");
        var impactService = new CapturingTestImpactAnalysisService();
        var tools = CreateTools(workspace.RootPath, impactService);

        var result = await tools.CodeAnalyticsImpactedTestsGetAsync(new CodeAnalyticsImpactedTestsInput
        {
            RepositoryRootPath = workspace.RootPath,
            TestWorkspaces = [new CodeAnalyticsTestWorkspaceInput { Path = "tests/Unit.slnx" }],
            Changes = [new CodeAnalyticsChangedCodeInput { Path = "src/Calculator.cs" }],
            ContextOnlyPaths = ["src/Calculator.cs"]
        });

        Assert.False(result.Ok);
        Assert.Equal("validation_error", result.Status);
        Assert.Equal("ValidationError", result.Error?.Code);
        Assert.Null(impactService.CapturedQuery);
    }

    [Fact]
    public async Task Tool_Rejects_A_Missing_Separate_Test_Workspace()
    {
        using var workspace = new TemporaryWorkspace();
        workspace.WriteFile("src/Calculator.cs", "namespace Sample; public static class Calculator { }");
        var impactService = new CapturingTestImpactAnalysisService();
        var tools = CreateTools(workspace.RootPath, impactService);

        var result = await tools.CodeAnalyticsImpactedTestsGetAsync(new CodeAnalyticsImpactedTestsInput
        {
            RepositoryRootPath = workspace.RootPath,
            TestWorkspaces = [new CodeAnalyticsTestWorkspaceInput { Path = "tests/Missing.slnx" }],
            Changes = [new CodeAnalyticsChangedCodeInput { Path = "src/Calculator.cs" }]
        });

        Assert.False(result.Ok);
        Assert.Equal("not_found", result.Status);
        Assert.Equal("TestWorkspacePathNotFound", result.Error?.Code);
        Assert.Null(impactService.CapturedQuery);
    }

    [Fact]
    public async Task Tool_Rejects_Invalid_Changed_Line_Ranges()
    {
        using var workspace = new TemporaryWorkspace();
        workspace.WriteFile("tests/Unit.slnx", "<Solution />");
        workspace.WriteFile("src/Calculator.cs", "namespace Sample; public static class Calculator { }");
        var impactService = new CapturingTestImpactAnalysisService();
        var tools = CreateTools(workspace.RootPath, impactService);

        var result = await tools.CodeAnalyticsImpactedTestsGetAsync(new CodeAnalyticsImpactedTestsInput
        {
            RepositoryRootPath = workspace.RootPath,
            TestWorkspaces = [new CodeAnalyticsTestWorkspaceInput { Path = "tests/Unit.slnx" }],
            Changes =
            [
                new CodeAnalyticsChangedCodeInput
                {
                    Path = "src/Calculator.cs",
                    LineRanges = [new CodeAnalyticsChangedLineRangeInput { StartLine = 10, EndLine = 4 }]
                }
            ]
        });

        Assert.False(result.Ok);
        Assert.Equal("validation_error", result.Status);
        Assert.Equal("ValidationError", result.Error?.Code);
        Assert.Null(impactService.CapturedQuery);
    }

    private static CodeAnalyticsTools CreateTools(
        string repositoryRootPath,
        ITestImpactAnalysisService impactAnalysisService)
    {
        var options = Options.Create(new McpServerOptions
        {
            Analysis = new AnalysisOptions
            {
                RepositoryRoot = repositoryRootPath,
                DefaultSolutionPath = "default.slnx",
                OutputRootPath = ".artifacts/code-analytics-tests"
            }
        });
        var runtimeConfiguration = new RuntimeConfiguration(options);
        var coordinator = new CodeAnalyticsCoordinator(
            applicationService: null!,
            impactAnalysisService,
            runtimeConfiguration);

        return new CodeAnalyticsTools(coordinator, NullLogger<CodeAnalyticsTools>.Instance);
    }

    private sealed class CapturingTestImpactAnalysisService : ITestImpactAnalysisService
    {
        public TestImpactQuery? CapturedQuery { get; private set; }

        public Task<TestImpactResponse> AnalyzeAsync(
            TestImpactQuery query,
            CancellationToken cancellationToken = default)
        {
            CapturedQuery = query;
            return Task.FromResult(new TestImpactResponse(
                IsComplete: true,
                Confidence: TestImpactConfidence.High,
                ResolvedChanges: [],
                RequiredSelections: [],
                ConditionalSelections: [],
                ContainmentOpportunities: [],
                WorkspaceHealth: [],
                FallbackKind: TestImpactFallbackKind.None,
                FallbackReason: null,
                Diagnostics: []));
        }
    }

    private sealed class TemporaryWorkspace : IDisposable
    {
        public TemporaryWorkspace()
        {
            RootPath = Path.Combine(Path.GetTempPath(), $"candoitall-codeanalytics-mcp-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(RootPath);
        }

        public string RootPath { get; }

        public string WriteFile(string relativePath, string content)
        {
            var path = Path.Combine(RootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);
            return Path.GetFullPath(path);
        }

        public void Dispose()
        {
            Directory.Delete(RootPath, recursive: true);
        }
    }
}
