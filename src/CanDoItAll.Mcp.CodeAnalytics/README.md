# CanDoItAll.Mcp.CodeAnalytics

## Purpose

MCP adapter for source, solution, symbol, and code-analysis inspection over the local CanDoItAll workspace.

## Project Type

- SDK: `Microsoft.NET.Sdk`
- Target framework(s): `net10.0`
- Validation command:

```powershell
dotnet build src/CanDoItAll.Mcp.CodeAnalytics/CanDoItAll.Mcp.CodeAnalytics.csproj
```

## References

Project references:

- `../CanDoItAll.Mcp.Core/CanDoItAll.Mcp.Core.csproj`
- `../../../CanDoItAll.CodeAnalysis/src/CanDoItAll.CodeAnalytics.Abstractions/CanDoItAll.CodeAnalytics.Abstractions.csproj`
- `../../../CanDoItAll.CodeAnalysis/src/CanDoItAll.CodeAnalytics.Analysis/CanDoItAll.CodeAnalytics.Analysis.csproj`
- `../../../CanDoItAll.CodeAnalysis/src/CanDoItAll.CodeAnalytics.Application/CanDoItAll.CodeAnalytics.Application.csproj`
- `../../../CanDoItAll.CodeAnalysis/src/CanDoItAll.CodeAnalytics.Facts/CanDoItAll.CodeAnalytics.Facts.csproj`
- `../../../CanDoItAll.CodeAnalysis/src/CanDoItAll.CodeAnalytics.Rendering/CanDoItAll.CodeAnalytics.Rendering.csproj`
- `../../../CanDoItAll.CodeAnalysis/src/CanDoItAll.CodeAnalytics.Storage/CanDoItAll.CodeAnalytics.Storage.csproj`
- `../../../CanDoItAll.CodeAnalysis/src/CanDoItAll.CodeAnalytics.Workspace/CanDoItAll.CodeAnalytics.Workspace.csproj`

Framework references:

- None

Direct package references are maintained in
[CanDoItAll.Mcp.CodeAnalytics.csproj](CanDoItAll.Mcp.CodeAnalytics.csproj).

## Architecture Notes

This project is an adapter for agent-facing tooling. It must stay thin over canonical app/module services and should not become a second implementation of product behavior.

## Related Docs

- [Repository overview](../../README.md)
- [Architecture and ownership](../../docs/architecture-and-ownership.md)
