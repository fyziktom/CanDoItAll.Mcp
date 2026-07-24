# Contributing

## Contribution Policy

Focused issues and pull requests are welcome. Discuss broad architecture changes before
implementation, and report security concerns through the private process in
[SECURITY.md](SECURITY.md), not through a public issue.

## Development Setup

1. Install the .NET SDK feature band pinned by `global.json`.
2. Place `CanDoItAll`, `CanDoItAll.CodeAnalysis`, and this repository beside one another.
3. Build the current CanDoItAll component packages into
   `CanDoItAll\ExternalPackages`.
4. Run commands from this repository root.

The sibling requirements are current adoption constraints, not permission to edit those
repositories as part of an unrelated change. See
[Architecture And Ownership](docs/architecture-and-ownership.md).

## Validation

Run the stable repository gate:

```powershell
& .\tools\validation\Test-Repository.ps1
```

The command restores and builds the solution, then runs the four stable test projects.
It writes normal .NET build output under `bin` and `obj`. Add
`-IncludeIntegrationTests` only when the local `CanDoItAll` runtime prerequisites are
available.

## Architecture Rules

- Keep MCP adapters thin over the product libraries that own domain behavior.
- Keep machine settings, installed MCP binaries, and resetup in the main `CanDoItAll`
  repository.
- Keep reusable MCP hosting and local-runtime primitives in their existing shared
  projects; avoid copying them into individual servers.
- Keep generated output and local state out of Git.
- Update documentation when public tools, settings, or installation contracts change.

## Pull Requests

- Keep changes focused.
- Add or update tests for behavior changes.
- Describe protocol, settings, and installation effects.
- Include the exact validation commands and results.
