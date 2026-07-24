# CanDoItAll.Mcp.DotNetWatch.IntegrationTests

## Purpose

MCP adapter for DotNetWatch.IntegrationTests capabilities.

## Project Type

- SDK: `Microsoft.NET.Sdk`
- Target framework(s): `net10.0`
- Validation command:

```powershell
dotnet test tests/CanDoItAll.Mcp.DotNetWatch.IntegrationTests/CanDoItAll.Mcp.DotNetWatch.IntegrationTests.csproj
```

## References

Project references:

- `../../src/CanDoItAll.Mcp.DotNetWatch/CanDoItAll.Mcp.DotNetWatch.csproj`

Framework references:

- None

Direct package references are maintained in
[CanDoItAll.Mcp.DotNetWatch.IntegrationTests.csproj](CanDoItAll.Mcp.DotNetWatch.IntegrationTests.csproj).

## Architecture Notes

This project is an adapter for agent-facing tooling. It must stay thin over canonical app/module services and should not become a second implementation of product behavior.

## Related Docs

- [Repository overview](../../README.md)
- [Architecture and ownership](../../docs/architecture-and-ownership.md)
- [Build, test, and resetup](../../docs/build-test-and-resetup.md)
