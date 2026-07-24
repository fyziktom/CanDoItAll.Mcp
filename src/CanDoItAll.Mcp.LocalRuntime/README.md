# CanDoItAll.Mcp.LocalRuntime

## Purpose

MCP adapter for local runtime helper operations.

## Project Type

- SDK: `Microsoft.NET.Sdk`
- Target framework(s): `net10.0`
- Validation command:

```powershell
dotnet build src/CanDoItAll.Mcp.LocalRuntime/CanDoItAll.Mcp.LocalRuntime.csproj
```

## References

Project references:

- `..//CanDoItAll.Mcp.Core//CanDoItAll.Mcp.Core.csproj`

Framework references:

- None

Direct package references are maintained in
[CanDoItAll.Mcp.LocalRuntime.csproj](CanDoItAll.Mcp.LocalRuntime.csproj).

## Architecture Notes

This project is an adapter for agent-facing tooling. It must stay thin over canonical app/module services and should not become a second implementation of product behavior.

## Related Docs

- [Repository overview](../../README.md)
- [Architecture and ownership](../../docs/architecture-and-ownership.md)
