# MCP Server Inventory

## Active Servers

| Server | Project | Installed by resetup | Notes |
| --- | --- | --- | --- |
| `candoitall_dotnetwatch` | `src/CanDoItAll.Mcp.DotNetWatch` | Shadow host under `CanDoItAll\.artifacts\mcp-server-shadow` | Launched through `tools\CanDoItAll.Mcp.DotNetWatch\Start-CanDoItAllDotNetWatchMcp.ps1`; settings stay in the main CanDoItAll repo. |
| `candoitall_components` | `src/CanDoItAll.Mcp.Components` | `CanDoItAll\.artifacts\mcp-installs\CanDoItAll.Mcp.Components\current` | Uses packaged `CanDoItAll.Components.*` libraries from the configured NuGet sources. |
| `candoitall_codeanalytics` | `src/CanDoItAll.Mcp.CodeAnalytics` | `CanDoItAll\.artifacts\mcp-installs\CanDoItAll.Mcp.CodeAnalytics\current` | Uses the sibling CodeAnalytics libraries and has no dependency on the main CanDoItAll app projects. |
| `candoitall_mermaid` | `src/CanDoItAll.Mcp.Mermaid` | `CanDoItAll\.artifacts\mcp-installs\CanDoItAll.Mcp.Mermaid\current` | Provides Mermaid syntax/catalog tooling through settings in the main repo. |
| `candoitall_sshops` | `src/CanDoItAll.Mcp.SshOps` | `CanDoItAll\.artifacts\mcp-installs\CanDoItAll.Mcp.SshOps\current` | Uses settings from `CanDoItAll.Mcp.SshOps.settings.json` in the main repo. |

## Companion Tools

| Tool | Path | Purpose |
| --- | --- | --- |
| DotNetWatch wrapper | `tools/CanDoItAll.Mcp.DotNetWatch` | Builds or reuses a shadowed DotNetWatch host so the MCP source can be rebuilt while the server is running. |
| DotNetWatch tray | `tools/CanDoItAll.Mcp.DotNetWatch.Tray` | User-facing tray operator for the shared backend. |
| MCP tool harness | `tools/CanDoItAll.Mcp.ToolHarness` | Local stdio harness for direct MCP tool testing. |

## Retired Servers

`CanDoItAll.Mcp.Processes` and `CanDoItAll.Mcp.ProjectStructure` are not active in this repository. Process and project-structure automation goes through the main CanDoItAll HTTP API and the repo-managed `candoitall-api-*` skills.
