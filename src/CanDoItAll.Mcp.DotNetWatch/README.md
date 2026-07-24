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

## Related Docs

- [Repository overview](../../README.md)
- [Architecture and ownership](../../docs/architecture-and-ownership.md)
