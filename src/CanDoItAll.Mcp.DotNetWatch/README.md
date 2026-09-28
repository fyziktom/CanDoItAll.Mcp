# CanDoItAll.Mcp.DotNetWatch

## Purpose

MCP adapter and backend for supervising dotnet watch, app readiness, browser sessions, and development capsules.

## Project Type

- SDK: `Microsoft.NET.Sdk`
- Target framework(s): `net10.0`
- Validation command:

```powershell
dotnet build src/CanDoItAll.Mcp.DotNetWatch/CanDoItAll.Mcp.DotNetWatch.csproj
```

## References

Project references:

- `..//CanDoItAll.Mcp.Core//CanDoItAll.Mcp.Core.csproj`
- `..//CanDoItAll.Mcp.LocalRuntime//CanDoItAll.Mcp.LocalRuntime.csproj`

Framework references:

- `Microsoft.AspNetCore.App`

Direct package references are maintained in
[CanDoItAll.Mcp.DotNetWatch.csproj](CanDoItAll.Mcp.DotNetWatch.csproj).

## Architecture Notes

This project is an adapter for agent-facing tooling. It must stay thin over canonical app/module services and should not become a second implementation of product behavior.

## Tailwind Companion

When a source-watch session starts for a project and `Process:TailwindAutoDetect` is
enabled, the backend looks for the nearest npm package with a `tailwindcss` or
`@tailwindcss/cli` dependency and runs that package's standard watch script next to
`dotnet watch`. It does not watch source files or rebuild CSS itself; Tailwind's own
watcher does that incrementally.

- The script named by `Process:TailwindWatchScript` (default `watch`) is used when it runs
  a Tailwind watch directly or through other scripts of the package, for example
  `concurrently 'npm run watch:app' ...`. Otherwise the backend picks another Tailwind
  watch script, preferring one whose `-o` output is inside the project being run. A
  package without any Tailwind watch script gets no companion.
- The script runs as `npm run <script>` in the package directory, through `node` and
  npm's `npm-cli.js` when both are on the backend `PATH`, and under the backend's process
  supervisor with an open standard input. `tailwindcss --watch` exits when its standard
  input ends, so the watch lives as long as the backend owns it. Scripts that use
  `--watch=always` opt out of that and survive a crashed backend.
- A `Done in …` line marks a finished build. A build that printed an error first is
  reported as `BuildFailed` while the watch keeps running; the next clean build restores
  `Ready`. An exited watch is restarted after 2 seconds, doubling up to 1 minute while it
  keeps failing.
- Missing dependencies (`node_modules/.bin/tailwindcss`) are reported as `Unavailable`;
  run `npm install` in the package and restart the app session.

`candoitall_app_status` reports the companion as `tailwind` (state, summary, script,
process id, last build, build and restart counts, last error), and the backend dashboard
shows it on the session card. Tailwind output is appended to the session log with the
`[tailwind]` prefix.

## Related Docs

- [Repository overview](../../README.md)
- [Architecture and ownership](../../docs/architecture-and-ownership.md)
