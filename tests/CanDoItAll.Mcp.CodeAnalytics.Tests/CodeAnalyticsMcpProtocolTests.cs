using System.Diagnostics;
using System.Text.Json;

namespace CanDoItAll.Mcp.CodeAnalytics.Tests;

public sealed class CodeAnalyticsMcpProtocolTests
{
    [Fact]
    public async Task Stdio_Server_Lists_And_Invokes_Impacted_Tests_Across_A_Separate_Test_Solution()
    {
        var mcpRepositoryRoot = FindRepositoryRoot("CanDoItAll.Mcp.slnx");
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        using var fixture = new TemporaryProtocolFixture(
            Path.Combine(mcpRepositoryRoot, "NuGet.config"));
        await fixture.RestoreAsync(timeout.Token);

        var configuration = AppContext.BaseDirectory.Contains(
            $"{Path.DirectorySeparatorChar}Release{Path.DirectorySeparatorChar}",
            StringComparison.OrdinalIgnoreCase)
            ? "Release"
            : "Debug";
        var serverAssemblyPath = Path.Combine(
            mcpRepositoryRoot,
            "src",
            "CanDoItAll.Mcp.CodeAnalytics",
            "bin",
            configuration,
            "net10.0",
            "CanDoItAll.Mcp.CodeAnalytics.dll");
        var toolsOutput = await RunToolHarnessAsync(
            mcpRepositoryRoot,
            configuration,
            serverAssemblyPath,
            fixture,
            "tools/list",
            argumentsPath: null,
            timeout.Token);
        using var toolsDocument = JsonDocument.Parse(toolsOutput);
        var impactedTestsTool = Assert.Single(
            toolsDocument.RootElement.EnumerateArray(),
            tool => string.Equals(
                tool.GetProperty("protocolTool").GetProperty("name").GetString(),
                "code_analytics_impacted_tests_get",
                StringComparison.OrdinalIgnoreCase));
        var protocolTool = impactedTestsTool.GetProperty("protocolTool");
        var annotations = protocolTool.GetProperty("annotations");
        Assert.True(annotations.GetProperty("readOnlyHint").GetBoolean());
        Assert.True(annotations.GetProperty("idempotentHint").GetBoolean());
        Assert.False(annotations.GetProperty("openWorldHint").GetBoolean());
        Assert.Equal(JsonValueKind.Object, protocolTool.GetProperty("outputSchema").ValueKind);

        var inputSchema = protocolTool.GetProperty("inputSchema").GetRawText();
        Assert.Contains("request", inputSchema, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("testWorkspaces", inputSchema, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("changes", inputSchema, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("lineRanges", inputSchema, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("behaviorIntent", inputSchema, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("contextOnlyPaths", inputSchema, StringComparison.OrdinalIgnoreCase);

        var invocationOutput = await RunToolHarnessAsync(
            mcpRepositoryRoot,
            configuration,
            serverAssemblyPath,
            fixture,
            "code_analytics_impacted_tests_get",
            fixture.ArgumentsPath,
            timeout.Token);
        using var invocationDocument = JsonDocument.Parse(invocationOutput);
        var invocation = invocationDocument.RootElement;
        Assert.True(invocation.GetProperty("ok").GetBoolean());
        Assert.Equal("code_analytics_impacted_tests_get", invocation.GetProperty("tool").GetString());

        var data = invocation.GetProperty("data");
        var analysis = data.GetProperty("analysis");
        var requiredSelection = Assert.Single(analysis.GetProperty("requiredSelections").EnumerateArray());
        Assert.Contains(
            "Sample.Core.Tests.CalculatorTests.Add_Returns_The_Sum",
            requiredSelection.GetProperty("selectors").EnumerateArray().Select(selector => selector.GetString()));
        var workspaceHealth = Assert.Single(analysis.GetProperty("workspaceHealth").EnumerateArray());
        Assert.True(workspaceHealth.GetProperty("isHealthy").GetBoolean());
        Assert.Equal(fixture.TestSolutionPath, workspaceHealth.GetProperty("workspacePath").GetString());
        Assert.Equal(
            fixture.ContextOnlySourcePath,
            Assert.Single(data.GetProperty("ignoredContextOnlyPaths").EnumerateArray()).GetString());
    }

    private static async Task<string> RunToolHarnessAsync(
        string mcpRepositoryRoot,
        string configuration,
        string serverAssemblyPath,
        TemporaryProtocolFixture fixture,
        string toolName,
        string? argumentsPath,
        CancellationToken cancellationToken)
    {
        var harnessAssemblyPath = Path.Combine(
            mcpRepositoryRoot,
            "tools",
            "CanDoItAll.Mcp.ToolHarness",
            "bin",
            configuration,
            "net10.0",
            "CanDoItAll.Mcp.ToolHarness.dll");
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = mcpRepositoryRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (var argument in new[]
                 {
                     harnessAssemblyPath,
                     "--server-assembly",
                     serverAssemblyPath,
                     "--settings",
                     fixture.SettingsPath,
                     "--working-directory",
                     fixture.RootPath,
                     "--tool",
                     toolName
                 })
        {
            startInfo.ArgumentList.Add(argument);
        }

        if (argumentsPath is not null)
        {
            startInfo.ArgumentList.Add("--arguments-file");
            startInfo.ArgumentList.Add(argumentsPath);
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Could not start MCP ToolHarness for '{toolName}'.");
        var standardOutputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardErrorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync(CancellationToken.None);
            }

            throw new TimeoutException($"MCP ToolHarness '{toolName}' exceeded the protocol test timeout.");
        }

        var standardOutput = await standardOutputTask;
        var standardError = await standardErrorTask;
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"MCP ToolHarness '{toolName}' failed with exit code {process.ExitCode}.{Environment.NewLine}{standardOutput}{Environment.NewLine}{standardError}");
        }

        return standardOutput;
    }

    private static string FindRepositoryRoot(string markerFileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, markerFileName)))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException($"Could not locate repository root containing '{markerFileName}'.");
    }

    private sealed class TemporaryProtocolFixture : IDisposable
    {
        private readonly string _nuGetConfigPath;

        public TemporaryProtocolFixture(string nuGetConfigPath)
        {
            _nuGetConfigPath = nuGetConfigPath;
            RootPath = Path.Combine(Path.GetTempPath(), $"candoitall-codeanalytics-protocol-{Guid.NewGuid():N}");
            ChangedSourcePath = WriteFile(
                "src/Sample.Core/Calculator.cs",
                """
                namespace Sample.Core;

                public static class Calculator
                {
                    public static int Add(int left, int right) => left + right;
                }
                """);
            ContextOnlySourcePath = WriteFile(
                "src/Sample.Core/CalculationContext.cs",
                """
                namespace Sample.Core;

                public sealed record CalculationContext(string Name);
                """);
            WriteFile(
                "src/Sample.Core/Sample.Core.csproj",
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net10.0</TargetFramework>
                    <ImplicitUsings>enable</ImplicitUsings>
                    <Nullable>enable</Nullable>
                  </PropertyGroup>
                </Project>
                """);
            WriteFile(
                "tests/Sample.Core.Tests/FactAttribute.cs",
                """
                namespace Xunit;

                [AttributeUsage(AttributeTargets.Method)]
                public sealed class FactAttribute : Attribute;
                """);
            WriteFile(
                "tests/Sample.Core.Tests/CalculatorTests.cs",
                """
                using Sample.Core;
                using Xunit;

                namespace Sample.Core.Tests;

                public sealed class CalculatorTests
                {
                    [Fact]
                    public void Add_Returns_The_Sum()
                    {
                        _ = Calculator.Add(2, 3);
                    }
                }
                """);
            WriteFile(
                "tests/Sample.Core.Tests/Sample.Core.Tests.csproj",
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net10.0</TargetFramework>
                    <ImplicitUsings>enable</ImplicitUsings>
                    <Nullable>enable</Nullable>
                  </PropertyGroup>
                  <ItemGroup>
                    <ProjectReference Include="../../src/Sample.Core/Sample.Core.csproj" />
                  </ItemGroup>
                </Project>
                """);
            WriteFile(
                "Product.slnx",
                """
                <Solution>
                  <Folder Name="/src/">
                    <Project Path="src/Sample.Core/Sample.Core.csproj" />
                  </Folder>
                </Solution>
                """);
            TestSolutionPath = WriteFile(
                "tests/Sample.Tests.slnx",
                """
                <Solution>
                  <Folder Name="/src/">
                    <Project Path="../src/Sample.Core/Sample.Core.csproj" />
                  </Folder>
                  <Folder Name="/tests/">
                    <Project Path="Sample.Core.Tests/Sample.Core.Tests.csproj" />
                  </Folder>
                </Solution>
                """);
            SettingsPath = WriteFile(
                "CanDoItAll.Mcp.CodeAnalytics.settings.json",
                JsonSerializer.Serialize(new
                {
                    Server = new
                    {
                        Name = "CanDoItAll.Mcp.CodeAnalytics.ProtocolTests"
                    },
                    Analysis = new
                    {
                        RepositoryRoot = RootPath,
                        DefaultSolutionPath = "Product.slnx",
                        OutputRootPath = ".artifacts/code-analytics",
                        GeneratorVersion = "protocol-tests",
                        MaxRecentSnapshots = 5,
                        MaxDiagramNodes = 20
                    }
                }));
            ArgumentsPath = WriteFile(
                "impacted-tests-request.json",
                JsonSerializer.Serialize(new
                {
                    request = new
                    {
                        repositoryRootPath = RootPath,
                        testWorkspaces = new[]
                        {
                            new
                            {
                                path = TestSolutionPath,
                                name = "Unit"
                            }
                        },
                        changes = new[]
                        {
                            new
                            {
                                path = ChangedSourcePath,
                                lineRanges = new[]
                                {
                                    new
                                    {
                                        startLine = 5,
                                        endLine = 5
                                    }
                                },
                                behaviorIntent = "BehaviorPreservingImplementation"
                            }
                        },
                        contextOnlyPaths = new[]
                        {
                            ContextOnlySourcePath
                        },
                        maxVisitedMembers = 5000,
                        maxSelectors = 30,
                        maxReasonPaths = 12
                    }
                }));
        }

        public string RootPath { get; }
        public string ChangedSourcePath { get; }
        public string ContextOnlySourcePath { get; }
        public string TestSolutionPath { get; }
        public string SettingsPath { get; }
        public string ArgumentsPath { get; }

        public async Task RestoreAsync(CancellationToken cancellationToken)
        {
            var startInfo = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = RootPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            var isolatedAppDataPath = Path.Combine(RootPath, ".appdata");
            Directory.CreateDirectory(isolatedAppDataPath);
            startInfo.Environment["APPDATA"] = isolatedAppDataPath;
            startInfo.ArgumentList.Add("restore");
            startInfo.ArgumentList.Add(TestSolutionPath);
            startInfo.ArgumentList.Add("--nologo");
            startInfo.ArgumentList.Add("--disable-build-servers");
            startInfo.ArgumentList.Add("--configfile");
            startInfo.ArgumentList.Add(_nuGetConfigPath);

            using var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Could not start dotnet restore for the protocol fixture.");
            var standardOutputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var standardErrorTask = process.StandardError.ReadToEndAsync(cancellationToken);
            try
            {
                await process.WaitForExitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                    await process.WaitForExitAsync(CancellationToken.None);
                }

                throw new TimeoutException("Protocol fixture restore exceeded the protocol test timeout.");
            }

            var standardOutput = await standardOutputTask;
            var standardError = await standardErrorTask;

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Protocol fixture restore failed with exit code {process.ExitCode}.{Environment.NewLine}{standardOutput}{Environment.NewLine}{standardError}");
            }
        }

        public void Dispose()
        {
            Directory.Delete(RootPath, recursive: true);
        }

        private string WriteFile(string relativePath, string content)
        {
            var path = Path.Combine(RootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);
            return Path.GetFullPath(path);
        }
    }
}
