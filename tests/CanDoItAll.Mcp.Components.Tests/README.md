# CanDoItAll.Mcp.Components.Tests

## Purpose

Test project for the corresponding CanDoItAll runtime, module, component, MCP, or integration behavior.

## Project Type

- SDK: `Microsoft.NET.Sdk`
- Target framework(s): `net10.0`
- Validation command:

```powershell
dotnet test tests/CanDoItAll.Mcp.Components.Tests/CanDoItAll.Mcp.Components.Tests.csproj --configuration Release -p:UseLocalCanDoItAllLibraries=true --filter "FullyQualifiedName~ComponentCatalogServiceTests|FullyQualifiedName~ComponentsToolsTests|FullyQualifiedName~ComponentsMcpProtocolTests" /m:1
```

Use source mode for the coordinated UI refactor until matching Components packages are published. Discovery with the same filter and `--list-tests` must find 39 cases. The focused gate covers reflected input/button/tab contracts, human-first avatar recommendations, organized stylesheet sources, curated examples, and the actual stdio tool protocol. Idle-shutdown tests remain a separate topic.

## References

Project references:

- `../../src/CanDoItAll.Mcp.Components/CanDoItAll.Mcp.Components.csproj`

Framework references:

- None

Direct package references are maintained in
[CanDoItAll.Mcp.Components.Tests.csproj](CanDoItAll.Mcp.Components.Tests.csproj).

## Architecture Notes

This project is an adapter for agent-facing tooling. It must stay thin over canonical app/module services and should not become a second implementation of product behavior.

## Related Docs

- [Repository overview](../../README.md)
- [Architecture and ownership](../../docs/architecture-and-ownership.md)
