using CanDoItAll.Mcp.Core.Contracts;

namespace CanDoItAll.Mcp.Components.Catalog;

public sealed partial class ComponentCatalogService
{
    private static readonly IReadOnlyList<string> AppShellModes =
    [
        "full-height",
        "standard",
        "focus-workbench"
    ];

    private static readonly IReadOnlyList<ComponentLibraryTemplate> LibraryTemplates =
    [
        new(
            "Common",
            "CanDoItAll.Components.Common",
            "Dependency-light layout enums, attribute helpers, and CSS class composition primitives shared by the rendering libraries.",
            true,
            [],
            ["Typed `Orientation`, `AlignItems`, `JustifyContent`, and `FlexWrap` values.", "Shared non-rendering component infrastructure."],
            ["Rendering product UI directly.", "Adding app-specific domain contracts."],
            [],
            [],
            [],
            [],
            null,
            ["Most applications receive Common transitively through BaseLib; reference it directly only when non-rendering primitives are needed."]),
        new(
            "BaseLib",
            "CanDoItAll.Components.BaseLib",
            "Primary reusable app UI: theme, shell, layout, navigation, forms, data display, feedback, dialogs, cards, and typography.",
            true,
            ["Common"],
            ["Ordinary application UI.", "Responsive app shells and page composition.", "Shared forms, navigation, feedback, and data-display patterns."],
            ["Canvas graph runtimes.", "Floating workbench windows that require drag and resize.", "Page-local structural CSS that shared parameters already express."],
            ["builder.Services.AddCanDoItAllBaseLib();"],
            [],
            [],
            ["_content/CanDoItAll.Components.BaseLib/css/material-icons.css", "_content/CanDoItAll.Components.BaseLib/css/output.css"],
            "/groups/foundations",
            ["Wrap the interactive app in `<ThemeHost>`.", "Mount `<DialogHost />`, `<Tooltip />`, and `<Notification />` once in the interactive layout."]),
        new(
            "CanvasLib",
            "CanDoItAll.Components.CanvasLib",
            "Typed canvas, graph, calendar, preview, accessibility, and workbench runtime surfaces.",
            true,
            ["BaseLib", "Common", "OverlayLib"],
            ["Graph and canvas authoring.", "Desktop-first workbenches.", "Canvas calendars and typed canvas runtime events."],
            ["Generic app shells and forms.", "WebGL scenes or WebGL playback.", "Hand-loading generated runtime scripts."],
            [],
            ["<CanvasLibHeadAssets />"],
            ["<CanvasLibBodyAssets IncludeRuntimeAssets=\"true\" IncludePreviewAssets=\"...\" IncludeCalendarAssets=\"...\" />"],
            [
                "_content/CanDoItAll.Components.CanvasLib/css/workbench/shell/01-layout-and-shell.css",
                "_content/CanDoItAll.Components.CanvasLib/css/workbench/chrome/02-toolbar-and-windows.css",
                "_content/CanDoItAll.Components.CanvasLib/css/workbench/panels/03-help-settings-and-preview.css",
                "_content/CanDoItAll.Components.CanvasLib/css/workbench/scene/04-scene-and-nodes.css",
                "_content/CanDoItAll.Components.CanvasLib/css/workbench/overlays/05-overlays-and-composer.css",
                "_content/CanDoItAll.Components.CanvasLib/css/workbench/responsive/06-motion-and-responsive.css"
            ],
            "/groups/canvas",
            ["Generated asset components load OverlayLib first and preserve verified script order.", "Enable preview and calendar assets only when those surfaces are used."]),
        new(
            "Charts",
            "CanDoItAll.Components.Charts",
            "Typed CanDoItAll chart boundary over Blazor ApexCharts.",
            true,
            [],
            ["Operational line, area, bar, pie, and donut charts.", "Shared chart series, options, palette, axis, and legend behavior."],
            ["Raw Apex component markup in product pages.", "Decorative charts with no operational question."],
            ["builder.Services.AddCanDoItAllCharts();"],
            ["<ChartsHeadAssets />"],
            [],
            ["_content/Blazor-ApexCharts/css/apexcharts.css"],
            "/groups/charts",
            ["Use `CdaChart`, `CdaChartSeries`, `CdaChartPoint`, and `CdaChartOptions` as the renderer-neutral boundary."]),
        new(
            "Mermaid",
            "CanDoItAll.Components.Mermaid",
            "Typed Mermaid diagram rendering with vendored assets, normalization, pan/zoom, node clicks, and render diagnostics.",
            true,
            [],
            ["Flowcharts and architecture diagrams.", "Diagram interactions and normalized Mermaid errors."],
            ["Direct Mermaid.js imports in pages.", "Untrusted source with loose security or HTML labels."],
            ["builder.Services.AddCanDoItAllMermaid();"],
            ["<MermaidHeadAssets />"],
            [],
            ["_content/CanDoItAll.Components.Mermaid/CanDoItAll.Components.Mermaid.bundle.scp.css"],
            "/groups/mermaid",
            ["Keep the default strict security level and disabled HTML labels unless the source is fully trusted."]),
        new(
            "OverlayLib",
            "CanDoItAll.Components.OverlayLib",
            "Generic bounded floating-window and component-toolbox surfaces with drag, resize, minimize, reset, hide, and show behavior.",
            true,
            ["BaseLib"],
            ["Floating inspectors and tool windows.", "Searchable component toolboxes inside bounded workbench containers."],
            ["Ordinary dialogs or notifications.", "Permanent page layout.", "Canvas-specific state without adapting it into `OverlayWindowState`."],
            [],
            ["<OverlayLibHeadAssets />"],
            ["<OverlayLibBodyAssets IncludeRuntimeAssets=\"true\" />"],
            ["_content/CanDoItAll.Components.OverlayLib/css/overlay-window.css"],
            "/groups/canvas",
            ["Set `ContainerSelector` and `SafeTopSelector` for the real bounded host.", "CanvasLib asset components already include OverlayLib assets."]),
        new(
            "QRCode",
            "CanDoItAll.Components.QRCode",
            "Generic QR payload rendering, dialog actions, camera scanner shell, and manual decoded-text fallback.",
            true,
            ["BaseLib"],
            ["Displaying generic QR payloads.", "Opening QR dialogs from page headers.", "Camera or manual QR scanning."],
            ["Embedding ledger, address, transaction, ticket, or other domain semantics in the shared library.", "Assuming camera access is always available."],
            ["builder.Services.AddCanDoItAllBaseLib();", "builder.Services.AddCanDoItAllQrCode();"],
            [],
            [],
            ["_content/CanDoItAll.Components.QRCode/CanDoItAll.Components.QRCode.bundle.scp.css"],
            "/groups/qr",
            ["Mount `<DialogHost />` once before using `QrCodeButton` or `QrScanButton`.", "Provide a manual decoded-text path and handle camera permission or availability failures."]),
        new(
            "Sandbox",
            "CanDoItAll.Components.Sandbox",
            "Interactive preview and regression host that supplies curated examples and proof routes to this MCP.",
            false,
            ["BaseLib", "CanvasLib", "Charts", "Common", "Mermaid", "OverlayLib", "QRCode"],
            ["Visual proof, behavior exploration, and regression coverage."],
            ["Product runtime references.", "Owning domain-specific components or application behavior."],
            [],
            [],
            [],
            [],
            "/groups/coverage",
            ["Use sandbox routes for browser proof after choosing shared structure; do not copy sandbox-only shell CSS into product pages."])
    ];

