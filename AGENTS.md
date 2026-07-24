# Repository Agent Instructions

## Shared Standards

Follow the reviewed standards in a resolved `CanDoItAll.SharedInfo` clone. This
repository owns its local implementation and any documented exceptions.

Use `$apply-candoitall-shared-standards` when available. It checks an explicit
`CANDOITALL_SHAREDINFO_ROOT` and nearby sibling locations without assuming that
SharedInfo is a child of this repository or that every machine uses the same root.

## Repository Scope

- This repository owns the CanDoItAll MCP server source, shared MCP runtime libraries,
  companion tools, and their tests.
- The sibling `CanDoItAll` repository owns machine settings, installed artifacts,
  resetup, and Codex configuration. `CanDoItAll.CodeAnalysis` and
  `CanDoItAll.Components` own the product libraries consumed here.

## Commands

- Build: `dotnet build .\CanDoItAll.Mcp.slnx --configuration Release`
- Test: `& .\tools\validation\Test-Repository.ps1 -NoRestore`
- Validate: `& .\tools\validation\Test-Repository.ps1`

## Safety

- Keep sibling repositories read-only unless the user explicitly requests a multi-repo
  change.
- Do not commit generated output, local settings, credentials, runtime state, or MCP
  protocol traffic.
- Preserve repository-specific changes that are unrelated to the active task.
- Treat changes to installed paths and resetup contracts as coordinated changes with the
  sibling `CanDoItAll` repository.
