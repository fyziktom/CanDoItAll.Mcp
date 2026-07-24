# CanDoItAll.Mcp.DotNetWatch.Tests

## Purpose

Test project for the corresponding CanDoItAll runtime, module, component, MCP, or integration behavior.

## Project Type

- SDK: `Microsoft.NET.Sdk`
- Target framework(s): `net10.0`
- Validation command:

```powershell
dotnet test tests/CanDoItAll.Mcp.DotNetWatch.Tests/CanDoItAll.Mcp.DotNetWatch.Tests.csproj
```

## References

Project references:

- `../../src/CanDoItAll.Mcp.DotNetWatch/CanDoItAll.Mcp.DotNetWatch.csproj`

Framework references:

- None

Direct package references are maintained in
[CanDoItAll.Mcp.DotNetWatch.Tests.csproj](CanDoItAll.Mcp.DotNetWatch.Tests.csproj).

## Architecture Notes

This project is an adapter for agent-facing tooling. It must stay thin over canonical app/module services and should not become a second implementation of product behavior.

## Related Docs

- [Repository overview](../../README.md)
- [Architecture and ownership](../../docs/architecture-and-ownership.md)
