# CanDoItAll.Mcp.Components

## Purpose

MCP adapter for discovering, selecting, configuring, and proving shared non-WebGL CanDoItAll component-library capabilities.

## Project Type

- SDK: `Microsoft.NET.Sdk`
- Target framework(s): `net10.0`
- Validation command:

```powershell
dotnet build src/CanDoItAll.Mcp.Components/CanDoItAll.Mcp.Components.csproj
```

## References

Component packages indexed by the server:

- `CanDoItAll.Components.BaseLib`
- `CanDoItAll.Components.CanvasLib`
- `CanDoItAll.Components.Charts`
- `CanDoItAll.Components.Common`
- `CanDoItAll.Components.Mermaid`
- `CanDoItAll.Components.OverlayLib`
- `CanDoItAll.Components.QRCode`
- `CanDoItAll.Components.Sandbox` for curated proof routes and examples

Project references:

- `../CanDoItAll.Mcp.Core/CanDoItAll.Mcp.Core.csproj`

Framework references:

- `Microsoft.AspNetCore.App`

Direct package references:

- `ModelContextProtocol (1.1.0)`

## Architecture Notes

This project is an adapter for agent-facing tooling. It must stay thin over canonical app/module services and should not become a second implementation of product behavior.

## Agent Usage Guidance

This MCP intentionally excludes every WebGL library and surface. For all other `CanDoItAll.Components.*` usage, agents must query this MCP before selecting components, adding services/assets, or creating structural CSS.

Use this sequence:

1. `components_libraries_list` for package roles, service registration, generated assets, styles, dependencies, and proof routes.
2. `components_recommend` with the concrete use case to obtain selected components, alternatives, setup, composition rules, and follow-up calls.
3. `app_shell_guide_get` before creating or changing an application layout or primary menu.
4. `component_get`, `component_usage_examples`, and `component_examples` for every shortlisted component.
5. `component_css_tokens_get` only after the structural choice is settled.

`app_shell_guide_get` is the canonical BaseLib application-shell path: `ThemeHost` -> `Layout` -> `SideMenu` -> `Body`, with routed pages using `PageScaffold`. It defines scroll ownership, route synchronization, menu item sources, height-measured More overflow, bottom utilities, responsive behavior, and the single mounted `DialogHost`, `Tooltip`, and `Notification` hosts.

`SideMenu` is a typed selection model, not an anchor collection. Give it a stable `MenuId`, map `ItemSelected` or `SideMenuService` selections from typed IDs/payloads to application navigation, and do not wrap it in `Sidebar` or create a second mobile menu.

For overlay services, query:

- `component_get("Notification")` before adding or moving toast feedback.
- `component_get("Tooltip")` before opening tooltip content through `TooltipService`.
- `component_get("TooltipTarget")` before wrapping declarative hover or focus help.

Notification position should be chosen by what must remain unobscured. Keep `TopRight` for ordinary desktop feedback, move to `BottomCenter` when top chrome is dense or mobile reach matters, use side positions near rails or list/detail panes, and reserve `TopCenter` for global but still non-blocking messages. If the state requires a choice, use `DialogService` or an inline `Alert` instead of a notification.

Tooltip position should be chosen by the trigger's edge pressure and the next likely action. Prefer `Top` or `Right` when there is room, switch to `Bottom` near the top edge, use `Top` near lower toolbars, use `Left` or `Right` for dense inline controls, and use corner or edge placements near viewport, card, toolbar, or panel corners.

When an agent uses non-default notification or tooltip placement, it should validate the sandbox behavior with Playwright at the relevant desktop and mobile viewport sizes.

## Related Docs

- Repository overview: `README.md` at the repo root
- Current architecture: `docs/architecture-beta.md`
