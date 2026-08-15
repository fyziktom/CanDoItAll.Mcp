# CanDoItAll.Mcp.CodeAnalytics.Tests

Focused adapter and stdio protocol coverage for the CodeAnalytics MCP server.

Run:

```powershell
dotnet test tests/CanDoItAll.Mcp.CodeAnalytics.Tests/CanDoItAll.Mcp.CodeAnalytics.Tests.csproj
```

The protocol fixture uses the sibling `CanDoItAll.CodeAnalysis` repository. Tests must
keep actual changed diff seeds separate from context-only paths and must cover test
projects loaded through a separate test solution.
