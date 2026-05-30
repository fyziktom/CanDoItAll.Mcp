# Settings And Artifacts

## Ownership

This repository owns MCP source code and the MCP solution. The main CanDoItAll repository owns machine-specific runtime inputs and outputs.

| Item | Owner | Location |
| --- | --- | --- |
| MCP source and tests | MCP repo | `C:\repositories\CanDoItAll.Mcp` |
| DotNetWatch settings | Main repo | `C:\repositories\CanDoItAll\CanDoItAll.Mcp.DotNetWatch.settings.json` |
| Components settings | Main repo | `C:\repositories\CanDoItAll\CanDoItAll.Mcp.Components.settings.json` |
| CodeAnalytics settings | Main repo | `C:\repositories\CanDoItAll\CanDoItAll.Mcp.CodeAnalytics.settings.json` |
| Mermaid settings | Main repo | `C:\repositories\CanDoItAll\CanDoItAll.Mcp.Mermaid.settings.json` |
| SshOps settings | Main repo | `C:\repositories\CanDoItAll\CanDoItAll.Mcp.SshOps.settings.json` |
| Current MCP installs | Main repo | `C:\repositories\CanDoItAll\.artifacts\mcp-installs` |
| DotNetWatch shadow host | Main repo | `C:\repositories\CanDoItAll\.artifacts\mcp-server-shadow` |
| Repo-managed skills | Main repo | `C:\repositories\CanDoItAll\codex\skills` |

## Resetup Contract

`Reinstall-CanDoItAllMcps.ps1` accepts both roots:

- `-RepoRoot` points at the main CanDoItAll workspace.
- `-McpRepoRoot` points at this MCP repository.

The script must build MCP projects from `-McpRepoRoot`, but it must continue to read settings and sync skills from `-RepoRoot`.

## Artifact Cleanup

Resetup removes historical MCP install artifacts before publishing fresh outputs:

- `.artifacts\mcp-installs`
- `.artifacts\mcp-server-shadow`

Generated history folders in `.artifacts` that contain old MCP binaries are not source-of-truth and can be deleted. Current MCP runtime state should only remain under the two live roots above.
