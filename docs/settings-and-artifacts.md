# Settings And Artifacts

## Ownership

This repository owns MCP source code and the MCP solution. The sibling `CanDoItAll`
repository owns machine-specific runtime inputs and outputs.

Paths below are relative to this repository root when both repositories use the expected
sibling layout.

| Item | Owner | Location |
|---|---|---|
| MCP source and tests | MCP repo | `.` |
| DotNetWatch settings | Main repo | `..\CanDoItAll\CanDoItAll.Mcp.DotNetWatch.settings.json` |
| Components settings | Main repo | `..\CanDoItAll\CanDoItAll.Mcp.Components.settings.json` |
| CodeAnalytics settings | Main repo | `..\CanDoItAll\CanDoItAll.Mcp.CodeAnalytics.settings.json` |
| Mermaid settings | Main repo | `..\CanDoItAll\CanDoItAll.Mcp.Mermaid.settings.json` |
| SshOps settings | Main repo | `..\CanDoItAll\CanDoItAll.Mcp.SshOps.settings.json` |
| Current MCP installs | Main repo | `..\CanDoItAll\.artifacts\mcp-installs` |
| DotNetWatch shadow host | Main repo | `..\CanDoItAll\.artifacts\mcp-server-shadow` |
| Repository-owned skills | Main repo | `..\CanDoItAll\codex\skills` |

## Resetup Contract

`Reinstall-CanDoItAllMcps.ps1` accepts both roots:

- `-RepoRoot` points at the main CanDoItAll workspace.
- `-McpRepoRoot` points at this MCP repository.

The script must build MCP projects from `-McpRepoRoot`, but it must continue to read
settings and sync skills from `-RepoRoot`. This boundary is why moving installed paths or
companion tool folders requires a coordinated change.

## Secrets And Local Settings

Settings may contain workspace paths, SSH targets, secret references, or other
machine-specific values. Keep them in their owning main workspace and do not copy them
into this repository. `.gitignore` excludes `*.local.json`, `.env*` other than
`.env.example`, `.secrets`, and MCP runtime state.

Do not include credentials, host details, or sensitive protocol logs in issues, test
fixtures, screenshots, or committed evidence.

## Artifact Cleanup

Resetup removes historical MCP install artifacts before publishing fresh outputs:

- `.artifacts\mcp-installs`
- `.artifacts\mcp-server-shadow`

Generated history folders under `.artifacts` are not source of truth and can be removed
by their owning resetup workflow. Source changes in this repository must not directly
delete sibling artifacts unless that action is explicitly requested and the exact target
has been validated.
