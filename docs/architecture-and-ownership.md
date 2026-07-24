# Architecture And Ownership

## Repository Role

CanDoItAll.Mcp is the adapter and local-operations boundary between MCP clients and
CanDoItAll product capabilities. MCP projects may expose, coordinate, and validate
product behavior, but they do not become a second implementation of that behavior.

The shared libraries have two narrow roles:

- `CanDoItAll.Mcp.Core` owns MCP hosting, protocol envelopes, identity, logging, HTTP
  probes, and concurrency primitives used by more than one server.
- `CanDoItAll.Mcp.LocalRuntime` owns local process and registry primitives used by
  runtime-oriented servers.

Each server owns its MCP tool contract and adapter logic. Product libraries remain the
source of truth for component behavior and code analysis.

## Ownership Boundaries

| Concern | Owning repository |
|---|---|
| MCP source, tests, server contracts, and companion tools | `CanDoItAll.Mcp` |
| Machine settings, installed binaries, shadow artifacts, resetup, and Codex configuration | `CanDoItAll` |
| CodeAnalytics domain and application services | `CanDoItAll.CodeAnalysis` |
| Component implementations and packages | `CanDoItAll.Components` |
| Shared repository-family standards and templates | `CanDoItAll.SharedInfo` |

Sibling repositories are dependencies, not writable subtrees. A change spanning an
ownership boundary must be explicitly coordinated and validated in every named
repository.

## Distribution Boundaries

This repository is distributed through the main repository's machine resetup workflow.
It does not currently publish NuGet packages, so `Directory.Build.props` disables packing
by default. Server settings and installed outputs remain outside this repository and must
not be copied into source control.

This repository does not own Docker or Compose assets. Adding containers would require a
concrete runtime and operations requirement, not an empty baseline folder.

## Current Standards-Adoption Exceptions

These exceptions are documented rather than silently normalized because resolving them
requires changes or release artifacts from sibling repositories.

### CodeAnalytics Source References

`CanDoItAll.Mcp.CodeAnalytics` references projects under the sibling
`CanDoItAll.CodeAnalysis` checkout. This conflicts with the preferred package boundary
for independently shipping repositories and means a clone cannot build that server by
itself. The root solution lists the currently referenced sibling projects under an
`external/CanDoItAll.CodeAnalysis` solution folder so a solution-level `Release` build
maps their configurations consistently; those projects remain sibling-owned and
read-only from this repository.

Exit condition: publish or otherwise provide reproducible versioned CodeAnalytics
packages, replace the sibling project references with package references, and validate
the packed dependency graph before removing the sibling prerequisite and external
solution entries.

### Components Local Package Feed

`NuGet.config` includes `..\CanDoItAll\ExternalPackages` because current Components
packages are produced into that workspace-owned feed. The source list is deterministic,
but the feed is not an independently reproducible release source.

Exit condition: publish the required Components packages to an approved stable feed or
add a separately owned, reproducible package-production contract; then remove the sibling
feed prerequisite.

### Companion Tool Paths

Compiled companion tools currently live directly below `tools` instead of a lower-case
purpose category such as `tools/dev`. Those paths are consumed by the main repository's
resetup workflow.

Exit condition: migrate the tool folders and every installer/settings reference in one
coordinated multi-repository change. Until then, the stable install contract takes
precedence over a repository-only move.

### Repository-Local CI

The repository has no CI workflow or CI badge while a clean build depends on the two
sibling contracts above. The repository-owned validation script is the local gate.

Exit condition: make dependency acquisition reproducible in an isolated checkout or
define a reviewed multi-repository CI checkout contract, then add the workflow and its
badge together.
