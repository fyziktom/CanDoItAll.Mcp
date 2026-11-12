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

Direct package references are maintained in
[CanDoItAll.Mcp.Components.csproj](CanDoItAll.Mcp.Components.csproj).

## Architecture Notes

This project is an adapter for agent-facing tooling. It must stay thin over canonical app/module services and should not become a second implementation of product behavior.

Public component types, parameters, enum values, and defaults are reflected from the assemblies referenced by this project. Package mode remains pinned to the published library version. For coordinated development against the sibling Components checkout, explicitly use `-p:UseLocalCanDoItAllLibraries=true` during restore and build. `CanDoItAllComponentsRoot` can override the sibling location. This mode references the sandbox catalog library without starting or referencing its executable host.

The `ui-refactoring-v2` contract and its focused tests require source mode until matching packages are published. Do not infer a new API from the repository's package version alone or reuse an `obj/project.assets.json` restored in a different mode. Rebuild and restart the MCP after changing the referenced library contract; source-derived usage examples alone do not prove that the running server reflects it.

```powershell
dotnet build src/CanDoItAll.Mcp.Components/CanDoItAll.Mcp.Components.csproj --configuration Release -p:UseLocalCanDoItAllLibraries=true /m:1
dotnet test tests/CanDoItAll.Mcp.Components.Tests/CanDoItAll.Mcp.Components.Tests.csproj --configuration Release -p:UseLocalCanDoItAllLibraries=true --filter "FullyQualifiedName~ComponentCatalogServiceTests|FullyQualifiedName~ComponentsToolsTests|FullyQualifiedName~ComponentsMcpProtocolTests" /m:1
```

Keep `Catalog.SandboxRoot` pointed at `samples/CanDoItAll.Components.Sandbox` in the source checkout. The modern catalog uses named examples and routes such as `/navigation#tabs`; the MCP retains `scenario` as the example title for existing protocol consumers.

## Agent Usage Guidance

This MCP intentionally excludes every WebGL library and surface. For all other `CanDoItAll.Components.*` usage, agents must query this MCP before selecting components, adding services/assets, or creating structural CSS.

Use this sequence:

1. `components_libraries_list` for package roles, service registration, generated assets, styles, dependencies, and proof routes.
2. `components_recommend` with the concrete use case to obtain selected components, alternatives, setup, composition rules, and follow-up calls.
3. `app_shell_guide_get` before creating or changing an application layout or primary menu.
4. `component_get`, `component_usage_examples`, and `component_examples` for every shortlisted component.
5. `component_css_tokens_get` only after the structural choice is settled.

`components_recommend` is also the compact-composition decision surface. Give it the real task and constraints, such as "item collection with create/edit dialogs and supporting metrics," instead of asking only for a component name. Each recommended component includes its own composition rules, and the response composition plan distinguishes task-first operational pages from sparse metric-first pages.

For operational pages, keep the list, form, table, canvas, or other primary work surface dominant. Prefer a compact `PageHeader`, `LayoutGap.Small` or `LayoutGap.Medium`, `CompactStatStrip` for supporting numbers, `Tabs` for mutually exclusive modes, and a correctly sized `Dialog` for bounded create/edit work. Do not repeat page orientation in a second hero or spend a full card on every short number. A host can provide `IPageHeaderOutlet` to publish the page's `PageHeaderPresentation` into a shared toolbar; the page remains the action owner, and disposal clears only that publisher's header.

Each outlet registration invalidates the toolbar renderers, including when existing fragments capture changed model values. Subscribe only the toolbar, help, and statistics outlets to that notification; rerendering the routed body from it creates a feedback loop.

The refactored input family is `TextInput`, `TextAreaInput`, `NumberInput<TValue>`, and `SelectInput<TValue>`. Bind model fields with `@bind-Value` to preserve `ValueExpression` and `EditContext` validation. `SelectInput` takes option elements as child content; the former `DropDown` data collection and `DropDownOption` contracts are removed. Use `Look` and `FormDensity` for appearance. Buttons use `ButtonTone`, `Variant`, `Size`, `Density`, and `Rounded`.

Tabs use `TabsDensity` and `TabsRounded` instead of `TabsVariant`. `NoContent` supports navigation-only strips, `TabsItem.Link` supplies route links, and `PanelOverflowMode` makes content scrolling explicit. Keep opened workspace tabs visible in the application shell; a single horizontally scrolling strip preserves the work area when many items are open. Verify active-tab visibility, long labels, keyboard focus, switching, and close actions.

`Avatar` retains bundled human portraits as the primary identity for agent colleagues. Preserve explicit image selections. Offer the bundled `robot-01.svg` through `robot-08.svg` assets or `RoboAvatar` as an explicit secondary choice; `HomoAvatar` is a separate optional illustrated human style. Do not change a colleague's portrait merely because a new generated style exists.