    private static readonly IReadOnlyList<ComponentDecisionGuide> DecisionGuides =
    [
        new(
            "app-shell",
            "Responsive application shell and primary navigation",
            ["app shell", "application shell", "main layout", "side menu", "side navigation", "navigation rail", "primary navigation"],
            [
                new("ThemeHost", "Theme boundary", "Applies the BaseLib semantic theme contract to the complete interactive application."),
                new("Layout", "Application frame", "Owns the viewport or natural-height flex shell without page-local structural CSS."),
                new("SideMenu", "Primary navigation", "Provides responsive rail, collapse, measured overflow, bottom utilities, panels, persistence, and small-screen top-menu behavior."),
                new("Body", "Scroll owner", "Provides the main content region and makes scroll ownership explicit."),
                new("PageScaffold", "Page boundary", "Gives each routed page its header, lead, width, rail, and full-height behavior.")
            ],
            [
                new("Sidebar", "Generic rail", "Use only when the rail is arbitrary content; do not wrap `SideMenu` in another sidebar."),
                new("PageShell", "Simple page wrapper", "Use for a basic centered document page when `PageScaffold` slots are unnecessary.")
            ],
            ["Call `app_shell_guide_get` and use its complete host, menu, scrolling, and responsive rules."]),
        new(
            "responsive-layout",
            "Responsive page and section composition",
            ["responsive layout", "responsive columns", "two column", "three column", "grid", "dashboard layout", "split layout"],
            [
                new("Grid", "Track system", "Use explicit responsive tracks, gaps, and section composition."),
                new("Row", "Nested track band", "Lets sibling columns inherit or override grid tracks and collapse responsively."),
                new("Column", "Grid cell", "Combines responsive span placement with local flex alignment."),
                new("Stack", "Linear flow", "Handles vertical or horizontal content flow before a two-dimensional grid is needed.")
            ],
            [
                new("Split", "Responsive two-sided row", "Use for two regions that stack at one breakpoint rather than a full track system."),
                new("Cluster", "Wrapping inline group", "Use for compact actions, chips, or metadata that wrap naturally.")
            ],
            ["Start with `Stack`; move to `Grid` or `Row` plus `Column` only when tracks or breakpoint changes are required."]),
        new(
            "forms",
            "Forms and data entry",
            ["form", "data entry", "settings fields", "edit fields", "validation", "input fields"],
            [
                new("FormStack", "Form flow", "Owns the vertical rhythm for a form."),
                new("FormSection", "Form grouping", "Groups related fields under a clear section boundary."),
                new("FormRow", "Field pairing", "Handles common paired or evenly split field layouts."),
                new("FormField", "Field semantics", "Keeps labels, help, validation, and control alignment together.")
            ],
            [
                new("Fieldset", "Semantic group", "Use for a related set of controls with one legend."),
                new("InlineActions", "Form actions", "Use for compact actions adjacent to fields.")
            ],
            ["Compose shared inputs inside `FormField`; avoid raw label/input wrappers when BaseLib already supplies the behavior."]),
        new(
            "data-grid",
            "Tabular data with sorting and selection",
            ["data grid", "table", "sortable", "paging", "pagination", "tabular", "column filter"],
            [
                new("DataGrid", "Table runtime", "Provides the shared accessible data-grid behavior and visual contract."),
                new("DataGridColumn", "Typed column", "Defines data, templates, sorting, and column behavior inside `DataGrid`.")
            ],
            [
                new("ListDetailShell", "Selection workspace", "Use when choosing one record and operating on a persistent detail pane matters more than rows and columns."),
                new("FactTable", "Compact facts", "Use for label/value facts rather than an interactive dataset.")
            ],
            ["Use a grid for comparable rows and columns; use list-detail when selection and the focused record are the workflow."]),
        new(
            "list-detail",
            "Selection-driven list and detail workspace",
            ["master detail", "list detail", "selection pane", "record detail", "inbox detail"],
            [
                new("ListDetailShell", "Workspace shell", "Keeps the list stable while the selected record changes in a focused detail pane."),
                new("ListPanelHeader", "List orientation", "Provides count, title, and actions for the selection region."),
                new("SelectionListItem", "Selectable row", "Expresses selected, disabled, and summary states consistently.")
            ],
            [new("Tabs", "Detail segmentation", "Place tabs inside a dense detail pane when its modes would otherwise become a long scroll.")],
            ["Keep selection state in the list shell; segment only the detail pane when it becomes mode-heavy."]),
        new(
            "tabs",
            "Peer views and in-page mode switching",
            ["tabs", "peer views", "alternate views", "mode switch", "long scroll"],
            [
                new("Tabs", "Panel navigation", "Use when one substantial pane should be visible at a time."),
                new("TabsItem", "Panel definition", "Provides each task-oriented label and coherent pane.")
            ],
            [
                new("SecondaryTabs", "Light switch", "Use for compact route or scenario changes inside an existing shell."),
                new("Steps", "Ordered flow", "Use when sequence and completion matter more than free switching.")
            ],
            ["Use `Tabs` to reduce mutually exclusive vertical content; use `SecondaryTabs` only when the outer shell already owns the surface."]),
        new(
            "steps",
            "Sequential wizard or ordered progress",
            ["wizard", "stepper", "sequential", "ordered progress", "multi step", "checkout steps"],
            [new("Steps", "Progress navigation", "Communicates order, current position, and what comes next."), new("StepsItem", "Step definition", "Defines each ordered stage and its state.")],
            [new("Tabs", "Peer navigation", "Use instead when users should freely switch between independent views.")],
            ["Choose steps only when sequence is part of the workflow contract."]),
        new(
            "tree-navigation",
            "Hierarchical navigation and selection",
            ["tree", "hierarchy", "nested navigation", "folder navigation", "outline"],
            [new("TreeView", "Hierarchy", "Provides accessible hierarchical expansion, selection, and keyboard behavior."), new("TreeViewNodeRow", "Node row", "Renders a consistent node interaction row.")],
            [new("SideMenu", "Primary app navigation", "Use for top-level destinations rather than arbitrary-depth hierarchy.")],
            ["Do not flatten hierarchy into side-menu items; keep application destinations and content trees as separate navigation levels."]),
        new(
            "feedback",
            "Feedback, state, and interruption level",
            ["feedback", "toast", "notification", "alert", "loading", "empty state", "tooltip", "confirmation dialog"],
            [
                new("Alert", "Persistent inline state", "Use for actionable information that must remain visible in context."),
                new("Notification", "Transient outcome", "Use for non-blocking save results and background outcomes from one mounted host."),
                new("Dialog", "Decision or focused interruption", "Use when the user must choose, confirm, or complete a bounded task."),
                new("TooltipTarget", "Short contextual help", "Use for compact hover and focus help around one trigger."),
                new("EmptyState", "Zero-data orientation", "Explains why content is absent and gives the next useful action."),
                new("LoadingState", "Progress orientation", "Prevents an intentional transition from appearing blank.")
            ],
            [new("HelpPopover", "Discoverable help", "Use when help needs more persistent reading than a tooltip.")],
            ["Choose by persistence and required action: tooltip < notification < alert < dialog."]),
        new(
            "metrics",
            "Cards, summaries, and metrics",
            ["metrics", "kpi", "stats", "summary tiles", "dashboard cards"],
            [
                new("StatsGrid", "Metric layout", "Provides a consistent metric-first responsive grid."),
                new("SummaryTiles", "Compact summaries", "Groups high-signal summary tiles."),
                new("MetricCard", "Single metric", "Presents one metric with shared hierarchy."),
                new("SectionCard", "Mixed content section", "Use when the region contains more than a metric.")
            ],
            [new("Grid", "Mixed dashboard", "Use when cards have different purposes and need explicit responsive tracks.")],
            ["Keep metric cards comparable; move mixed-purpose content to `Grid` plus semantic card components."]),
        new(
            "charts",
            "Operational charts",
            ["chart", "line chart", "bar chart", "pie chart", "donut", "time series"],
            [new("CdaChart", "Typed chart", "Keeps product code behind CanDoItAll chart models instead of raw Apex APIs."), new("ChartsHeadAssets", "Chart assets", "Adds the required Apex stylesheet once in the host.")],
            [new("ProgressBar", "Single progress value", "Use when one bounded percentage is clearer than a chart.")],
            ["Register charts, mount `ChartsHeadAssets`, and validate nonblank SVG output in a real browser."]),
        new(
            "mermaid",
            "Mermaid diagrams",
            ["mermaid", "flowchart", "architecture diagram", "sequence diagram", "diagram source"],
            [new("MermaidDiagram", "Diagram renderer", "Provides strict-by-default rendering, pan/zoom, node clicks, and normalized errors."), new("MermaidHeadAssets", "Scoped styles", "Loads the Mermaid component stylesheet once.")],
            [],
            ["Keep strict security defaults for untrusted source and validate parsing/rendering in a browser."]),
        new(
            "overlay-window",
            "Floating tool window or inspector",
            ["floating window", "tool window", "floating inspector", "draggable panel", "resizable panel", "component toolbox"],
            [new("OverlayWindow", "Floating window runtime", "Owns bounded placement, drag, resize, minimize, reset, hide, and show behavior."), new("OverlayComponentToolbox", "Searchable toolbox", "Provides grouped, searchable component choices inside a tool window."), new("OverlayLibHeadAssets", "Overlay styles", "Loads the shared floating-window stylesheet."), new("OverlayLibBodyAssets", "Overlay runtime", "Loads the plain JavaScript runtime once at the end of the body.")],
            [new("Dialog", "Modal task", "Use when the surface should block or focus the user rather than float persistently.")],
            ["Set the real `ContainerSelector` and `SafeTopSelector`; do not copy drag/resize behavior into a page component."]),
        new(
            "qr-code",
            "QR display and scanning",
            ["qr", "qr code", "scan code", "barcode scanner", "camera scan"],
            [new("QrCodeView", "Inline QR", "Renders a generic payload as accessible SVG."), new("QrCodeButton", "QR dialog action", "Opens a shared dialog for a typed generic QR payload."), new("QrScanButton", "Scanner action", "Opens camera scanning with a manual decoded-text fallback.")],
            [new("QrScanDialogContent", "Custom scanner content", "Use directly only when the standard scan button shell is insufficient.")],
            ["Register BaseLib and QRCode services, load QR scoped CSS, mount `DialogHost`, and keep domain payload meaning in the consuming app."]),
        new(
            "canvas",
            "Canvas, graph, calendar, and workbench surfaces",
            ["canvas", "graph editor", "node editor", "workbench", "canvas calendar"],
            [new("CanvasWorkbench", "Workbench runtime", "Use for graph-based authoring and dense desktop-first canvas workflows."), new("CanvasLibHeadAssets", "Canvas styles", "Loads OverlayLib and CanvasLib styles in verified order."), new("CanvasLibBodyAssets", "Canvas runtime", "Loads only the runtime, preview, and calendar scripts the host needs.")],
            [new("CanvasCalendar", "Calendar canvas", "Use for calendar-specific selection, CRUD, and export surfaces."), new("CanvasFloatingWindow", "Canvas window adapter", "Use when canvas state must adapt to OverlayLib's generic window runtime.")],
            ["Use typed Canvas contracts and generated asset components; this guide intentionally excludes every WebGL library and surface."]),
        new(
            "page-header",
            "Page orientation and action chrome",
            ["page header", "header actions", "toolbar", "filter bar", "page title actions"],
            [new("PageHeader", "Page orientation", "Combines eyebrow, title, description, stats, and action slots."), new("PageHeaderActionButton", "Header command", "Keeps compact header actions consistent and tooltip-ready."), new("Toolbar", "Working controls", "Groups dense filters and actions around a working surface.")],
            [new("FilterBar", "Filter controls", "Use when the header-adjacent region is primarily query and filter state.")],
            ["Put page orientation in the `PageScaffold.Header` slot; keep working controls close to the surface they affect."])
    ];

