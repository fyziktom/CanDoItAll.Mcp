# CanDoItAll.Mcp

This repository owns the CanDoItAll MCP servers and MCP companion tools. It is intentionally separate from the main `CanDoItAll` application repository so MCP builds can be validated, shadowed, and reinstalled without pulling the web app, product modules, or infrastructure projects into the same solution.

The main CanDoItAll repository still owns machine setup:

- MCP settings files, such as `CanDoItAll.Mcp.DotNetWatch.settings.json`.
- Installed MCP artifacts under `CanDoItAll\.artifacts\mcp-installs`.
- DotNetWatch shadow artifacts under `CanDoItAll\.artifacts\mcp-server-shadow`.
- Repo-managed Codex skills under `CanDoItAll\codex\skills`.
- The resetup script at `CanDoItAll\tools\Reinstall-CanDoItAllMcps.ps1`.

## Projects

| Path | Purpose |
| --- | --- |
| `src/CanDoItAll.Mcp.Core` | Shared MCP hosting, contracts, logging, identity, and concurrency primitives. |
| `src/CanDoItAll.Mcp.LocalRuntime` | Local process and registry helpers used by runtime-oriented MCPs. |
| `src/CanDoItAll.Mcp.DotNetWatch` | Shared dotnet-watch MCP backend for app lifecycle, build, test, logs, and health checks. |
| `src/CanDoItAll.Mcp.Components` | Component catalog MCP backed by the packaged CanDoItAll component libraries. |
| `src/CanDoItAll.Mcp.CodeAnalytics` | Code analytics MCP backed by the sibling CodeAnalytics libraries. |
| `src/CanDoItAll.Mcp.Mermaid` | Mermaid syntax and validation MCP. |
| `src/CanDoItAll.Mcp.SshOps` | SSH operations MCP. |
| `tools/CanDoItAll.Mcp.DotNetWatch` | PowerShell wrapper that builds or reuses the DotNetWatch shadow host. |
| `tools/CanDoItAll.Mcp.DotNetWatch.Tray` | Tray operator for the shared DotNetWatch backend. |
| `tools/CanDoItAll.Mcp.ToolHarness` | Local MCP tool harness. |

The MCP solution does not reference the CanDoItAll web app or product modules. Component dependencies are consumed from NuGet packages through `NuGet.config`, matching the normal application consumption path.

## Build

From this repository root:

```powershell
dotnet restore .\CanDoItAll.Mcp.slnx
dotnet build .\CanDoItAll.Mcp.slnx -c Release
```

Run the focused stable tests:

```powershell
dotnet test .\tests\CanDoItAll.Mcp.Components.Tests\CanDoItAll.Mcp.Components.Tests.csproj -c Release
dotnet test .\tests\CanDoItAll.Mcp.DotNetWatch.Tests\CanDoItAll.Mcp.DotNetWatch.Tests.csproj -c Release
dotnet test .\tests\CanDoItAll.Mcp.Mermaid.Tests\CanDoItAll.Mcp.Mermaid.Tests.csproj -c Release
dotnet test .\tests\CanDoItAll.Mcp.SshOps.Tests\CanDoItAll.Mcp.SshOps.Tests.csproj -c Release
```

DotNetWatch integration tests start local processes and should be run explicitly:

```powershell
dotnet test .\tests\CanDoItAll.Mcp.DotNetWatch.IntegrationTests\CanDoItAll.Mcp.DotNetWatch.IntegrationTests.csproj -c Release --filter "Category!=Quarantined"
```

## Reinstall On A Machine

The supported reinstall entry point remains in the main CanDoItAll repository because that script also updates Codex config, VS Code config, skill sync, install artifacts, and tray shortcuts.

From `C:\repositories\CanDoItAll`:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\Reinstall-CanDoItAllMcps.ps1 -McpRepoRoot C:\repositories\CanDoItAll.Mcp
```

If this repository sits beside `CanDoItAll`, `-McpRepoRoot` can be omitted.

## Documentation

- [Server inventory](docs/server-inventory.md)
- [Build, test, and resetup](docs/build-test-and-resetup.md)
- [Settings and artifacts](docs/settings-and-artifacts.md)
