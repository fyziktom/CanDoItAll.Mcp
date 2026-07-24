# CanDoItAll.Mcp

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![License](https://img.shields.io/badge/license-MIT--derived%20with%20website%20link-blue.svg)](LICENSE)

CanDoItAll.Mcp contains the CanDoItAll MCP servers, shared MCP runtime libraries, and
local companion tools. Keeping these projects separate from the main application makes
MCP builds, shadowing, testing, and reinstalling independently manageable.

## Ownership

This repository owns:

- MCP server and shared-runtime source under `src`;
- stable and integration tests under `tests`;
- the DotNetWatch wrapper and tray plus the local tool harness under `tools`;
- the MCP solution and repository-specific engineering documentation.

This repository does not own:

- machine-specific MCP settings, installed MCP artifacts, Codex configuration, or the
  reinstall workflow; those remain in the sibling `CanDoItAll` repository;
- CodeAnalytics domain behavior; it remains in `CanDoItAll.CodeAnalysis`;
- component-library behavior or packages; they remain in `CanDoItAll.Components`.

See [Architecture And Ownership](docs/architecture-and-ownership.md) for the dependency
boundaries and current standards-adoption exceptions.

## Projects And Entry Points

| Path | Purpose |
|---|---|
| `src/CanDoItAll.Mcp.Core` | Shared MCP hosting, contracts, logging, identity, and concurrency primitives. |
| `src/CanDoItAll.Mcp.LocalRuntime` | Local process and registry helpers for runtime-oriented MCPs. |
| `src/CanDoItAll.Mcp.DotNetWatch` | App lifecycle, build, test, logs, browser-session, and health tooling. |
| `src/CanDoItAll.Mcp.Components` | Component catalog and composition guidance backed by packaged Components libraries. |
| `src/CanDoItAll.Mcp.CodeAnalytics` | Code-analysis MCP backed by the sibling CodeAnalytics libraries. |
| `src/CanDoItAll.Mcp.Mermaid` | Mermaid syntax and authoring-guidance MCP. |
| `src/CanDoItAll.Mcp.SshOps` | Controlled SSH operations MCP. |
| `tools/CanDoItAll.Mcp.DotNetWatch` | PowerShell wrapper for building or reusing the DotNetWatch shadow host. |
| `tools/CanDoItAll.Mcp.DotNetWatch.Tray` | Windows tray operator for the shared DotNetWatch backend. |
| `tools/CanDoItAll.Mcp.ToolHarness` | Local command-line MCP tool harness. |

## Requirements

- The .NET 10 SDK feature band pinned by `global.json`.
- Sibling checkouts of `CanDoItAll` and `CanDoItAll.CodeAnalysis`.
- Current component packages under `..\CanDoItAll\ExternalPackages`.
- Windows for the tray application and the machine-level resetup workflow.

The sibling requirements are explicit local-workspace dependencies. They are not
filesystem dependencies that this repository owns or may mutate.

## Build And Test

Run the complete stable validation gate from this repository root:

```powershell
& .\tools\validation\Test-Repository.ps1
```

The script restores and builds the solution in `Release`, verifies required repository
files and local Markdown links, and runs the Components, DotNetWatch, Mermaid, and SshOps
stable test projects. It writes normal .NET output under ignored `bin` and `obj`
directories.

The equivalent build-only commands are:

```powershell
dotnet restore .\CanDoItAll.Mcp.slnx
dotnet build .\CanDoItAll.Mcp.slnx --configuration Release --no-restore
```

Run the environment-dependent DotNetWatch integration tests explicitly:

```powershell
& .\tools\validation\Test-Repository.ps1 -IncludeIntegrationTests
```

## Run And Reinstall

The Mermaid server is the smallest server that can be started with repository defaults:

```powershell
dotnet run --project .\src\CanDoItAll.Mcp.Mermaid\CanDoItAll.Mcp.Mermaid.csproj
```

It uses stdio transport and waits for an MCP client. Normal machine installation remains
owned by the main `CanDoItAll` repository because that workflow also updates settings,
Codex configuration, install artifacts, and tray shortcuts. See
[Build, Test, And Resetup](docs/build-test-and-resetup.md).

## Distribution And Status

This repository does not currently produce public NuGet packages; packing is disabled by
default in `Directory.Build.props`. MCP binaries are published locally by the main
repository's resetup workflow.

There is not yet a repository-local CI workflow because a clean build still requires the
two documented sibling workspaces. The local validation script is the current gate until
those dependencies have reproducible package or coordinated checkout contracts.

## Documentation

- [Architecture and ownership](docs/architecture-and-ownership.md)
- [Server inventory](docs/server-inventory.md)
- [Build, test, and resetup](docs/build-test-and-resetup.md)
- [Settings and artifacts](docs/settings-and-artifacts.md)
- [Contributing](CONTRIBUTING.md)
- [Security](SECURITY.md)

## License And Contributions

This repository uses the
[MIT-Derived License with CanDoItAll Website Link Requirement](LICENSE). Redistributions
of the software or a substantial portion of it in source or binary form must include at
least one link to [aicandoitall.com](https://aicandoitall.com). One such link satisfies
the added condition for a distribution containing multiple covered CanDoItAll libraries.

Code contributions are limited to partners approved by the maintainer. See
[CONTRIBUTING.md](CONTRIBUTING.md) and contact the `fyziktom` account on LinkedIn before
opening a pull request.
