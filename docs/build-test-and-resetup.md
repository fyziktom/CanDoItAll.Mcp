# Build, Test, And Resetup

## Prerequisites

- The .NET 10 SDK feature band pinned by `global.json`.
- A sibling `CanDoItAll` checkout for the local Components package feed, resetup, and
  integration tests.
- A sibling `CanDoItAll.CodeAnalysis` checkout for the CodeAnalytics MCP project
  references.
- Current Components packages under `..\CanDoItAll\ExternalPackages`, as configured in
  `NuGet.config`.

These are current adoption constraints. See
[Architecture And Ownership](architecture-and-ownership.md) for their exit conditions.

## Stable Validation

Run the repository-owned validation entry point:

```powershell
& .\tools\validation\Test-Repository.ps1
```

The script:

1. verifies the required root files and local Markdown links;
2. restores `CanDoItAll.Mcp.slnx`;
3. builds the solution in `Release`;
4. runs the Components, DotNetWatch, Mermaid, and SshOps stable test projects.

The command creates normal ignored .NET output under `bin` and `obj`. Preview the
external-tool work without changing build output:

```powershell
& .\tools\validation\Test-Repository.ps1 -WhatIf
```

If restore already completed, use:

```powershell
& .\tools\validation\Test-Repository.ps1 -NoRestore
```

## Build Only

```powershell
dotnet restore .\CanDoItAll.Mcp.slnx
dotnet build .\CanDoItAll.Mcp.slnx --configuration Release --no-restore
```

The solution build is the primary compile gate. It contains the MCP servers, shared MCP
libraries, companion tools, and MCP test projects.

## Integration Tests

DotNetWatch integration tests start local processes and use the sibling `CanDoItAll`
workspace for settings and runtime state. Run the non-quarantined integration gate only
when those prerequisites are available:

```powershell
& .\tools\validation\Test-Repository.ps1 -IncludeIntegrationTests
```

Run the integration project without its filter only when live-process, long-running, and
quarantined scenarios are explicitly part of the task.

## Machine Resetup

From this repository root, enter the sibling main repository and invoke its owned resetup
entry point:

```powershell
Push-Location ..\CanDoItAll
try {
    & .\tools\Reinstall-CanDoItAllMcps.ps1 -McpRepoRoot ..\CanDoItAll.Mcp
}
finally {
    Pop-Location
}
```

The resetup workflow:

- builds the DotNetWatch shadow host from this repository;
- publishes Components, CodeAnalytics, Mermaid, SshOps, and the DotNetWatch tray from
  this repository;
- publishes `CanDoItAll.Manager` from the main repository because it is not an MCP
  server;
- updates the main repository's `.vscode\mcp.json` and the user's Codex configuration;
- syncs repository-owned skills;
- recreates current MCP install and shadow artifacts under the main repository's ignored
  `.artifacts` directory.

Resetup changes machine configuration and local install artifacts. Review its parameters
in the owning repository before running it.

## Manual DotNetWatch Shadow Rebuild

From this repository root:

```powershell
& .\tools\CanDoItAll.Mcp.DotNetWatch\Start-CanDoItAllDotNetWatchMcp.ps1 `
    -RepoRoot ..\CanDoItAll `
    -McpRepoRoot . `
    -Configuration Release `
    -SettingsPath ..\CanDoItAll\CanDoItAll.Mcp.DotNetWatch.settings.json `
    -ForceRebuild `
    -PrepareOnly
```

This command creates or replaces ignored shadow-build output in the sibling main
repository.
