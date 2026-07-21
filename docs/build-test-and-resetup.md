# Build, Test, And Resetup

## Prerequisites

- .NET 10 SDK.
- The sibling `CanDoItAll` repository when running resetup or integration tests.
- The sibling `CanDoItAll.CodeAnalysis` repository for the CodeAnalytics MCP project references.
- Component packages available through `..\CanDoItAll\ExternalPackages`, as configured in `NuGet.config`.

## Build

```powershell
dotnet restore .\CanDoItAll.Mcp.slnx
dotnet build .\CanDoItAll.Mcp.slnx -c Release
```

Use the solution build as the primary compile gate. The solution contains the MCP servers, shared MCP libraries, companion tools, and MCP test projects.

## Stable Tests

```powershell
dotnet test .\tests\CanDoItAll.Mcp.Components.Tests\CanDoItAll.Mcp.Components.Tests.csproj -c Release
dotnet test .\tests\CanDoItAll.Mcp.DotNetWatch.Tests\CanDoItAll.Mcp.DotNetWatch.Tests.csproj -c Release
dotnet test .\tests\CanDoItAll.Mcp.Mermaid.Tests\CanDoItAll.Mcp.Mermaid.Tests.csproj -c Release
dotnet test .\tests\CanDoItAll.Mcp.SshOps.Tests\CanDoItAll.Mcp.SshOps.Tests.csproj -c Release
```

## Integration Tests

DotNetWatch integration tests use the MCP repo for server binaries and the sibling `CanDoItAll` repo for workspace settings and runtime state.

```powershell
dotnet test .\tests\CanDoItAll.Mcp.DotNetWatch.IntegrationTests\CanDoItAll.Mcp.DotNetWatch.IntegrationTests.csproj -c Release --filter "Category!=Quarantined"
```

Run the no-filter integration project only when live process, long-running, and quarantined scenarios are part of the task.

## Machine Resetup

Run resetup from the main CanDoItAll repo:

```powershell
cd C:\repositories\CanDoItAll
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\Reinstall-CanDoItAllMcps.ps1 -McpRepoRoot C:\repositories\CanDoItAll.Mcp
```

The script:

- Builds the DotNetWatch shadow host from this repo.
- Publishes Components, CodeAnalytics, Mermaid, SshOps, and the DotNetWatch tray from this repo.
- Publishes `CanDoItAll.Manager` from the main repo because it is not an MCP server.
- Updates `.vscode\mcp.json` in the main repo.
- Updates `%USERPROFILE%\.codex\config.toml`.
- Syncs skills from the main repo `codex\skills`.
- Recreates current MCP install and DotNetWatch shadow artifacts under the main repo `.artifacts` directory.

## Manual DotNetWatch Shadow Rebuild

From the main CanDoItAll repo:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File ..\CanDoItAll.Mcp\tools\CanDoItAll.Mcp.DotNetWatch\Start-CanDoItAllDotNetWatchMcp.ps1 -RepoRoot . -McpRepoRoot ..\CanDoItAll.Mcp -Configuration Release -SettingsPath .\CanDoItAll.Mcp.DotNetWatch.settings.json -ForceRebuild -PrepareOnly
```
