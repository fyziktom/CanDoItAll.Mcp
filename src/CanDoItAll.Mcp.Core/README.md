# CanDoItAll.Mcp.Core

## Purpose

Shared MCP transport, contracts, options, logging, and tool infrastructure used by CanDoItAll MCP servers.

## Project Type

- SDK: `Microsoft.NET.Sdk`
- Target framework(s): `net10.0`
- Validation command:

```powershell
dotnet build src/CanDoItAll.Mcp.Core/CanDoItAll.Mcp.Core.csproj
```

## References

Project references:

- None

Framework references:

- None

Direct package references are maintained in
[CanDoItAll.Mcp.Core.csproj](CanDoItAll.Mcp.Core.csproj).

## Architecture Notes

This project is an adapter for agent-facing tooling. It must stay thin over canonical app/module services and should not become a second implementation of product behavior.

## Related Docs

- [Repository overview](../../README.md)
- [Architecture and ownership](../../docs/architecture-and-ownership.md)
