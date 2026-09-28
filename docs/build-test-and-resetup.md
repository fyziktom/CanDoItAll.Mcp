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
4. runs the CodeAnalytics, Components, DotNetWatch, Mermaid, and SshOps stable test
   projects.

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

## CodeAnalytics MCP Installation

The main repository owns the installed CodeAnalytics MCP artifact. Publish and validate a
staged artifact, then switch it into the stable `current` path with its focused installer:

```powershell
Push-Location ..\CanDoItAll
try {
    & .\tools\install\Install-CanDoItAllCodeAnalyticsMcp.ps1 `
        -McpRepositoryRoot ..\CanDoItAll.Mcp `
        -SmokeRequestPath <path-to-test-impact-request.json>
}
finally {
    Pop-Location
}
```

The installer:

- publishes from the MCP repository into a unique staging directory;
- lists tools and invokes `code_analytics_impacted_tests_get` against the staged server;
- stops only processes executing the exact owned `current` entry point;
- retains the prior install in a timestamped backup and switches the staged directory;
- repeats the protocol smoke against `current` and rolls back if it fails.

The stable configured entry-point path does not change. Run with `-WhatIf` to preview the
installation without publishing, stopping processes, or changing artifacts. A connected
MCP client may need to reconnect before it discovers a newly added tool schema.

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
