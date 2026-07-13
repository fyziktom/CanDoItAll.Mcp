using System.Text.Json;
using ModelContextProtocol.Client;

namespace CanDoItAll.Mcp.Components.Tests;

public sealed class ComponentsMcpProtocolTests
{
    [Fact]
    public async Task Stdio_Server_Lists_And_Invokes_New_Guidance_Tools()
    {
        var workspaceRoot = FindWorkspaceRoot();
        var settingsPath = Path.Combine(Path.GetTempPath(), $"candoitall-components-mcp-{Guid.NewGuid():N}.settings.json");
        await File.WriteAllTextAsync(settingsPath, JsonSerializer.Serialize(new
        {
            Server = new
            {
                Name = "CanDoItAll.Mcp.Components.ProtocolTests",
                WorkspaceRoot = workspaceRoot,
                IdleShutdown = new
                {
                    Enabled = false,
                    InactivityTimeoutSeconds = 300,
                    CheckIntervalSeconds = 15
                }
            },
            Catalog = new
            {
                CommonRoot = Path.Combine("..", "CanDoItAll.Components", "src", "CanDoItAll.Components.Common"),
                BaseLibRoot = Path.Combine("..", "CanDoItAll.Components", "src", "CanDoItAll.Components.BaseLib"),
                CanvasLibRoot = Path.Combine("..", "CanDoItAll.Components", "src", "CanDoItAll.Components.CanvasLib"),
                ChartsRoot = Path.Combine("..", "CanDoItAll.Components", "src", "CanDoItAll.Components.Charts"),
                MermaidRoot = Path.Combine("..", "CanDoItAll.Components", "src", "CanDoItAll.Components.Mermaid"),
                OverlayLibRoot = Path.Combine("..", "CanDoItAll.Components", "src", "CanDoItAll.Components.OverlayLib"),
                QrCodeRoot = Path.Combine("..", "CanDoItAll.Components", "src", "CanDoItAll.Components.QRCode"),
                SandboxRoot = Path.Combine("..", "CanDoItAll.Components", "samples", "CanDoItAll.Components.Sandbox")
            }
        }));

        try
        {
            var configuration = AppContext.BaseDirectory.Contains($"{Path.DirectorySeparatorChar}Release{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                ? "Release"
                : "Debug";
            var projectPath = Path.Combine(workspaceRoot, "src", "CanDoItAll.Mcp.Components", "CanDoItAll.Mcp.Components.csproj");
            var transport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "CanDoItAll.Mcp.Components.ProtocolTests",
                Command = "dotnet",
                Arguments = ["run", "--project", projectPath, "--configuration", configuration, "--no-build", "--", "--settings", settingsPath],
                WorkingDirectory = workspaceRoot,
                ShutdownTimeout = TimeSpan.FromSeconds(15)
            });

            await using var client = await McpClient.CreateAsync(transport);
            var tools = await client.ListToolsAsync();
            var toolNames = tools.Select(tool => tool.Name).ToArray();

            Assert.Contains("components_libraries_list", toolNames, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("app_shell_guide_get", toolNames, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("components_recommend", toolNames, StringComparer.OrdinalIgnoreCase);

            var appShell = await client.CallToolAsync(
                "app_shell_guide_get",
                new Dictionary<string, object?> { ["mode"] = "full-height" });
            var recommendation = await client.CallToolAsync(
                "components_recommend",
                new Dictionary<string, object?> { ["useCase"] = "scan a QR code with camera fallback" });

            Assert.False(appShell.IsError is true);
            Assert.False(recommendation.IsError is true);
            var appShellOutput = appShell.StructuredContent is JsonElement appShellStructuredContent
                ? appShellStructuredContent.GetRawText()
                : JsonSerializer.Serialize(appShell.Content);
            var recommendationOutput = recommendation.StructuredContent is JsonElement recommendationStructuredContent
                ? recommendationStructuredContent.GetRawText()
                : JsonSerializer.Serialize(recommendation.Content);

            Assert.Contains("SideMenu", appShellOutput, StringComparison.Ordinal);
            Assert.Contains("QrScanButton", recommendationOutput, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(settingsPath);
        }
    }

    private static string FindWorkspaceRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "CanDoItAll.Mcp.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate the CanDoItAll.Mcp workspace root for protocol tests.");
    }
}