    public IReadOnlyList<ComponentLibraryDocument> GetLibraries()
    {
        var componentCounts = index.Value.Components
            .GroupBy(component => component.Library, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

        return LibraryTemplates
            .Select(template => new ComponentLibraryDocument(
                template.Name,
                template.PackageId,
                template.Purpose,
                template.ProductRuntimeLibrary,
                componentCounts.TryGetValue(template.Name, out var count) ? count : 0,
                template.Dependencies,
                template.UseFor,
                template.AvoidFor,
                template.ServiceRegistrations,
                template.HeadAssets,
                template.BodyAssets,
                template.Stylesheets,
                template.SandboxRoute,
                template.Notes))
            .ToArray();
    }

    public AppShellGuideData GetAppShellGuide(string? mode)
    {
        var normalizedMode = string.IsNullOrWhiteSpace(mode)
            ? "full-height"
            : mode.Trim().ToLowerInvariant();

        if (!AppShellModes.Contains(normalizedMode, StringComparer.OrdinalIgnoreCase))
        {
            throw new ToolInvocationException(
                "ValidationFailed",
                $"Unknown app shell mode '{mode}'. Use one of: {string.Join(", ", AppShellModes)}.");
        }

        var layoutExample = normalizedMode switch
        {
            "standard" => StandardAppShellExample,
            "focus-workbench" => FocusWorkbenchAppShellExample,
            _ => FullHeightAppShellExample
        };

        var pageExample = normalizedMode == "focus-workbench"
            ? FocusWorkbenchPageExample
            : StandardPageExample;

        return new AppShellGuideData(
            normalizedMode,
            AppShellModes,
            [
                "Reference `CanDoItAll.Components.BaseLib` and import `CanDoItAll.Components.BaseLib` plus `CanDoItAll.Components.Common`.",
                "Call `builder.Services.AddCanDoItAllBaseLib()` so SideMenu, dialog, tooltip, and notification services are scoped correctly.",
                "Load `_content/CanDoItAll.Components.BaseLib/css/material-icons.css` and `_content/CanDoItAll.Components.BaseLib/css/output.css` in `App.razor`.",
                "Use an interactive render mode for the layout and mount `<DialogHost />`, `<Tooltip />`, and `<Notification />` exactly once."
            ],
            [
                "`ThemeHost`: semantic light/dark token boundary around the full interactive app.",
                "`Layout`: top-level flex shell; use `FitViewport` only when the app, not the document, owns viewport height.",
                "`SideMenu`: primary responsive navigation; it is already an `<aside>`, so do not wrap it in `Sidebar`.",
                "`Body`: main content region and explicit scroll owner.",
                "`PageScaffold`: routed-page header, lead, width, secondary rail, and full-height boundary.",
                "`Grid`, `Row`, `Column`, `Stack`, `Cluster`, and `Split`: inner page composition after the shell is established."
            ],
            layoutExample,
            pageExample,
            [
                "Give every menu a stable, application-wide `MenuId`; expansion persistence is keyed from it unless `PersistenceKey` overrides it.",
                "Use `Items` for runtime data and `<MenuItems>` for static declarative entries. They compose, so never repeat an item ID across the two sources.",
                "Use `MoreItems` or `OverflowBehavior.AlwaysInMore` for secondary destinations, and `BottomItems` for account, settings, and help utilities.",
                "`SideMenu` is a typed selection/action model, not an anchor list. Map stable item IDs or typed `Payload` values to routes in `ItemSelected`, call `NavigationManager.NavigateTo`, and synchronize selection when the route changes.",
                "Use `SideMenuService.SetItems` only for temporary context or tab overrides; call `ResetItems` to restore declared items.",
                "Use `SideMenuService.SelectAsync`, `SetExpanded`, and `ToggleExpanded` when another feature drives navigation. Dispose every `Subscribe` registration.",
                "Let measured overflow choose the More menu. Set `VisibleItemCapacity` only for a deliberate deterministic constraint or focused proof.",
                "Use child items or `PanelContent` for a bounded utility panel. Do not turn primary navigation into deeply nested menus."
            ],
            [
                "Choose one vertical scroll owner. In a full-height shell use `Layout FitViewport=\"true\" LockOverflow=\"true\"` and `Body FillHeight=\"true\" Scrollable=\"true\"`.",
                "Use `ViewportOffset` only when chrome outside `Layout` consumes known viewport height.",
                "Use `PageScaffold FillHeight=\"true\"` for workspaces whose inner surface owns overflow; leave it false for document pages that scroll naturally.",
                "Use `PageScaffoldMode.FocusWorkbench` only for dense workbench pages that intentionally use the full width.",
                "Use `Sidebar` for a generic custom rail, not around `SideMenu`. Nesting both creates duplicate width and scroll ownership.",
                "Inside pages, prefer `Stack` for one dimension, `Grid` for explicit tracks, `Row` plus `Column` for inherited responsive tracks, and `Split` for one two-sided breakpoint."
            ],
            [
                "`SideMenu` automatically changes from large/medium rail to a small-screen top menu below 768px; do not build a second mobile navigation component.",
                "The small-screen menu opens downward and the full-height body remains the content scroll owner.",
                "Use `ColumnsSm`, `ColumnsMd`, `ColumnsLg`, or `ColumnTemplate*` rather than page-local media-query wrappers.",
                "Test collapsed and expanded rails, height-driven overflow, the More panel, bottom utility panels, keyboard activation, and a real browser resize across the 768px breakpoint."
            ],
            [
                "Call `component_get` for `Layout`, `SideMenu`, `Body`, and `PageScaffold` before implementation.",
                "Call `component_usage_examples` for `SideMenu` and open `/groups/navigation/side-menu` for the canonical responsive proof.",
                "Verify exactly one BaseLib service registration and one overlay host of each type.",
                "Verify route changes update menu selection and menu selections navigate without duplicated callbacks.",
                "Use browser assertions at desktop, constrained-height desktop, and mobile widths; build success cannot prove overflow, persistence, focus, or dropdown placement."
            ]);
    }

    public ComponentRecommendationData RecommendComponents(
        string useCase,
        string? constraints = null,
        string? library = null,
        int limit = 6)
    {
        if (string.IsNullOrWhiteSpace(useCase))
        {
            throw new ToolInvocationException("ValidationFailed", "A concrete use case is required.");
        }

        var normalizedUseCase = useCase.Trim();
        var normalizedConstraints = constraints?.Trim() ?? string.Empty;
        var queryText = $"{normalizedUseCase} {normalizedConstraints}".Trim();
        var normalizedLibrary = library?.Trim();
        var boundedLimit = Math.Clamp(limit, 1, 12);

        var scoredGuides = DecisionGuides
            .Select(guide => (Guide: guide, Score: ScoreDecisionGuide(guide, queryText)))
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Guide.Title, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var topScore = scoredGuides.FirstOrDefault().Score;
        var matchedGuides = scoredGuides
            .Where(item => item.Score >= Math.Max(2, topScore * 0.5))
            .Take(3)
            .ToArray();

        var recommendations = new List<ComponentRecommendationDocument>();
        var alternatives = new List<ComponentRecommendationDocument>();
        var seenRecommendations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenAlternatives = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var matchedGuide in matchedGuides)
        {
            AddDecisionComponents(
                matchedGuide.Guide.Recommendations,
                matchedGuide.Score,
                normalizedLibrary,
                recommendations,
                seenRecommendations);

            AddDecisionComponents(
                matchedGuide.Guide.Alternatives,
                matchedGuide.Score * 0.65,
                normalizedLibrary,
                alternatives,
                seenAlternatives,
                seenRecommendations);
        }

        if (recommendations.Count < boundedLimit)
        {
            var search = Search(normalizedUseCase, normalizedLibrary, group: null, limit: boundedLimit * 2);
            foreach (var hit in search.Components)
            {
                if (recommendations.Count >= boundedLimit || !seenRecommendations.Add(hit.Name))
                {
                    continue;
                }

                var component = TryGetComponent(hit.Name);
                if (component is null)
                {
                    continue;
                }

                recommendations.Add(ToRecommendation(
                    component,
                    "Catalog match",
                    $"High-signal search match for the requested use case. {component.Summary}",
                    hit.Score));
            }
        }

        var selectedRecommendations = recommendations
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .Take(boundedLimit)
            .ToArray();

        var selectedAlternatives = alternatives
            .Where(item => selectedRecommendations.All(selected => !string.Equals(selected.Name, item.Name, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Min(4, boundedLimit))
            .ToArray();

        var libraries = GetLibraries().ToDictionary(item => item.Name, StringComparer.OrdinalIgnoreCase);
        var requiredSetup = selectedRecommendations
            .Select(item => item.Library)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(libraries.ContainsKey)
            .SelectMany(libraryName =>
            {
                var componentLibrary = libraries[libraryName];
                return (componentLibrary.ServiceRegistrations ?? [])
                    .Concat(componentLibrary.HeadAssets ?? [])
                    .Concat(componentLibrary.BodyAssets ?? [])
                    .Concat((componentLibrary.Stylesheets ?? []).Select(stylesheet => $"Load `{stylesheet}`."));
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var compositionPlan = matchedGuides
            .SelectMany(item => item.Guide.CompositionPlan)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var followUpTools = selectedRecommendations
            .Take(4)
            .SelectMany(component => new[]
            {
                $"component_get component=\"{component.Name}\"",
                $"component_usage_examples component=\"{component.Name}\"",
                $"component_examples component=\"{component.Name}\""
            })
            .Prepend(matchedGuides.Any(item => string.Equals(item.Guide.Key, "app-shell", StringComparison.OrdinalIgnoreCase))
                ? "app_shell_guide_get mode=\"full-height\""
                : "components_libraries_list")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new ComponentRecommendationData(
            normalizedUseCase,
            normalizedConstraints,
            matchedGuides.Select(item => item.Guide.Title).ToArray(),
            selectedRecommendations,
            selectedAlternatives,
            requiredSetup,
            compositionPlan,
            followUpTools);
    }

    private void AddDecisionComponents(
        IReadOnlyList<DecisionComponent> decisionComponents,
        double guideScore,
        string? library,
        List<ComponentRecommendationDocument> target,
        HashSet<string> seen,
        HashSet<string>? excluded = null)
    {
        for (var index = 0; index < decisionComponents.Count; index++)
        {
            var decisionComponent = decisionComponents[index];
            if (excluded?.Contains(decisionComponent.Name) == true || !seen.Add(decisionComponent.Name))
            {
                continue;
            }

            var component = TryGetComponent(decisionComponent.Name);
            if (component is null ||
                (!string.IsNullOrWhiteSpace(library) && !string.Equals(component.Library, library, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            target.Add(ToRecommendation(
                component,
                decisionComponent.Role,
                decisionComponent.Why,
                (guideScore * 100) - index));
        }
    }

    private ComponentDocument? TryGetComponent(string name)
        => index.Value.Components.FirstOrDefault(component => string.Equals(component.Name, name, StringComparison.OrdinalIgnoreCase));

    private static ComponentRecommendationDocument ToRecommendation(
        ComponentDocument component,
        string role,
        string why,
        double score)
        => new(
            component.Name,
            component.Library,
            role,
            why,
            component.Guidance.UseFor,
            component.Guidance.AvoidFor,
            component.DependencyNames,
            score);

    private static double ScoreDecisionGuide(ComponentDecisionGuide guide, string query)
    {
        var score = 0d;
        foreach (var keyword in guide.Keywords)
        {
            if (query.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                score += keyword.Contains(' ') ? 4 : 2;
                continue;
            }

            var tokens = keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (tokens.Length > 1 && tokens.All(token => token.Length < 3 || query.Contains(token, StringComparison.OrdinalIgnoreCase)))
            {
                score += 1;
            }
        }

        return score;
    }

    private const string FullHeightAppShellExample = """
        @inherits LayoutComponentBase
        @inject NavigationManager Navigation

        <ThemeHost ThemeKey="@CadThemes.Light">
            <Layout FitViewport="true" LockOverflow="true">
                <SideMenu MenuId="primary-navigation"
                          Title="Application"
                          Items="@NavigationItems"
                          FillHeight="true"
                          @bind-Expanded="menuExpanded"
                          ItemSelected="NavigateAsync" />

                <Body FillHeight="true" Scrollable="true">
                    @Body
                </Body>
            </Layout>

            <DialogHost />
            <Tooltip />
            <Notification Position="NotificationPosition.TopRight" />
        </ThemeHost>
        """;

    private const string StandardAppShellExample = """
        @inherits LayoutComponentBase
        @inject NavigationManager Navigation

        <ThemeHost ThemeKey="@CadThemes.Light">
            <Layout>
                <SideMenu MenuId="primary-navigation"
                          Title="Application"
                          Items="@NavigationItems"
                          FillHeight="false"
                          @bind-Expanded="menuExpanded"
                          ItemSelected="NavigateAsync" />

                <Body>
                    @Body
                </Body>
            </Layout>

            <DialogHost />
            <Tooltip />
            <Notification Position="NotificationPosition.TopRight" />
        </ThemeHost>
        """;

    private const string FocusWorkbenchAppShellExample = """
        @inherits LayoutComponentBase
        @inject NavigationManager Navigation

        <ThemeHost ThemeKey="@CadThemes.Dark">
            <Layout FitViewport="true" LockOverflow="true">
                <SideMenu MenuId="workbench-navigation"
                          Title="Workbench"
                          Items="@NavigationItems"
                          FillHeight="true"
                          @bind-Expanded="menuExpanded"
                          ItemSelected="NavigateAsync" />

                <Body FillHeight="true" Scrollable="false">
                    @Body
                </Body>
            </Layout>

            <DialogHost />
            <Tooltip />
            <Notification Position="NotificationPosition.BottomCenter" />
        </ThemeHost>
        """;

    private const string StandardPageExample = """
        <PageScaffold>
            <Header>
                <PageHeader Eyebrow="Workspace"
                            Title="Page title"
                            Description="State the purpose and current context.">
                    <Actions>
                        <PageHeaderActionButton Icon="add" Label="Create" Click="CreateAsync" />
                    </Actions>
                </PageHeader>
            </Header>

            <Grid Columns="1" ColumnsLg="2" Gap="LayoutGap.Large">
                <SectionCard Title="Primary work">...</SectionCard>
                <SectionCard Title="Supporting context">...</SectionCard>
            </Grid>
        </PageScaffold>
        """;

    private const string FocusWorkbenchPageExample = """
        <PageScaffold Mode="PageScaffoldMode.FocusWorkbench"
                      FillHeight="true"
                      MaxWidthClass="max-w-full">
            <Header>
                <PageHeader Eyebrow="Workbench" Title="Focused surface" Compact="true" />
            </Header>

            <Grid Columns="1" FillHeight="true" Gap="LayoutGap.Small">
                @* The workbench or canvas surface owns its internal overflow. *@
            </Grid>
        </PageScaffold>
        """;

    private sealed record ComponentLibraryTemplate(
        string Name,
        string PackageId,
        string Purpose,
        bool ProductRuntimeLibrary,
        IReadOnlyList<string> Dependencies,
        IReadOnlyList<string> UseFor,
        IReadOnlyList<string> AvoidFor,
        IReadOnlyList<string> ServiceRegistrations,
        IReadOnlyList<string> HeadAssets,
        IReadOnlyList<string> BodyAssets,
        IReadOnlyList<string> Stylesheets,
        string? SandboxRoute,
        IReadOnlyList<string> Notes);

    private sealed record ComponentDecisionGuide(
        string Key,
        string Title,
        IReadOnlyList<string> Keywords,
        IReadOnlyList<DecisionComponent> Recommendations,
        IReadOnlyList<DecisionComponent> Alternatives,
        IReadOnlyList<string> CompositionPlan);

    private sealed record DecisionComponent(
        string Name,
        string Role,
        string Why);
}