`SelectionListItem` has two explicit interaction modes. Without `OnSelect`, its content is a noninteractive surface. With `OnSelect`, the content region becomes a selection button; `Disabled` affects only that button. The `Actions` fragment remains consumer-owned, so each action must receive its own disabled state.

`ThemeHost` emits `data-ui-theme`. The current color contract lives in the Components repository's `Tailwind/theme.css`: use structural `--tone-*` variables and pair semantic background, foreground, and border colors. The `--color-chrome-*` scale reverses in dark mode, while `--color-neutral-*` remains fixed for intentionally constant surfaces and shadows. Spacing, shape, and shadows still use `--cad-space-*`, `--cad-radius-*`, and `--cad-shadow-*`. Load app-owned overrides after BaseLib CSS, pass `ColorScheme` for custom theme keys when needed, and verify both switching directions in the same browser session, including open overlays and form controls.

Overlay windows and toolboxes consume `--ui-overlay-*` colors. BaseLib resolves their neutral aliases and `--ui-overlay-{info,secondary,success,warning,danger}-{bg,fg,border}` status tokens at each theme boundary; standalone OverlayLib styles retain literal defaults when BaseLib styles are absent. Override status colors as a matching set. Checkboxes use `--ui-checkbox-checkmark` and `--ui-checkbox-indeterminate` image tokens for their light and dark glyphs, including document-root theme boundaries.

Charts require `AddCanDoItAllCharts()`, `ChartsHeadAssets` in the head, and `ChartsBodyAssets` once before Blazor starts. The body assets load BaseLib's shared theme script followed by the chart-specific theme bridge. Set `IncludeThemeTokens="false"` only when an earlier body-assets component, such as `CanvasLibBodyAssets`, already loads the shared script. Verify the mounted chart's legends, axes, grid lines, and tooltips after switching light to dark and back; the stylesheet alone does not connect chart colors to the application theme.

Choose content-aware sizes rather than universal large defaults:

- `TextAreaInput Size="TextAreaSize.Compact"` for short comments, `Standard` for ordinary descriptions, and `Extended` for substantial prose; use `Rows` only as a deliberate override.
- `ModalSize.Compact` for a short single-column task, `Medium` for an ordinary grouped form, `Wide` for genuine multi-column review, and `Full` only for workbench-like tasks. Use `DenseChrome` for short focused dialogs.
- `CardGrid` plus structured `Card` media, header, tags, body, and actions for visual collections. Prefer selection rows or `DataGrid` when records are primarily textual or comparable.
- `CompactStatStrip` or a badge when metrics support another task. Reserve `StatsGrid`, `MetricCard`, and `SummaryTiles` for metric-first pages with enough available space.

`component_usage_examples` returns a bounded multi-line opening tag so important parameters are not lost when Razor attributes span lines. Curated sandbox examples are ordered before incidental product and module usage; treat them as composition proof, then confirm the behavior on the consumer's target viewport.

`app_shell_guide_get` is the canonical BaseLib application-shell path: `ThemeHost` -> `Layout` -> `SideMenu` -> `Body`, with routed pages using `PageScaffold`. It defines scroll ownership, route synchronization, menu item sources, height-measured More overflow, bottom utilities, responsive behavior, and the single mounted `DialogHost`, `Tooltip`, and `Notification` hosts.

`SideMenu` is a typed selection model, not an anchor collection. Give it a stable `MenuId`, map `ItemSelected` or `SideMenuService` selections from typed IDs/payloads to application navigation, and do not wrap it in `Sidebar` or create a second mobile menu.

For overlay services, query:

- `component_get("Notification")` before adding or moving toast feedback.
- `component_get("Tooltip")` before opening tooltip content through `TooltipService`.
- `component_get("TooltipTarget")` before wrapping declarative hover or focus help.

Notification position should be chosen by what must remain unobscured. Keep `TopRight` for ordinary desktop feedback, move to `BottomCenter` when top chrome is dense or mobile reach matters, use side positions near rails or list/detail panes, and reserve `TopCenter` for global but still non-blocking messages. If the state requires a choice, use `DialogService` or an inline `Alert` instead of a notification.

Tooltip position should be chosen by the trigger's edge pressure and the next likely action. Prefer `Top` or `Right` when there is room, switch to `Bottom` near the top edge, use `Top` near lower toolbars, use `Left` or `Right` for dense inline controls, and use corner or edge placements near viewport, card, toolbar, or panel corners.

When an agent uses non-default notification or tooltip placement, it should validate behavior with Playwright at the consumer's supported desktop viewports. Reusable BaseLib cross-viewport coverage remains a separate library concern.

## Related Docs

- [Repository overview](../../README.md)
- [Architecture and ownership](../../docs/architecture-and-ownership.md)
