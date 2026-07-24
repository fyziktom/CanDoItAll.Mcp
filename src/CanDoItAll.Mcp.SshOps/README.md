# CanDoItAll.Mcp.SshOps

## Purpose

MCP adapter for controlled SSH operations and remote execution workflows.

## Project Type

- SDK: `Microsoft.NET.Sdk`
- Target framework(s): `net10.0`
- Validation command:

```powershell
dotnet build src/CanDoItAll.Mcp.SshOps/CanDoItAll.Mcp.SshOps.csproj
```

## References

Project references:

- `..//CanDoItAll.Mcp.Core//CanDoItAll.Mcp.Core.csproj`

Framework references:

- None

Direct package references are maintained in
[CanDoItAll.Mcp.SshOps.csproj](CanDoItAll.Mcp.SshOps.csproj).

## Architecture Notes

This project is an adapter for agent-facing tooling. It must stay thin over canonical app/module services and should not become a second implementation of product behavior.

## Related Docs

- [Repository overview](../../README.md)
- [Architecture and ownership](../../docs/architecture-and-ownership.md)
