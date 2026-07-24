# CanDoItAll.Mcp.ToolHarness

## Purpose

Command-line harness for exercising MCP tools during local development and diagnostics.

## Project Type

- SDK: `Microsoft.NET.Sdk`
- Target framework(s): `net10.0`
- Validation command:

```powershell
dotnet build tools/CanDoItAll.Mcp.ToolHarness/CanDoItAll.Mcp.ToolHarness.csproj
```

## References

Project references:

- None

Framework references:

- None

Direct package references are maintained in
[CanDoItAll.Mcp.ToolHarness.csproj](CanDoItAll.Mcp.ToolHarness.csproj).

## Architecture Notes

This project is an adapter for agent-facing tooling. It must stay thin over canonical app/module services and should not become a second implementation of product behavior.

## Related Docs

- [Repository overview](../../README.md)
- [Architecture and ownership](../../docs/architecture-and-ownership.md)
