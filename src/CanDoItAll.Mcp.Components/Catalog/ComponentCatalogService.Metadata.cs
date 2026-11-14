using System.Text.RegularExpressions;

namespace CanDoItAll.Mcp.Components.Catalog;

public sealed partial class ComponentCatalogService
{
    private static readonly Regex ComponentReferenceRegex = new(@"<(?<name>[A-Z][A-Za-z0-9_]*)\b", RegexOptions.Compiled);
    private static readonly Regex RouteDirectiveRegex = new(@"^\s*@page\s+""(?<route>[^""]+)""", RegexOptions.Compiled);
    private const string CanvasLibScopedStylesheet = "_content/CanDoItAll.Components.CanvasLib/CanDoItAll.Components.CanvasLib.bundle.scp.css";

    private static readonly IReadOnlyList<string> CanvasLibStylesheets =
    [
        "_content/CanDoItAll.Components.CanvasLib/css/workbench/shell/01-layout-and-shell.css",
        "_content/CanDoItAll.Components.CanvasLib/css/workbench/chrome/02-toolbar-and-windows.css",
        "_content/CanDoItAll.Components.CanvasLib/css/workbench/panels/03-help-settings-and-preview.css",
        "_content/CanDoItAll.Components.CanvasLib/css/workbench/scene/04-scene-and-nodes.css",
        "_content/CanDoItAll.Components.CanvasLib/css/workbench/overlays/05-overlays-and-composer.css",
        "_content/CanDoItAll.Components.CanvasLib/css/workbench/responsive/06-motion-and-responsive.css"
    ];
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> CssNotesByComponent = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
    {
        ["ThemeHost"] =
        [
            "Emits `data-ui-theme` for the light, dark, or custom theme key; the shared token definitions live in `Tailwind/theme.css`.",
            "Use `--tone-page-bg`, `--tone-surface`, `--tone-surface-soft`, `--tone-border`, and `--tone-text-*` for structural colors, and pair each semantic tone's background, foreground, and border.",
            "The `--color-chrome-*` scale reverses in dark mode; `--color-neutral-*` remains fixed for deliberately constant surfaces and shadows.",
            "Load app-owned overrides after BaseLib output CSS and scope them to the intended `data-ui-theme` boundary. Keep the shared `--cad-space-*`, `--cad-radius-*`, and `--cad-shadow-*` tokens for spacing, shape, and shadows."
        ],
        ["Button"] =
        [
            "Uses the shared BaseLib button variants, sizes, and tones from `_content/CanDoItAll.Components.BaseLib/css/output.css`.",
            "Prefer `Tone` (`ButtonTone`), `Variant`, `Size`, `Density`, and `Rounded` before adding page-local button styling."
        ],
        ["CopyButton"] =
        [
            "Uses the shared BaseLib button and inline icon-copy styles from `_content/CanDoItAll.Components.BaseLib/css/output.css`.",
            "Prefer `DisplayMode`, `Size`, `Tone`, and `Variant` before adding page-local copy affordances or custom clipboard handlers."
        ],
        ["PageScaffold"] =
        [
            "Owns compact page-level spacing and max-width conventions through the shared BaseLib output CSS.",
            "Use scaffold slots before introducing page-local wrapper structure."
        ],
        ["PageHeader"] =
        [
            "Uses the shared compact header rhythm from `_content/CanDoItAll.Components.BaseLib/css/output.css`.",
            "Prefer `Compact`, `Stats`, and `Actions` before adding a second hero, stat-card row, or page-local header wrapper."
        ],
        ["TextAreaInput"] =
        [
            "Uses content-aware semantic heights from the BaseLib form styles instead of one large minimum height.",
            "Prefer `Size` for expected content length and use `Rows` only as a deliberate override."
        ],
        ["CheckBox"] =
        [
            "Uses shared form-control colors and the `--ui-checkbox-checkmark` and `--ui-checkbox-indeterminate` image tokens defined for light and dark theme boundaries.",
            "Keep checked and indeterminate glyph contrast aligned with the control's semantic fill, including when `data-ui-theme` is set on the document root."
        ],
        ["SecretField"] =
        [
            "Its input, reveal control, and copy actions form one compound control whose wrapping must respond to the containing width, not only the viewport.",
            "Validate the wrapped state inside a realistic narrow grid column, including a roughly 300–320px column on a large desktop viewport."
        ],
        ["Card"] =
        [
            "Uses the shared card anatomy and spacing contract from `_content/CanDoItAll.Components.BaseLib/css/output.css`.",
            "Use `Media`, `Header`, `Tags`, body content, and `Actions` slots to keep peer item cards aligned."
        ],
        ["CardGrid"] =
        [
            "Uses shared responsive card tracks and compact gaps from the BaseLib output stylesheet.",
            "Use `Grid` instead when a collection needs custom tracks or is primarily tabular."
        ],
        ["CompactStat"] =
        [
            "Uses the compact label/value contract for supporting metrics without card-sized chrome.",
            "Keep values short; use a metric card only when the metric is the page's primary content."
        ],
        ["CompactStatStrip"] =
        [
            "Keeps supporting metrics in a compact wrapping row beside orientation or controls.",
            "Do not repeat the same metrics in both this strip and larger summary cards."
        ],
        ["Tabs"] =
        [
            "Uses the shared Tailwind-owned `cad-tabs` styles from `_content/CanDoItAll.Components.BaseLib/css/output.css`; it should not depend on legacy `zy-*` selectors.",
            "Use `Density`, `Rounded`, `ContentRounded`, `Tone`, `BorderMode`, and `OverflowMode`; `TabsVariant` is no longer part of the refactored contract.",
            "Use `NoContent` for a navigation-only strip, `NoSlider` only when the active indicator is deliberately unnecessary, and `PanelOverflowMode` to name the content scroll owner."
        ],
        ["SecondaryTabs"] =
        [
            "Secondary tabs stay lighter than full `Tabs` and should read as a compact route or scenario switch instead of a content container.",
            "Use shared variants and text states first; do not restyle them into a second full tabs system."
        ],
        ["StatusBadge"] =
        [
            "Maps semantic tones to the shared status surface palette in the generated BaseLib stylesheet.",
            "Status chips should communicate state, not replace headings or summaries."
        ],
        ["Notification"] =
        [
            "Renders fixed viewport toast stacks from one mounted host using Tailwind utility classes in `_content/CanDoItAll.Components.BaseLib/css/output.css`.",
            "`NotificationMessage.Position` and the `Notify(..., position: ...)` overload override the host `Position` per message; use that instead of page-local fixed wrappers.",
            "The close affordance is intentionally the compact X control to preserve notification width for summary and detail copy."
        ],
        ["Tooltip"] =
        [
            "Renders the active `TooltipService` state through one mounted host using Tailwind utility classes in `_content/CanDoItAll.Components.BaseLib/css/output.css`.",
            "`TooltipOptions.Position` controls service-opened tooltip placement; choose a `TooltipPosition` side, corner, or edge alignment instead of custom absolute-positioning CSS."
        ],
        ["TooltipTarget"] =
        [
            "Wraps trigger content and forwards hover/focus coordinates to `TooltipService` with shared Tailwind tooltip rendering.",
            "`TooltipTarget Position` uses the same `TooltipPosition` enum as service-opened tooltips, so declarative and imperative tooltip placement rules stay aligned."
        ],
        ["SideMenu"] =
        [
            "Uses the shared `cad-side-menu` styles from `_content/CanDoItAll.Components.BaseLib/css/output.css`, including large and medium rails plus the small-screen top dropdown.",
            "Measured item-region height drives overflow into More; do not recreate capacity or breakpoint behavior with page-local CSS.",
            "The menu is already an `<aside>` and should sit directly beside `<Body>` inside `<Layout>`, not inside another `<Sidebar>`."
        ],
        ["Layout"] =
        [
            "Uses the BaseLib application-frame styles from `Tailwind/layout/layout.css`.",
            "Use `FitViewport`, `ViewportOffset`, and `LockOverflow` to establish one deliberate shell-level scroll model."
        ],
        ["DataGrid"] =
        [
            "Uses the shared BaseLib table contract and hardened keyboard, sorting, and selection behavior.",
            "Prefer typed `DataGridColumn` definitions and grid parameters before wrapping a raw table or adding page-local interaction scripts."
        ],
        ["Dialog"] =
        [
            "Uses BaseLib dialog structure plus the component's colocated JavaScript module for focus and viewport behavior.",
            "Mount `<DialogHost />` once for service-driven dialogs; keep direct controlled `<Dialog>` usage for component-owned open state.",
            "Choose `Size` from the task layout and use `DenseChrome` for short focused forms instead of stretching every dialog to a large default."
        ],
        ["ContextMenu"] =
        [
            "Uses BaseLib menu structure plus the component's colocated JavaScript module for viewport-aware placement and keyboard behavior.",
            "Use typed menu items and the shared close/selection pipeline instead of custom absolute menus."
        ],
        ["TreeView"] =
        [
            "Uses shared `cda-treeview` styles and hardened tree keyboard behavior.",
            "Keep hierarchical navigation in `TreeView`; do not flatten arbitrary depth into `SideMenu`."
        ],
        ["OverlayWindow"] =
        [
            "Uses `_content/CanDoItAll.Components.OverlayLib/css/overlay-window.css` plus the runtime loaded by `<OverlayLibBodyAssets />`.",
            "Neutral colors use `--ui-overlay-{surface,surface-soft,border,border-soft,text,text-strong,text-muted,shadow-color}`; status colors use `--ui-overlay-{info,secondary,success,warning,danger}-{bg,fg,border}`.",
            "BaseLib resolves the overlay aliases from current semantic tones at every `data-ui-theme` boundary. Overlay CSS retains standalone literal defaults when BaseLib styles are absent; override paired status colors together.",
            "Container and safe-top selectors define the real placement boundary; avoid copying drag, resize, minimize, or normalization logic into consumers."
        ],
        ["QrCodeView"] =
        [
            "Uses the QRCode scoped CSS bundle and renders SVG through the registered `IQrCodeRenderer`.",
            "Keep the payload generic and accessible; domain meaning belongs in the consuming application."
        ],
        ["QrScanButton"] =
        [
            "Uses the QRCode scoped CSS bundle, BaseLib `DialogService`, and a mounted `<DialogHost />`.",
            "Camera scanning must retain the manual decoded-text fallback for unavailable or denied camera access."
        ],
        ["CanvasOverlayDialog"] =
        [
            "Uses the colocated `CanvasOverlayDialog.razor.css` for dialog paint, included by `<CanvasLibHeadAssets />` through the generated CanvasLib scoped CSS bundle.",
            "Dialog surface, foreground, border, and shadow use existing `--ui-overlay-{surface,text,border-soft,shadow-color}` aliases. BaseLib resolves these at each `data-ui-theme` boundary; the scoped CSS retains standalone literal defaults when BaseLib styles are absent. Child typography and controls retain their own semantic tokens.",
            "Override foreground and surface colors together through these aliases; keep dialog paint in shared CSS instead of reintroducing a fixed light inline background.",
            "Use `/modals#canvas-overlay-dialog` to verify light and dark themes, disabled native and linked actions, form fields, maximize, and close behavior."
        ],
        ["CanvasWorkbench"] =
        [
            "Uses the shared CanvasLib workbench stylesheets exposed by `<CanvasLibHeadAssets />` under `_content/CanDoItAll.Components.CanvasLib/css/workbench/...` plus the typed `CanvasThemeTokenPack` theme vocabulary.",
            "Toolbar, floating windows, preview cards, and diagnostics share the same `cw-*` token space."
        ],
        ["CanvasCalendar"] =
        [
            "Uses the shared canvas stylesheet and the same theme token pack as the workbench surfaces.",
            "Calendar boundary previews should stay aligned with the runtime token vocabulary rather than inventing parallel styles."
        ],
        ["CanvasBoundaryCard"] =
        [
            "Boundary cards share the same canvas preview card treatment and `cw-*` token space as the runtime previews.",
            "Use boundary cards for proof and documentation surfaces, not for runtime authoring content."
        ]
    };

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> DefaultCssNotesByLibrary = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
    {
        ["BaseLib"] =
        [
            "BaseLib components render against `_content/CanDoItAll.Components.BaseLib/css/output.css`.",
            "Structural colors use `--tone-*`; semantic status colors pair `--tone-{name}-{soft,solid}-{bg,fg,border}` under the current `data-ui-theme` boundary.",
            "The chrome palette reverses in dark mode while neutral colors remain fixed; spacing, shape, and shadows retain the shared `--cad-space-*`, `--cad-radius-*`, and `--cad-shadow-*` tokens."
        ],
        ["CanvasLib"] =
        [
            "`<CanvasLibHeadAssets />` loads OverlayLib styles first, then the generated CanvasLib scoped CSS bundle and the six global workbench stylesheets. The scoped bundle is required for components such as `CanvasOverlayDialog` even when all global workbench styles are present.",
            "Canvas surfaces also use the typed `CanvasThemeTokenPack` so runtime and preview assets stay aligned."
        ],
        ["Charts"] =
        [
            "Charts components need `<ChartsHeadAssets />` for Apex CSS and `<ChartsBodyAssets />` before Blazor starts for semantic token resolution and runtime theme updates.",
            "Keep `IncludeThemeTokens=true` unless an earlier asset component already loads BaseLib's shared theme script; omitting the chart body assets leaves chart text and grid colors disconnected from the application theme.",
            "Consumers should use CanDoItAll chart models and `CdaChart` instead of raw Apex component markup."
        ],
        ["Mermaid"] =
        [
            "Mermaid components render through the vendored official Mermaid ESM bundle under `_content/CanDoItAll.Components.Mermaid/js/vendor/`.",
            "Consumers should use `MermaidDiagram` and its typed options/events instead of directly importing Mermaid.js in pages."
        ],
        ["OverlayLib"] =
        [
            "OverlayLib components render against the stylesheet exposed by `<OverlayLibHeadAssets />` and the plain runtime exposed by `<OverlayLibBodyAssets />`.",
            "Window and toolbox surfaces use `--ui-overlay-*` neutral aliases plus paired `--ui-overlay-{info,secondary,success,warning,danger}-{bg,fg,border}` status tokens. BaseLib resolves these aliases at each `data-ui-theme` boundary; standalone CSS retains literal defaults when BaseLib styles are absent.",
            "Use `OverlayWindowState` as the generic state boundary for placement, size, visibility, and minimized state."
        ],
        ["QRCode"] =
        [
            "QRCode components render against `_content/CanDoItAll.Components.QRCode/CanDoItAll.Components.QRCode.bundle.scp.css`.",
            "Register `AddCanDoItAllQrCode()` and keep QR payload semantics in the consuming application."
        ]
    };
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> BaseLibCssSourceFilesByComponent = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
    {
        ["Tabs"] = [@"Tailwind\navigation\tabs.css"],
        ["TabsItem"] = [@"Tailwind\navigation\tabs.css"],
        ["SecondaryTabs"] = [@"Tailwind\navigation\tabs.css"],
        ["RibbonTabs"] = [@"Tailwind\navigation\tabs.css"],
        ["PageHeader"] = [@"Tailwind\navigation\page-header.css"],
        ["FilterBar"] = [@"Tailwind\navigation\page-header.css"],
        ["TreeView"] = [@"Tailwind\navigation\tree-view.css"],
        ["TreeViewNodeRow"] = [@"Tailwind\navigation\tree-view.css"],
        ["SideMenu"] = [@"Tailwind\navigation\side-menu.css"],
        ["SideMenuItem"] = [@"Tailwind\navigation\side-menu.css"],
        ["Layout"] = [@"Tailwind\layout\layout.css"],
        ["Sidebar"] = [@"Tailwind\layout\sidebar.css"],
        ["Body"] = [@"Tailwind\layout\body.css"],
        ["ThemeHost"] = [@"Tailwind\theme.css", @"Tailwind\layout\theme-host.css"],
        ["TextInput"] = [@"Tailwind\forms\styled-input-base.css", @"Tailwind\forms\form-controls.css"],
        ["TextAreaInput"] = [@"Tailwind\forms\styled-input-base.css", @"Tailwind\forms\form-controls.css"],
        ["NumberInput"] = [@"Tailwind\forms\styled-input-base.css", @"Tailwind\forms\form-controls.css"],
        ["SelectInput"] = [@"Tailwind\forms\styled-input-base.css", @"Tailwind\forms\form-controls.css"],
        ["TagEditor"] = [@"Tailwind\forms\tag-editor.css"],
        ["Button"] = [@"Tailwind\buttons\button.css"],
        ["CopyButton"] = [@"Tailwind\buttons\button.css", @"Tailwind\buttons\copy-button.css"],
        ["Badge"] = [@"Tailwind\badges\badge.css"],
        ["BadgesGroup"] = [@"Tailwind\badges\badge.css"],
        ["Chip"] = [@"Tailwind\badges\badge.css"],
        ["ChipRow"] = [@"Tailwind\badges\badge.css"],
        ["Pill"] = [@"Tailwind\badges\pill.css"],
        ["PillList"] = [@"Tailwind\badges\pill.css"],
        ["StatusBadge"] = [@"Tailwind\badges\status-badge.css"],
        ["Alert"] = [@"Tailwind\feedback\alert.css"],
        ["HelpPopover"] = [@"Tailwind\feedback\help-popover.css"],
        ["HelpCard"] = [@"Tailwind\feedback\help-card.css"],
        ["Tooltip"] = [],
        ["TooltipTarget"] = [],
        ["Dialog"] = [@"Tailwind\modals\dialog.css"],
        ["DialogScaffold"] = [@"Tailwind\modals\dialog-scaffold.css"]
    };
    private static readonly IReadOnlyDictionary<string, ComponentGuidanceDocument> GuidanceByFamily = new Dictionary<string, ComponentGuidanceDocument>(StringComparer.OrdinalIgnoreCase)
    {
        ["Badges"] = new(
            [
                "Compact status, counts, labels, and short categorical emphasis."
            ],
            [
                "Replacing headings, summaries, or full explanation copy."
            ],
            [
                "Keep badge copy short and scannable.",
                "Use semantic tone first and avoid turning badges into miniature cards."
            ]),
        ["Buttons"] = new(
            [
                "Primary, secondary, and inline actions with shared emphasis rules."
            ],
            [
                "Using button styling as generic layout chrome."
            ],
            [
                "Use shared variants and sizes before page-local classes.",
                "If many actions compete, reduce the set or move secondary actions into lighter navigation."
            ]),
        ["Cards"] = new(
            [
                "Grouped surfaces for summaries, actions, metrics, and section-level content."
            ],
            [
                "Building very long pages by stacking many cards that users consume one mode at a time."
            ],
            [
                "Use cards when the surface itself is the interaction or grouping unit.",
                "If adjacent cards represent alternate views of the same object, promote the pattern into tabs or a split shell.",
                "For visual collections, keep media, identity, tags, content, and actions in consistent regions; use rows or a grid when imagery does not improve recognition."
            ]),
        ["DataVisualization"] = new(
            [
                "Charts, data grids, axes, and quantitative progress display."
            ],
            [
                "Decorative charts that do not help someone operate or decide."
            ],
            [
                "Keep the chart or grid tied to a decision or workflow.",
                "Prefer shared display components before inventing local chart shells."
            ]),
        ["Feedback"] = new(
            [
                "Alerts, empty states, loading surfaces, notifications, and contextual help."
            ],
            [
                "Primary page structure or decorative marketing copy."
            ],
            [
                "Use feedback components to orient, explain state, and suggest the next step.",
                "Keep tone calm and utility-first; if the state needs a real workflow branch, switch layout instead of adding louder alert chrome."
            ]),
        ["Forms"] = new(
            [
                "Data entry, configuration, validation, and small editing workflows."
            ],
            [
                "Hand-built field wrappers when shared field, row, and section components already exist."
            ],
            [
                "Prefer `FormField`, `FormRow`, and shared input parameters before raw input markup.",
                "Use inline help and validation to reduce scanning friction instead of adding extra layout wrappers.",
                "Choose control width and height from expected content; do not give a short textarea the same height as a substantial prose editor.",
                "Make compound controls wrap from their actual containing width, then prove them inside realistic narrow grid columns as well as at viewport breakpoints."
            ]),
        ["Identity"] = new(
            [
                "Presence, avatars, icons, and attribution details that improve scanning."
            ],
            [
                "Decorative icon usage with no orientation value."
            ],
            [
                "Use identity components to help recognition and hierarchy.",
                "If an icon does not improve scanning, remove it."
            ]),
        ["Layout"] = new(
            [
                "Page structure, responsive regions, spacing rhythm, and workspace composition."
            ],
            [
                "Using layout primitives to simulate semantic components that already exist."
            ],
            [
                "Start with layout intent, not wrappers.",
                "Use small or medium shared gaps for ordinary operational composition, then let the primary surface fill remaining space logically.",
                "If content splits into modes, use tabs, list-detail, or rails before accepting a long single-column page."
            ]),
        ["Lists"] = new(
            [
                "Rows, grouped metadata, list/detail workspaces, and selection-driven review surfaces."
            ],
            [
                "Using list shells when the user is not actually selecting between records or views."
            ],
            [
                "Keep list density readable and let the detail area stay focused.",
                "Put bounded create and edit forms in dialogs when users do not need the list visible for comparison.",
                "When the detail area becomes tall and mode-heavy, segment it with tabs instead of stacking more sections."
            ]),
        ["Modals"] = new(
            [
                "Dialogs and transient overlays that keep the user in context."
            ],
            [
                "Permanent page structure or long-form navigation."
            ],
            [
                "Use overlays for focused interruption, not as a substitute for page composition.",
                "Choose dialog width from the task layout and use dense chrome for short forms; do not default every modal to a wide surface.",
                "Keep modal flows short and explicit."
            ]),
        ["Navigation"] = new(
            [
                "Tabs, headers, trees, steps, toolbars, and route-level orientation."
            ],
            [
                "Passive content layout that should live in cards, lists, or scaffold regions instead."
            ],
            [
                "Choose the lightest navigation that still makes state obvious.",
                "Use a compact page header for operational pages and keep working controls adjacent to their surface.",
                "Use tabs aggressively when it reduces vertical sprawl and keeps one focused working surface visible."
            ]),
        ["Storage"] = new(
            [
                "Storage-oriented summaries, badges, and capacity surfaces."
            ],
            [
                "General-purpose layout or status surfaces outside storage workflows."
            ],
            [
                "Keep storage displays quantitative and utility-first.",
                "Reuse shared summary surfaces before introducing storage-specific local wrappers."
            ]),
        ["Typography"] = new(
            [
                "Heading rhythm, copy hierarchy, captions, dividers, and text emphasis."
            ],
            [
                "Replacing semantic structure with arbitrary font-size utility classes."
            ],
            [
                "Use typography components to keep scanning and hierarchy consistent.",
                "Prefer shared text styles before page-local text tuning."
            ]),
        ["Calendar"] = new(
            [
                "Calendar-specific runtime surfaces, editors, and selection workflows."
            ],
            [
                "Generic app layout that should stay in BaseLib."
            ],
            [
                "Keep calendar interactions tied to the shared canvas model and workbench surfaces."
            ]),
        ["Core"] = new(
            [
                "Canvas runtime infrastructure such as floating windows and accessibility layers."
            ],
            [
                "General product UI outside the workbench runtime."
            ],
            [
                "Treat core canvas components as runtime infrastructure, not page-level decoration."
            ]),
        ["Diagnostics"] = new(
            [
                "Canvas diagnostics and runtime debugging overlays."
            ],
            [
                "Routine operator surfaces that should stay calm and minimal."
            ],
            [
                "Keep diagnostics discoverable but secondary to the main work surface."
            ]),
        ["Graph"] = new(
            [
                "Canvas graph composition, overlays, interaction, and primitives."
            ],
            [
                "General layout outside graph or scene authoring flows."
            ],
            [
                "Align graph surfaces with the shared canvas contract and interaction model."
            ]),
        ["Workbench"] = new(
            [
                "Canvas workbench shells and stage-level runtime surfaces."
            ],
            [
                "Lightweight page regions that should use BaseLib layout."
            ],
            [
                "Use workbench components when the page is truly a canvas workspace, not just a dense form."
            ])
    };
    private static readonly IReadOnlyDictionary<string, string> SummaryByComponent = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Tabs"] = "Shared BaseLib tabs for segmenting dense content into focused panels so workspace pages do not become one long scroll.",
        ["TabsItem"] = "Child panel definition for `Tabs` that supplies the tab label, optional icon or badge, and the pane content for the selected view.",
        ["SecondaryTabs"] = "Shared BaseLib secondary tab strip for lighter scenario switches, route toggles, and inline subnavigation.",
        ["Steps"] = "Shared BaseLib sequential progress navigation for wizard-like flows where order matters more than free switching.",
        ["PageHeader"] = "Compact BaseLib page orientation for title, concise context, supporting stats, and actions without a duplicate hero region.",
        ["Toolbar"] = "Shared BaseLib toolbar component for dense inline actions, filters, and control clusters around a working surface.",
        ["CopyButton"] = "Shared BaseLib clipboard action for copying a selected input slice or the full nearby value, with text, icon-button, and tiny inline icon variants for obvious copy affordances.",
        ["TreeView"] = "Shared BaseLib tree navigation component for hierarchical exploration and selection.",
        ["SideMenu"] = "Primary BaseLib application navigation with typed and declarative items, responsive rail-to-top-menu behavior, collapse persistence, height-measured overflow, utility panels, and service-driven context switching.",
        ["SideMenuItem"] = "Declarative typed item for `SideMenu`, including badges, overflow policy, children, custom panel content, payload, visibility, disabled state, and per-item selection callback.",
        ["Layout"] = "Top-level BaseLib application frame for natural-height or viewport-owned shells with explicit overflow control.",
        ["Sidebar"] = "Generic BaseLib application rail for arbitrary custom content; use `SideMenu` directly when the rail is primary navigation.",
        ["Body"] = "BaseLib main-content region that makes fill-height and vertical scroll ownership explicit inside `Layout`.",
        ["ThemeHost"] = "BaseLib semantic theme boundary that applies light or dark token values and control normalization to the full component tree.",
        ["DataGrid"] = "Shared BaseLib typed data grid with hardened sorting, selection, rendering, and accessibility behavior.",
        ["ContextMenu"] = "Shared BaseLib typed context menu with viewport-aware placement, keyboard interaction, and consistent selection and close behavior.",
        ["Dialog"] = "Shared BaseLib controlled dialog surface with viewport and focus behavior; use `DialogHost` plus `DialogService` for service-driven dialogs.",
        ["EmptyState"] = "Shared BaseLib empty-state component for zero-data, no-selection, and first-run orientation surfaces.",
        ["LoadingState"] = "Shared BaseLib loading-state component for progress and transition surfaces that should feel intentional instead of blank.",
        ["Alert"] = "Shared BaseLib alert component for actionable status and inline system feedback.",
        ["Notification"] = "Shared BaseLib notification host for service-triggered toast stacks with per-message viewport positioning and compact X dismiss controls.",
        ["Tooltip"] = "Shared BaseLib tooltip host for service-triggered contextual help with side, corner, and edge-aligned placement options.",
        ["TooltipTarget"] = "Shared BaseLib tooltip trigger wrapper for hover and focus help that can choose a local TooltipPosition without page-level tooltip plumbing.",
        ["FormField"] = "Shared BaseLib form-field wrapper for label, helper, validation, and control alignment.",
        ["TextInput"] = "Shared BaseLib text input component for standard single-line entry flows.",
        ["TextAreaInput"] = "Shared BaseLib multiline input with semantic compact, standard, and extended heights selected from expected content length.",
        ["NumberInput"] = "Typed BaseLib numeric input with InputBase validation and invariant value parsing.",
        ["SelectInput"] = "Typed BaseLib selection input with explicit option child content and InputBase validation.",
        ["Avatar"] = "Person-oriented identity image with a deterministic bundled human portrait when no explicit image is supplied.",
        ["HomoAvatar"] = "Deterministic generated human-style SVG avatar for an explicitly selected illustrated identity.",
        ["RoboAvatar"] = "Optional deterministic robot-style SVG avatar; use it only when the user selects the robot identity style.",
        ["Card"] = "Shared BaseLib item surface with optional media, header, tags, body, and action slots for consistent card anatomy.",
        ["CardGrid"] = "Shared BaseLib responsive collection for peer visual cards with compact, consistent gaps.",
        ["CompactStat"] = "Shared BaseLib compact label/value pair for a short supporting metric without card-sized chrome.",
        ["CompactStatStrip"] = "Shared BaseLib wrapping row for supporting metrics that should not displace the page's primary task.",
        ["SummaryTiles"] = "Shared BaseLib metric-first tile collection for sparse pages where summary metrics deserve prominent card-sized space.",
        ["MetricCard"] = "Shared BaseLib card for a primary metric that deserves more hierarchy than an inline compact stat.",
        ["SectionCard"] = "Shared BaseLib section surface for grouped content blocks, often inside a scaffold, grid, or tabs panel.",
        ["PageScaffold"] = "Shared BaseLib page shell that coordinates header, lead, and workspace regions so dense pages can use width intentionally.",
        ["ListDetailShell"] = "Shared BaseLib master-detail shell for selection-driven workspaces with a stable list region and a focused detail region.",
        ["CanvasWorkbench"] = "Shared CanvasLib workbench surface for graph-based authoring, tool windows, and dense desktop-first runtime composition.",
        ["CanvasCalendar"] = "Shared CanvasLib calendar surface for schedule navigation, selection, and event-oriented workspace views.",
        ["CanvasFloatingWindow"] = "Shared CanvasLib floating window surface for movable auxiliary panels inside the workbench runtime.",
        ["EmptyStateOverlay"] = "Shared CanvasLib empty-state overlay for graph and canvas surfaces with no active content.",
        ["CdaChart"] = "Shared Charts wrapper for Apex-backed line, area, bar, pie, and donut charts using CanDoItAll-owned series, point, option, and palette models.",
        ["ChartsHeadAssets"] = "Shared Charts asset component that adds the ApexCharts stylesheet required by the CanDoItAll chart wrapper.",
        ["ChartsBodyAssets"] = "Shared Charts asset component that loads BaseLib theme tokens and the chart theme bridge for initial colors and runtime theme updates.",
        ["MermaidDiagram"] = "CanDoItAll Mermaid.js wrapper that renders official Mermaid diagrams with Blazor callbacks for node clicks, pan/zoom controls, and normalized syntax diagnostics."
        ,
        ["OverlayWindow"] = "Generic OverlayLib floating-window runtime for bounded placement, drag, resize, minimize, reset, hide, and show behavior backed by `OverlayWindowState`.",
        ["OverlayComponentToolbox"] = "Searchable OverlayLib toolbox surface for grouped component choices, badges, selection, and collapsible sections inside a workbench tool window.",
        ["OverlayLibHeadAssets"] = "OverlayLib host asset component that loads the shared floating-window stylesheet.",
        ["OverlayLibBodyAssets"] = "OverlayLib host asset component that loads the plain floating-window runtime once near the end of the body.",
        ["QrCodeView"] = "Accessible QRCode SVG renderer for a generic payload, optional caption, render options, and graceful empty or render-error state.",
        ["QrCodeButton"] = "BaseLib page-header action that opens a dialog containing a typed generic QR payload.",
        ["QrCodeDialogContent"] = "Dialog content for displaying a typed generic QR payload with shared copy and layout behavior.",
        ["QrScanButton"] = "BaseLib page-header action that opens camera QR scanning with a manual decoded-text fallback and typed result callback.",
        ["QrScanDialogContent"] = "QRCode scanner dialog content for camera or manual payload entry with classification, validation, callback, and dialog result handling."
    };
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> TagsByComponent = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
    {
        ["Tabs"] =
        [
            "progressive-disclosure",
            "reduce-scroll",
            "segmented-workspace",
            "panel-switching",
            "settings-rail",
            "overflow"
        ],
        ["TabsItem"] =
        [
            "tab-panel",
            "panel-content",
            "badge"
        ],
        ["SecondaryTabs"] =
        [
            "subnavigation",
            "filters",
            "scenario-switch"
        ],
        ["Steps"] =
        [
            "wizard",
            "progress",
            "sequential-navigation"
        ],
        ["CopyButton"] =
        [
            "clipboard",
            "copy-action",
            "inline-action",
            "link",
            "path",
            "hash",
            "token"
        ],
        ["Notification"] =
        [
            "feedback",
            "toast",
            "notification-position",
            "overlay-service",
            "dismiss"
        ],
        ["Tooltip"] =
        [
            "contextual-help",
            "placement",
            "tooltip",
            "overlay-service",
            "hover"
        ],
        ["TooltipTarget"] =
        [
            "contextual-help",
            "placement",
            "tooltip-trigger",
            "hover",
            "focus"
        ],
        ["PageScaffold"] =
        [
            "page-shell",
            "layout",
            "workspace"
        ],
        ["Layout"] = ["app-shell", "viewport", "scroll-owner", "responsive-layout"],
        ["Body"] = ["app-shell", "main-content", "scroll-owner"],
        ["Sidebar"] = ["app-shell", "generic-rail"],
        ["ThemeHost"] = ["theme", "light-dark", "semantic-tokens", "app-shell"],
        ["SideMenu"] = ["app-shell", "primary-navigation", "responsive", "collapse", "overflow", "more-menu", "mobile-menu", "persistence", "service-driven"],
        ["SideMenuItem"] = ["primary-navigation", "declarative-item", "badge", "overflow", "panel", "payload"],
        ["DataGrid"] = ["data-grid", "table", "sorting", "selection", "accessibility"],
        ["ContextMenu"] = ["context-menu", "keyboard", "viewport-placement", "actions"],
        ["Dialog"] = ["dialog", "modal", "focus", "viewport", "overlay-service"],
        ["PageHeader"] = ["compact-ui", "page-title", "header-actions", "supporting-stats", "reduce-scroll"],
        ["TextAreaInput"] = ["form", "multiline", "content-aware-size", "compact-ui", "long-text"],
        ["TextInput"] = ["form", "input", "validation", "single-line"],
        ["NumberInput"] = ["form", "input", "validation", "numeric"],
        ["SelectInput"] = ["form", "input", "validation", "selection", "options"],
        ["Avatar"] = ["identity", "avatar", "human", "portrait", "colleague"],
        ["HomoAvatar"] = ["identity", "avatar", "human", "generated"],
        ["RoboAvatar"] = ["identity", "avatar", "robot", "optional"],
        ["Card"] = ["card-anatomy", "media", "tags", "actions", "visual-collection"],
        ["CardGrid"] = ["card-collection", "responsive-grid", "visual-items", "compact-ui"],
        ["CompactStat"] = ["supporting-metric", "compact-ui", "label-value", "reduce-scroll"],
        ["CompactStatStrip"] = ["supporting-metrics", "compact-ui", "metric-strip", "reduce-scroll"],
        ["SummaryTiles"] = ["metric-first", "summary-tiles", "dashboard"],
        ["MetricCard"] = ["metric-first", "kpi", "dashboard-card"],
        ["OverlayWindow"] = ["floating-window", "drag", "resize", "minimize", "bounded-overlay"],
        ["OverlayComponentToolbox"] = ["component-toolbox", "search", "grouped-items", "floating-window"],
        ["QrCodeView"] = ["qr-code", "svg", "payload", "accessibility"],
        ["QrCodeButton"] = ["qr-code", "dialog", "page-header-action"],
        ["QrScanButton"] = ["qr-code", "scanner", "camera", "manual-fallback", "dialog"],
        ["QrScanDialogContent"] = ["qr-code", "scanner", "camera", "manual-fallback", "validation"],
        ["SectionCard"] =
        [
            "panel",
            "surface",
            "section"
        ],
        ["ListDetailShell"] =
        [
            "master-detail",
            "detail-pane",
            "split-view"
        ],
        ["CdaChart"] =
        [
            "apex",
            "area",
            "bar",
            "chart-wrapper",
            "datetime-axis",
            "donut",
            "line",
            "multi-series",
            "pie",
            "visualization"
        ],
        ["ChartsHeadAssets"] =
        [
            "assets",
            "apex",
            "chart-wrapper",
            "stylesheet"
        ],
        ["MermaidDiagram"] =
        [
            "mermaid",
            "diagram",
            "flowchart",
            "architecture-beta",
            "pan-zoom",
            "node-click",
            "syntax-diagnostics"
        ]
    };
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> AdditionalDependenciesByComponent = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
    {
        ["Tabs"] = ["TabsItem"],
        ["Steps"] = ["StepsItem"],
        ["SideMenu"] = ["SideMenuItem", "Icon"],
        ["QrCodeButton"] = ["PageHeaderActionButton", "DialogHost"],
        ["QrScanButton"] = ["PageHeaderActionButton", "DialogHost"],
        ["QrCodeDialogContent"] = ["QrCodeView", "DialogScaffold", "CopyButton"],
        ["QrScanDialogContent"] = ["DialogScaffold", "Button"],
        ["CanvasFloatingWindow"] = ["OverlayWindow"],
        ["CanvasLibHeadAssets"] = ["OverlayLibHeadAssets"],
        ["CanvasLibBodyAssets"] = ["OverlayLibBodyAssets"]
    };
    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> MemberDescriptionsByComponent = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
    {
        ["Tabs"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["AriaLabel"] = "Accessible label announced for the whole tab list when the surrounding heading is not enough.",
            ["BorderMode"] = "Controls whether the tab buttons render with the default border, a softer border treatment, or no border at all.",
            ["Change"] = "Raised after a user-initiated tab switch so flows can react to the new selected panel.",
            ["ChildContent"] = "Optional fallback child content path when the tab items are declared directly instead of inside `TabItems`.",
            ["Class"] = "Adds extra Tailwind classes to the tabs root for shell-level atmosphere without replacing the shared internal structure.",
            ["Density"] = "Independent spacing preset: Normal, Comfortable, or Compact. Replaces the removed TabsVariant presets.",
            ["Rounded"] = "Corner radius of the tab buttons, independent of density and content-panel rounding.",
            ["ContentRounded"] = "Corner radius of the content-panel wrapper.",
            ["NoContent"] = "Omits the content wrapper for a navigation-only strip whose routed content is owned elsewhere.",
            ["NoSlider"] = "Hides the selected-tab underline indicator without changing selection behavior.",
            ["OverflowMode"] = "Chooses whether long tab strips auto-decide, wrap into multiple rows, or stay single-row and scroll horizontally.",
            ["PanelOverflowMode"] = "Controls vertical overflow inside the content panel; use with FillHeight only when this panel owns scrolling.",
            ["RenderMode"] = "Chooses server rendering of only the active panel or client rendering of every panel for faster instant switches.",
            ["SelectedIndex"] = "Zero-based selected tab index among the visible tabs.",
            ["SelectedIndexChanged"] = "Two-way binding callback for the selected tab index.",
            ["Style"] = "Appends inline styles to the tabs root when a one-off shell tweak is unavoidable.",
            ["TabItems"] = "Slot that contains the `TabsItem` children defining labels and panel content.",
            ["TabPosition"] = "Moves the tab strip to the top, bottom, left, or right of the panel surface.",
            ["Tone"] = "Advanced accent color for the selected state. Use it to shift emphasis, not to rescue readability."
        },
        ["TabsItem"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["AdditionalAttributes"] = "Passes extra attributes and classes to the rendered tab button.",
            ["BadgeText"] = "Optional supporting badge shown inside the tab button for counts or short status markers.",
            ["ChildContent"] = "Panel content rendered when the tab becomes active.",
            ["Disabled"] = "Keeps the tab visible but unavailable for selection.",
            ["Icon"] = "Optional icon token rendered before the tab text.",
            ["Link"] = "Optional navigation URL rendered as an anchor; pair navigation-only tabs with NoContent when the router owns content.",
            ["Text"] = "Primary tab label. If omitted, the component falls back to a stable generic title.",
            ["Visible"] = "Removes the tab from the rendered strip when false."
        },
        ["SecondaryTabs"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Header"] = "Optional content rendered above the secondary tab strip for local orientation.",
            ["Items"] = "Collection of lightweight tab items used for compact scenario switching or route toggles.",
            ["SelectedKey"] = "Currently selected item key.",
            ["SelectedKeyChanged"] = "Two-way binding callback for the selected item key."
        },
        ["SectionCard"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Description"] = "Supporting copy for the section header.",
            ["Title"] = "Section heading rendered by the shared panel shell."
        },
        ["PageHeader"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Compact"] = "Uses the compact operational-page rhythm while retaining a concise description when one is supplied.",
            ["Description"] = "Concise current context for the page; do not repeat it in a second hero or introductory card.",
            ["Stats"] = "Supporting metrics rendered beside page orientation. Prefer `CompactStatStrip` content and omit the slot when metrics are not decision-relevant.",
            ["Actions"] = "Small primary action set for the page. Keep working filters and surface-specific commands in a nearby `Toolbar`.",
            ["Outlet"] = "Optional cascading IPageHeaderOutlet owned by the host. Publishes PageHeaderPresentation on every parameter update so captured fragments stay current; clearing is scoped to the publishing owner. Invalidate only the toolbar and outlet renderers, not the routed body."
        },
        ["TextAreaInput"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Size"] = "Semantic expected-content height: `Compact` for short comments, `Standard` for ordinary descriptions, and `Extended` for substantial prose.",
            ["Rows"] = "Explicit row override. Leave it unset or zero to let `Size` choose the default height; set it only for a known content-specific requirement."
        },
        ["SelectInput"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            ["ChildContent"] = "Explicit option elements, including typed enum values and a deliberate empty option for nullable selection. There is no Data or DropDownOption collection.",
            ["ValueExpression"] = "Expression identifying the bound model field for EditContext validation; @bind-Value supplies it automatically."
        },
        ["Avatar"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            ["ImageUrl"] = "Explicit selected image. Keep it when changing other identity settings.",
            ["DefaultImageSeed"] = "Stable identity seed used to select a bundled human portrait when no image is supplied.",
            ["UseDefaultImageWhenMissing"] = "Defaults to true and uses a human portrait. False requests the FallbackText rendering explicitly."
        },
        ["Dialog"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Size"] = "Task-layout width: `Compact` for a short single-column form, `Medium` for ordinary grouped forms, `Wide` for genuine multi-column review, and `Full` only for workbench-like tasks.",
            ["DenseChrome"] = "Reduces dialog header, body, and footer chrome for short focused tasks without shrinking the form controls themselves."
        },
        ["Card"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Media"] = "Optional centered and constrained media region at the top of a visual item card.",
            ["Header"] = "Item identity and short status region; keep the hierarchy consistent across peer cards.",
            ["Tags"] = "Short categories or metadata adjacent to identity; use shared badge and chip components.",
            ["ChildContent"] = "Primary card content. Legacy child-only cards remain valid when structured regions are unnecessary.",
            ["Actions"] = "Small action set aligned consistently at the card footer; avoid duplicating common collection-level commands on every card."
        },
        ["CardGrid"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Section"] = "Adds section semantics to the shared responsive card collection; use `Grid` when custom tracks are required.",
            ["ChildContent"] = "Peer `Card` items with consistent anatomy and action placement."
        },
        ["CompactStat"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Label"] = "Short metric label that remains scannable in a compact strip.",
            ["Value"] = "Short supporting value; use `Text` only as the compatibility alias when needed.",
            ["HelperText"] = "Optional brief context. Move longer explanation into visible page copy or contextual help."
        },
        ["SelectionListItem"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Disabled"] = "When `OnSelect` is supplied, disables only the rendered selection button. It does not affect passive content or consumer-owned actions.",
            ["OnSelect"] = "Enables row selection: when supplied, the content region renders as a selection button and invokes this callback; without it, content renders as a noninteractive surface.",
            ["Actions"] = "Consumer-owned row commands rendered outside the optional selection button. Apply disabled state explicitly to every action that should be unavailable."
        },
        ["PageScaffold"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ChildContent"] = "Main page body region where tabs, split views, grids, and sections should live.",
            ["Header"] = "Top page header region for orientation and primary actions.",
            ["Lead"] = "Optional lead content that introduces the workspace before the main body.",
            ["SecondaryRail"] = "Optional side rail for supporting context, shortcuts, or meta information."
        },
        ["CdaChart"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["AdditionalAttributes"] = "Passes stable selectors such as `data-testid` to the chart wrapper root for browser validation.",
            ["Class"] = "Adds extra classes to the wrapper shell without replacing the chart-library boundary.",
            ["Description"] = "Optional supporting text rendered above the chart to explain the data shape or operational context.",
            ["EmptyText"] = "Message shown when every supplied series has no points.",
            ["Height"] = "Chart height forwarded to the underlying renderer; use stable dimensions so labels and toolbars do not resize the layout.",
            ["Options"] = "CanDoItAll-owned chart options that map chart type, axes, fill, legend, toolbar, labels, colors, and formatting to the current renderer.",
            ["Series"] = "CanDoItAll-owned series collection built from `CdaChartSeries` and `CdaChartPoint`; do not pass raw Apex series markup.",
            ["Style"] = "Adds inline style to the wrapper root for host-level sizing only.",
            ["Title"] = "Optional chart title rendered by the wrapper before the plot."
        },
        ["ChartsHeadAssets"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Href"] = "Stylesheet path for ApexCharts CSS; keep the default unless the external chart asset path changes."
        },
        ["ChartsBodyAssets"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["IncludeThemeTokens"] = "Loads BaseLib's shared theme script before the chart theme bridge. Keep the default true unless an earlier asset component already loads it; the chart-specific bridge is always required."
        },
        ["MermaidDiagram"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Description"] = "Optional supporting copy shown above the rendered Mermaid diagram.",
            ["Error"] = "Callback raised when Mermaid parsing or rendering fails; the payload includes normalized line, column, excerpt, token, and expected-token details when available.",
            ["NodeClicked"] = "Callback raised when the wrapper detects a click or keyboard activation on a rendered diagram node.",
            ["Options"] = "Typed Mermaid initialization options, including theme, security level, flowchart settings, and architecture randomization.",
            ["PanZoomEnabled"] = "Enables the wrapper viewBox pan and zoom behavior on the rendered SVG.",
            ["Rendered"] = "Callback raised after Mermaid renders successfully with diagram id, SVG id, and node count.",
            ["ShowControls"] = "Shows the wrapper zoom-out, reset, and zoom-in toolbar when pan/zoom is enabled.",
            ["Source"] = "Mermaid source text passed to the official Mermaid parser and renderer.",
            ["Title"] = "Optional diagram title shown above the rendered SVG."
        },
        ["Notification"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Position"] = "Default viewport stack used for messages that do not set `NotificationMessage.Position`; prefer per-message positions when the toast belongs near a specific action region."
        },
        ["Layout"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["FitViewport"] = "Makes the shell consume the viewport after `ViewportOffset`; use only when the application owns vertical scrolling.",
            ["ViewportOffset"] = "Height reserved for chrome outside this layout when `FitViewport` is enabled.",
            ["LockOverflow"] = "Prevents shell overflow when a child `Body` or work surface is the intended scroll owner."
        },
        ["Body"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["FillHeight"] = "Participates in the parent full-height shell and permits an intentional inner scroll owner.",
            ["Scrollable"] = "Enables vertical scrolling on the main content region; keep only one vertical scroll owner in the shell."
        },
        ["SideMenu"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["MenuId"] = "Stable application-wide identity used by SideMenuService subscriptions and default localStorage expansion persistence.",
            ["Items"] = "Runtime primary destinations. These compose with declarative `MenuItems`; IDs must stay unique across both sources.",
            ["MoreItems"] = "Typed destinations that always appear in the More surface alongside height-overflowed primary items.",
            ["BottomItems"] = "Typed utility destinations such as account, settings, and help, rendered in the rail or mobile bottom region.",
            ["MenuItems"] = "Declarative primary `SideMenuItem` slot; compose it with `Items` instead of choosing only one authoring style.",
            ["MoreMenuItems"] = "Declarative typed More-menu item slot for secondary destinations.",
            ["BottomMenuItems"] = "Declarative utility item slot for account, settings, or help panels.",
            ["MoreContent"] = "Optional arbitrary BaseLib content appended below typed More items; do not put primary navigation destinations here.",
            ["Expanded"] = "Two-way collapse state. It is remembered by default per `MenuId` unless persistence is disabled or overridden.",
            ["AllowCollapse"] = "Shows the desktop rail collapse control while preserving the automatic small-screen top-menu behavior.",
            ["FillHeight"] = "Lets the menu share a bounded application shell height so measured overflow can move items to More.",
            ["PersistExpandedState"] = "Stores expansion in localStorage. Disable when the application owns persistence explicitly.",
            ["VisibleItemCapacity"] = "Optional fixed desktop capacity. Normally omit it and let measured available height control overflow.",
            ["ItemSelected"] = "Raised for pointer, keyboard, and externally driven selections; map item IDs or payloads to route or feature behavior here."
        },
        ["SideMenuItem"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Id"] = "Stable unique item identity within the menu.",
            ["OverflowBehavior"] = "Keeps an item eligible for automatic placement, prefers it in the visible rail, or always places it under More.",
            ["Activation"] = "Chooses normal selection or opening a child/custom panel for this destination.",
            ["Children"] = "Typed child actions shown in the item panel; use for bounded utilities rather than a deep primary-navigation tree.",
            ["PanelContent"] = "Optional custom BaseLib content for the item panel.",
            ["Payload"] = "Typed application-owned context passed with the selected item; use it instead of parsing labels or magic strings."
        },
        ["OverlayWindow"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["WindowId"] = "Stable DOM/runtime identity for the floating window.",
            ["State"] = "Controlled `OverlayWindowState` containing visibility, minimized state, and normalized geometry.",
            ["ContainerSelector"] = "Selector for the real bounding host. Set it when the default canvas selectors do not describe the consumer surface.",
            ["SafeTopSelector"] = "Selector for toolbar or safe-top chrome that the window must not cover.",
            ["AllowResize"] = "Enables the shared resize lifecycle; keep it false only for intentionally fixed geometry.",
            ["StateChanged"] = "Returns normalized window state after movement, resize, visibility, minimize, or reset behavior."
        },
        ["QrCodeView"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Payload"] = "Required generic text encoded into the QR SVG.",
            ["Options"] = "Typed error-correction and rendering options supplied to the registered QR renderer.",
            ["AriaLabel"] = "Accessible purpose for the rendered code; describe the action or payload context, not the visual shape."
        },
        ["QrScanButton"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["EnableCamera"] = "Enables the browser camera scanner while the dialog still retains its manual decoded-text fallback.",
            ["Scanned"] = "Returns the typed, classified scan result after a valid camera or manual payload is accepted."
        },
        ["TooltipTarget"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ChildContent"] = "Trigger content that opens the tooltip on hover and focus.",
            ["CloseOnMouseLeave"] = "Closes the tooltip when the pointer leaves the trigger; keep enabled for short hover help.",
            ["Delay"] = "Optional wait before showing the tooltip so dense toolbars do not flicker during quick pointer movement.",
            ["Duration"] = "Optional lifetime after opening; set null or a longer value only for tooltip content that needs extra reading time.",
            ["Position"] = "Tooltip placement relative to the pointer; choose the side or corner that keeps the bubble inside the viewport and away from the next likely action.",
            ["TabIndex"] = "Keyboard focus order for the trigger wrapper.",
            ["TestId"] = "Optional stable selector for Playwright checks of non-default tooltip placements.",
            ["Text"] = "Short plain-text tooltip content for compact help.",
            ["TooltipClass"] = "Additional Tailwind classes for the tooltip surface, without replacing the shared overlay structure.",
            ["TooltipContent"] = "Rich tooltip content rendered with access to the shared TooltipService.",
            ["TriggerClass"] = "Additional Tailwind classes for the inline trigger wrapper."
        }
    };
    private static readonly IReadOnlyDictionary<string, string> DefaultParameterDescriptionsByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["AdditionalAttributes"] = "Passes unmatched HTML attributes through to the rendered element.",
        ["ChildContent"] = "Slot for the component body content.",
        ["Class"] = "Adds extra Tailwind classes to the component root without replacing the shared structure.",
        ["Style"] = "Appends inline styles to the component root."
    };
    private static readonly IReadOnlyDictionary<string, ComponentGuidanceDocument> GuidanceByComponent = new Dictionary<string, ComponentGuidanceDocument>(StringComparer.OrdinalIgnoreCase)
    {
        ["CanvasWorkbench"] = new(
            ["Interactive node canvases with shared selection, navigation, clipboard requests, and selected-node actions."],
            ["Independent document keyboard handlers or hover timers that compete with the shared canvas runtime."],
            [
                "Keep runtime and preview surfaces aligned with the shared canvas contracts and CanvasThemeTokenPack; mount CanvasLibHeadAssets and CanvasLibBodyAssets for the complete shared assets.",
                "Populate CanvasWorkbenchNode.ToolbarActions for the single selected-node toolbar. The visible Node actions control and Ctrl+M toggle one toolbar whose size stays independent of canvas zoom and whose position is clamped to the canvas. Escape closes it and restores canvas focus.",
                "Set CanvasWorkbenchAction.KeyboardCommand to CanvasWorkbenchKeyboardCommand.CopyIdentifier for Ctrl+I; None adds no keyboard command. ShortcutKey retains its existing unmodified-key shortcut within the active context menu. Ctrl+I dispatches the copy-identifier action even when the toolbar is closed.",
                "The shared router owns active-canvas scope and leaves typing controls, editable content, descendant dialogs and overlays, and other canvases isolated. A canvas hosted inside an outer dialog still owns its focused keys. Do not add a competing global key listener or restore hover-driven toolbar timers.",
                "Handle ContextActionRequested in the consumer for toolbar and copy-identifier commands. The host retains current-selection validation, authorization, copy-info/tree/delete behavior, and OS clipboard writes; declaring a toolbar action does not grant mutation authority.",
                "Existing Ctrl+C, Ctrl+X, Ctrl+V, and Ctrl+D continue through ClipboardRequested and CanvasWorkbenchClipboardOptions capabilities. Ctrl+Z and Ctrl+Y remain unhandled by the shared command router.",
                "Use the existing /canvas#canvas-workbench sample in examples/Canvas/WorkbenchWithFloatingWindows.razor to exercise typed ToolbarActions, ContextActionRequested receipts, single-selection eligibility, and toolbar dismissal after node removal."
            ]),
        ["PageHeader"] = new(
            [
                "Compact page orientation with a title, concise current context, supporting metrics, and a small action set.",
                "Operational pages where useful work should appear early in the viewport."
            ],
            [
                "A decorative marketing hero or repeated introduction above another title.",
                "A container for every filter and surface-specific command on the page."
            ],
            [
                "Default to `Compact=true` for operational pages and keep a concise `Description` when it prevents ambiguity.",
                "Use `Stats` for supporting `CompactStatStrip` content, not a second row of large metric cards.",
                "Keep only page-level actions in `Actions`; place working controls in a toolbar next to the surface they affect."
            ]),
        ["TextAreaInput"] = new(
            [
                "Multiline comments, descriptions, notes, prompts, and substantial prose with a height matched to expected content."
            ],
            [
                "Single-line names, identifiers, or commands that belong in `TextInput`.",
                "Using one tall default for every multiline field regardless of expected content."
            ],
            [
                "Use `TextAreaSize.Compact` for short comments, `Standard` for ordinary descriptions, and `Extended` for substantial prose.",
                "Set `Rows` only when a known content shape needs a deliberate override; an explicit value takes precedence over `Size`.",
                "Give the field the available form width unless a narrower width communicates a real input constraint."
            ]),
        ["TextInput"] = new(
            ["Single-line model fields, filters, names, and identifiers."],
            ["Multiline prose or application-owned validation hidden in DOM event handlers."],
            [
                "Use @bind-Value for model fields so ValueExpression identifies the real field in EditContext.",
                "Use Look and FormDensity for visual treatment; preserve input validation and explicit Disabled or ReadOnly states."
            ]),
        ["NumberInput"] = new(
            ["Typed numeric model fields, including nullable numeric values."],
            ["Text fields that merely happen to contain digits but are identifiers."],
            ["Set TValue to the domain numeric type and use @bind-Value so parsing errors remain visible through EditContext validation."]),
        ["SelectInput"] = new(
            ["Typed selection from a bounded list or enum."],
            ["Passing the removed DropDown Data or DropDownOption contract to the refactored input."],
            [
                "Declare option elements in ChildContent, use the actual value type as TValue, and bind Value to the model field.",
                "Use an empty option only when the domain permits no selection; do not silently replace an invalid selection with a default."
            ]),
        ["Avatar"] = new(
            ["People and agent colleagues whose primary identity is a human portrait."],
            ["Replacing an existing selected portrait merely because a robot style is available."],
            [
                "Keep the human portrait as the default. AvatarFallbackResolver uses bundled avatar-01.jpg through avatar-08.jpg when ImageUrl is missing.",
                "Offer robot-01.svg through robot-08.svg from the same assets/identity/avatars directory as an explicit secondary picker choice, then persist the selected URL.",
                "Pass a stable DefaultImageSeed and meaningful Alt text; keep explicit ImageUrl selections unchanged."
            ]),
        ["HomoAvatar"] = new(
            ["An explicitly selected generated human-style identity."],
            ["Silently replacing the application's bundled or uploaded human portrait."],
            ["Use a stable Seed or Id and meaningful Title; this optional generated style is separate from Avatar's portrait default."]),
        ["RoboAvatar"] = new(
            ["An explicitly selected optional robot-style identity."],
            ["The primary default identity for agent colleagues."],
            ["Keep robot style opt-in, preserve the chosen seed or persisted image URL, and provide meaningful Title text."]),
        ["SecretField"] = new(
            [
                "Secret entry that combines an input with reveal and copy actions in one reusable field."
            ],
            [
                "A page-local input/action wrapper that duplicates the shared secret handling.",
                "Assuming a large viewport guarantees enough width inside a multi-column form."
            ],
            [
                "Treat the input, reveal control, and copy actions as a compound control that must wrap from its actual containing width, not only viewport media queries.",
                "Browser-test the field in its real form composition, including a roughly 300–320px grid column on a large desktop viewport; verify readable controls, action access, and no horizontal overflow.",
                "If the shared component cannot produce that wrapped state, improve BaseLib rather than adding page-local structural CSS."
            ]),
        ["Card"] = new(
            [
                "Peer visual items whose media, identity, tags, content, and independent actions benefit from a consistent surface."
            ],
            [
                "Dense text-only records that scan better as selection rows or a data grid.",
                "Decorative nested surfaces or long documents split into arbitrary card fragments."
            ],
            [
                "Use the `Media`, `Header`, `Tags`, body content, and `Actions` slots so peer cards keep consistent anatomy.",
                "Center and constrain top media, keep tags near identity or state, and place the small action set consistently at the footer.",
                "Legacy child-only composition remains valid for simple surfaces; do not add empty regions just to fill the template."
            ]),
        ["CardGrid"] = new(
            [
                "Responsive collections of peer visual cards with consistent spacing."
            ],
            [
                "Comparable records that need columns, sorting, or bulk operation.",
                "Collections that need custom track definitions; use `Grid` there."
            ],
            [
                "Use consistent `Card` anatomy within one grid and keep collection-level create, filter, and bulk actions outside individual cards.",
                "Do not force sparse cards to consume arbitrary height; let content fill available space logically."
            ]),
        ["CompactStat"] = new(
            [
                "One short label/value pair that supports the primary task without demanding card-sized emphasis."
            ],
            [
                "A primary KPI that needs trend, explanation, or comparison hierarchy."
            ],
            [
                "Keep labels and values short, and group peers in `CompactStatStrip`.",
                "Use a badge instead when the value is simply a count or status attached to another label."
            ]),
        ["CompactStatStrip"] = new(
            [
                "A compact wrapping row of supporting metrics beside page orientation or working controls."
            ],
            [
                "A sparse metric-first page where larger comparable cards make the main information easier to scan."
            ],
            [
                "Use this when another list, form, canvas, or table is the page's primary task.",
                "Do not repeat its values in `SummaryTiles` or `MetricCard` regions lower on the same page."
            ]),
        ["SelectionListItem"] = new(
            [
                "Selection-driven list rows with optional metadata, status, details, and independent actions.",
                "Display-oriented rows that use the shared list anatomy as a noninteractive content surface when `OnSelect` is absent."
            ],
            [
                "Adding `OnSelect` when the row has no real selection or navigation behavior.",
                "Assuming row-disabled state automatically disables consumer-owned action controls."
            ],
            [
                "Provide `OnSelect` only when activating the row performs a real selection or navigation. With a delegate, the content region renders as a selection button; without one, it renders as a noninteractive surface.",
                "When the selection button exists, `Disabled=true` disables only that button. It does not disable the `Actions` fragment.",
                "Actions are consumer-owned: pass disabled state to each action independently, and disable both selection and every action when the entire item is unavailable."
            ]),
        ["SummaryTiles"] = new(
            [
                "Sparse metric-first pages where several comparable summaries deserve prominent card-sized space."
            ],
            [
                "Supporting numbers above a list, form, or work surface; use `CompactStatStrip` or badges there."
            ],
            [
                "Keep tiles comparable and use medium or smaller shared gaps.",
                "If tiles push the primary workflow below the fold, demote the metrics to compact stats."
            ]),
        ["MetricCard"] = new(
            [
                "A primary metric with enough importance or supporting context to justify card hierarchy."
            ],
            [
                "Every short number on an operational page."
            ],
            [
                "Use comparable metric cards together in `StatsGrid`; use `CompactStat` for secondary context.",
                "Do not pad a bare number into a large card merely to fill space when a working surface needs the viewport."
            ]),
        ["Grid"] = new(
            [
                "Page and section shells that need explicit tracks.",
                "Responsive layouts that should be driven by `Columns*`, `Rows*`, or `ColumnTemplate*` parameters."
            ],
            [
                "Simple one-dimensional flows that `Stack` already expresses cleanly."
            ],
            [
                "Prefer `Columns*`, `Rows*`, and `ColumnTemplate*` before raw CSS grid utilities.",
                "Use `Gap`, `ColumnGap`, `RowGap`, `AlignItems`, and `JustifyContent` before adding wrapper markup.",
                "Let child `Row` components inherit the parent track variables when the same-row content should collapse with the parent grid."
            ]),
        ["Row"] = new(
            [
                "Nested layout bands inside a `Grid`.",
                "Same-row groups that need their own track count or need to collapse responsively."
            ],
            [
                "Simple horizontal button groups or inline content that `Stack` already handles."
            ],
            [
                "`Row` spans the available grid width and can inherit the parent grid track model automatically.",
                "Use `Columns*` or `ColumnTemplate*` on the row only when that row needs a different track model than its parent.",
                "Use responsive `ColumnsSm` and `ColumnsMd` on the row when sibling columns should wrap under each other."
            ]),
        ["Column"] = new(
            [
                "Content cells inside a `Row`.",
                "Leaf layout containers that need local flex alignment plus a grid placement."
            ],
            [
                "Standalone page shells where `Stack`, `SectionCard`, or `Grid` would express the intent more clearly."
            ],
            [
                "Treat `Column` as both a grid item and a local flex container.",
                "Use `Orientation`, `Gap`, `AlignItems`, and `JustifyContent` instead of ad-hoc flex wrappers.",
                "Use `Size*` only when the row is using span-based sizing; otherwise let inherited track definitions place the column."
            ]),
        ["Stack"] = new(
            [
                "One-dimensional vertical or horizontal flows.",
                "Grouped copy, pill rows, action clusters, and compact sidebars."
            ],
            [
                "Two-dimensional layouts that need explicit tracks across breakpoints."
            ],
            [
                "Reach for `Stack` first when the structure is linear.",
                "When a panel starts needing left/right regions or breakpoint-driven track changes, move to `Grid` or `Row` plus `Column`.",
                "Use `GapScale`, `Wrap`, `AlignItems`, and `JustifyContent` before custom flex classes."
            ]),
        ["FormRow"] = new(
            [
                "Standard paired or evenly split form fields.",
                "Form surfaces that should follow the shared label and spacing rhythm."
            ],
            [
                "Complex asymmetric layouts where `Grid` or `Row` provides clearer control."
            ],
            [
                "Use `FormRow` for common field pairings before creating a custom form wrapper.",
                "Combine `FormRow` with `FormField` so label, helper text, and control spacing stay inside BaseLib."
            ]),
        ["Tabs"] = new(
            [
                "Workspace pages whose sections are mutually exclusive and would otherwise become a long vertical scroll.",
                "Settings, review, and case-management flows that need progressive disclosure inside one stable shell.",
                "Horizontal or vertical panel switching where one selected pane should dominate the viewport."
            ],
            [
                "Short inline toggles or route chips where `SecondaryTabs` is the lighter fit.",
                "Strictly sequential flows where `Steps` should communicate order and progress.",
                "Comparison layouts where users need multiple panes visible at the same time."
            ],
            [
                "Prefer `Tabs` before stacking several `SectionCard` blocks that users read one at a time.",
                "Use Density and Rounded independently; use NoContent for a navigation-only strip rather than creating empty panels.",
                "Start with the readable default surface; use `Tone` as an advanced accent and keep shell customization on the root `Class` only.",
                "Use `TabsOverflowMode.Wrap` or `TabsOverflowMode.Scroll` intentionally for longer label sets, and move to `TabPosition.Left` for settings rails.",
                "For opened workspace tabs, prefer a single horizontally scrolling strip that preserves work area height; verify long labels, the active tab, keyboard focus, and close controls at a large desktop viewport.",
                "Keep the strip focused. If the labels stop scanning quickly, split the workflow instead of adding more tabs."
            ]),
        ["TabsItem"] = new(
            [
                "A focused pane inside `Tabs` with a task-oriented label and one coherent content region."
            ],
            [
                "Standalone section surfaces that should just be cards or layout blocks.",
                "Decorative labels with no associated panel content."
            ],
            [
                "Keep labels short and specific, then use `BadgeText` sparingly for counts or short status hints.",
                "Use Link for route navigation and let the host own routed content and open-workspace lifetime; switching the strip must not discard unrelated editor drafts.",
                "If a pane needs a unique shell, customize the parent `Tabs` root instead of rebuilding the inner tab button structure."
            ]),
        ["SecondaryTabs"] = new(
            [
                "Compact scenario switches, route segments, and inline mode changes.",
                "Lightweight navigation where the surrounding page or panel already owns the content shell."
            ],
            [
                "Dense content panels that need a visible selected seam and a shared panel surface; use `Tabs` there instead.",
                "Sequential progress indicators that should use `Steps`."
            ],
            [
                "Reach for `SecondaryTabs` when users are changing context inside an already established shell.",
                "If switching the tab also switches a substantial content panel and reduces page scroll, promote the pattern to `Tabs`."
            ]),
        ["Steps"] = new(
            [
                "Wizard-like flows, checklists, and ordered progress where completion state matters.",
                "Navigation that should teach the user what comes next."
            ],
            [
                "Free workspace switching between independent panels where tabs would be clearer."
            ],
            [
                "Use `Steps` when sequence is the message. If users need to hop between peer sections, use `Tabs` instead."
            ]),
        ["ThemeHost"] = new(
            [
                "The semantic visual boundary around the complete interactive application.",
                "Light or dark mode driven by `CadThemes` and the shared CSS variable contract."
            ],
            [
                "One-off theme wrappers around individual cards or pages that create inconsistent token scopes."
            ],
            [
                "Wrap the root interactive layout once and keep app-owned theme overrides in an app stylesheet loaded after BaseLib output.",
                "Use `ThemeKey` for the `data-ui-theme` boundary and set `ColorScheme` explicitly for custom keys when native controls need light or dark browser styling.",
                "Use current semantic tone variables from `Tailwind/theme.css`, pair foreground and background colors, and preserve fixed neutral colors where surfaces intentionally keep one appearance.",
                "Verify light-to-dark and dark-to-light switching on the same rendered surface, including open overlays and native form controls; do not hand-edit generated output CSS."
            ]),
        ["Layout"] = new(
            [
                "The top-level natural-height or viewport-owned application frame.",
                "A shell that places primary navigation beside the main content region."
            ],
            [
                "A local content wrapper inside a routed page.",
                "Nested application frames that create competing viewport and overflow rules."
            ],
            [
                "Pair `Layout FitViewport=true LockOverflow=true` with exactly one child scroll owner such as `Body Scrollable=true`.",
                "Place `SideMenu` directly inside `Layout`; it already renders an aside and should not be wrapped by `Sidebar`.",
                "Use `ViewportOffset` only for chrome that truly sits outside the layout."
            ]),
        ["Body"] = new(
            [
                "The main content region beside primary application navigation.",
                "The deliberate scroll owner in a full-height app shell."
            ],
            [
                "Dialog body content; use dialog scaffolds there instead.",
                "A second nested scrolling region without a clearly bounded work surface."
            ],
            [
                "Use `FillHeight=true Scrollable=true` for a full-height product shell whose pages scroll naturally.",
                "Use `Scrollable=false` only when a full-height child such as a focused workbench owns overflow."
            ]),
        ["Sidebar"] = new(
            [
                "An arbitrary custom application rail with explicit height and scrolling behavior."
            ],
            [
                "Wrapping `SideMenu`, which is already the semantic and visual primary navigation aside."
            ],
            [
                "Use this for a bespoke supporting rail; pair it directly with `Body` inside `Layout`."
            ]),
        ["SideMenu"] = new(
            [
                "Primary application destinations that must work as a desktop rail and a small-screen top dropdown.",
                "Context-sensitive navigation whose item set can be temporarily replaced by `SideMenuService`.",
                "Navigation with bounded utility panels, bottom account/settings actions, and measured overflow."
            ],
            [
                "Deep arbitrary hierarchy; use `TreeView` for that content navigation.",
                "A static anchor list when callers will not handle selection and route synchronization.",
                "Page-local mobile navigation duplicates or custom More-menu capacity calculations."
            ],
            [
                "Give the menu one stable `MenuId`; use a matching `SideMenuService` scope and dispose `Subscribe` registrations.",
                "Compose list-fed `Items` with declarative `<MenuItems>` only when all IDs are unique. Use `MoreItems` and `BottomItems` for their semantic regions.",
                "Handle `ItemSelected` for pointer, keyboard, and external selection. Map stable IDs or typed payloads to routing; do not infer behavior from labels.",
                "Let the component measure height for More overflow. Use `OverflowBehavior.AlwaysInMore` for secondary destinations and `PreferVisible` only for genuinely important ones.",
                "Let the component become a top menu below the small breakpoint and validate rail, More, panels, keyboard, and resize behavior in `/navigation#side-menu`."
            ]),
        ["SideMenuItem"] = new(
            [
                "A static primary, More, or bottom utility item inside `SideMenu`.",
                "A bounded child list or BaseLib panel attached to a utility destination."
            ],
            [
                "Repeating an ID used by list-fed menu items.",
                "Representing a deeply nested content tree or a page-local arbitrary dropdown."
            ],
            [
                "Keep `Text` scannable, `Icon` recognizable, and `Description` concise for mobile and panels.",
                "Use `Payload` for typed consumer context and `OverflowBehavior` for destination priority."
            ]),
        ["DataGrid"] = new(
            [
                "Comparable records that need table columns, sorting, selection, and keyboard-accessible operation."
            ],
            [
                "A single selected object with a persistent detail workspace; use `ListDetailShell`.",
                "Label/value facts or a static report that does not need grid interaction."
            ],
            [
                "Declare typed `DataGridColumn` children and keep row actions concise.",
                "Validate keyboard navigation, selected-row behavior, sorting, and dense data at realistic viewport widths."
            ]),
        ["Dialog"] = new(
            [
                "A controlled component-owned modal or an isolated focused task.",
                "Bounded create or edit flows that should not permanently displace a collection or work surface."
            ],
            [
                "Transient feedback that needs only `Notification`.",
                "Permanent page navigation or long scrolling workspaces.",
                "Tasks whose information must stay visible for comparison with the underlying page."
            ],
            [
                "Use `DialogService` and one mounted `DialogHost` for service-driven flows; use controlled `IsOpen` state only when the owning component truly owns the lifecycle.",
                "Prefer `DangerActionDialog` for destructive confirmation and `DialogScaffold` for a standard title, copy, body, and footer.",
                "Choose `ModalSize.Compact` for short single-column forms, `Medium` for ordinary grouped forms, `Wide` for genuine multi-column review, and `Full` only for workbench-like tasks.",
                "Use `DenseChrome=true` for short focused tasks and load dialog-only data when opening instead of front-loading the page."
            ]),
        ["ContextMenu"] = new(
            [
                "Contextual actions attached to one item, row, node, or work surface."
            ],
            [
                "Primary navigation, permanent toolbars, or long multi-step task forms."
            ],
            [
                "Use shared menu primitives and callbacks; let the component handle placement, dismissal, and keyboard behavior.",
                "Keep commands contextual and concise. Promote commonly used actions to visible page or row controls."
            ]),
        ["TreeView"] = new(
            [
                "Accessible hierarchy browsing, expansion, selection, and keyboard navigation."
            ],
            [
                "Top-level product destinations that should be a `SideMenu`.",
                "A small finite set of peer modes that tabs communicate more clearly."
            ],
            [
                "Preserve semantic parent-child data and test keyboard expansion, collapse, and selection in a real browser."
            ]),
        ["PageScaffold"] = new(
            [
                "Full-page shells that need a stable header and a deliberate content region.",
                "Dense pages that should use width intentionally instead of defaulting to a narrow single-column stack."
            ],
            [
                "Tiny inline content regions where a local `SectionCard`, `Grid`, or `Stack` is enough."
            ],
            [
                "Make the primary task surface the largest and earliest region; do not repeat page orientation in a separate hero.",
                "Use tabs, split views, or grid-based regions inside the scaffold body when content naturally breaks into modes or panes.",
                "Move bounded create/edit work into dialogs and avoid turning the scaffold body into a long stack of unrelated cards."
            ]),
        ["SectionCard"] = new(
            [
                "Grouped content blocks inside a page, grid cell, or tabs panel."
            ],
            [
                "Using many stacked cards to simulate navigation between mutually exclusive content modes."
            ],
            [
                "If several section cards represent alternate views of the same object, promote them into `Tabs` so the page does not grow vertically without limit."
            ]),
        ["CopyButton"] = new(
            [
                "Copying links, file paths, hashes, tokens, invite codes, commands, and other obvious clipboard targets.",
                "Inline copy affordances next to a nearby link, input, path, hash, or read-only value where a one-click copy action is expected."
            ],
            [
                "Primary page actions or ambiguous icon-only controls where the copied target is not visually obvious.",
                "Custom copy wrappers when the shared component can target the nearby element with `TargetElementId` or `TargetSelector`."
            ],
            [
                "Use `CopyButton` automatically for obvious copyable values such as links, paths, hashes, IDs, and tokens instead of building local clipboard logic.",
                "Default to `DisplayMode=IconOnly` with `Size=ExtraSmall` or `Size=Small` when the copy target is adjacent and self-explanatory.",
                "Use `DisplayMode=Text`, `TextWithIcon`, or `IconButton` when the control stands alone or the copy target needs more context.",
                "Let the built-in copied-state check icon provide lightweight confirmation instead of adding extra status chrome for simple copy flows."
            ]),
        ["Notification"] = new(
            [
                "Transient confirmations, save results, background task outcomes, and non-blocking feedback that should not interrupt the current workflow.",
                "Short service-driven messages rendered from a single mounted `<Notification />` host."
            ],
            [
                "Field validation, long instructions, or content that must remain visible until the user acts.",
                "Confirmation, destructive, or decision-making flows where `DialogService` or an inline `Alert` is clearer."
            ],
            [
                "Default to `TopRight` for ordinary desktop product toasts because it stays near global chrome and avoids the main reading column.",
                "Use `TopCenter` only for global high-importance but still non-blocking status; if it requires a decision, use `DialogService` instead.",
                "Use `BottomCenter` when the page header or top navigation is dense, when mobile reach matters, or when the toast should not cover top actions.",
                "Use `BottomLeft`, `BottomRight`, `CenterLeft`, or `CenterRight` when feedback belongs near a side rail, list pane, or action region; keep the center of the working surface unobscured.",
                "Avoid central notification stacks for routine messages; reserve `Center` or center-side positions for urgent transient feedback that still does not need a modal.",
                "Keep notification copy short, keep the close affordance as the compact X control, and set `NotificationMessage.Position` or `Notify(..., position: ...)` when a message needs a different placement from the host default."
            ]),
        ["Tooltip"] = new(
            [
                "Short contextual help for icons, dense toolbar actions, small controls, and terms that need quick clarification.",
                "Service-triggered tooltip content rendered from the single mounted `<Tooltip />` host."
            ],
            [
                "Required instructions, validation errors, long explanatory copy, or content users need to compare while working.",
                "Replacing visible labels on controls that are not already obvious from context."
            ],
            [
                "Default to `Top` or `Right` when there is enough room, choosing the side that keeps the bubble out of the main reading path.",
                "Use `Bottom` for triggers close to the top edge and `Top` for triggers near the bottom edge, sticky footers, or lower toolbars.",
                "Use `Left` or `Right` for inline controls and icon clusters where vertical placement would cover neighboring rows.",
                "Use corner and edge placements such as `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight`, `LeftTop`, and `RightBottom` near viewport, card, toolbar, or panel corners so the tooltip stays visible and avoids adjacent controls.",
                "Prefer `TooltipTarget Position` for declarative hover and focus help; use `TooltipOptions.Position` when opening through `TooltipService` from custom pointer logic.",
                "Validate non-default placements with Playwright at the sandbox viewport sizes that match the target workflow, especially mobile or dense toolbar cases."
            ]),
        ["TooltipTarget"] = new(
            [
                "Declarative hover and focus help around a single trigger element.",
                "Icon-only or compact controls where the trigger should stay local to the markup."
            ],
            [
                "Complex help content that needs persistent reading or multiple interactions.",
                "Cases where visible helper text, a `HelpPopover`, or an inline `Alert` would be more discoverable."
            ],
            [
                "Pick the same `TooltipPosition` rules as the tooltip service: choose the side or corner that keeps the bubble inside the viewport and away from the next likely click target.",
                "Keep `Text` short; use `TooltipContent` only for compact rich content.",
                "Use `Delay` for crowded controls to avoid flicker, and keep `CloseOnMouseLeave` true for normal hover help."
            ]),
        ["ListDetailShell"] = new(
            [
                "Selection-driven workspaces with a stable list region and a detail pane.",
                "Review flows where the detail pane should stay focused while the selected record changes."
            ],
            [
                "Simple pages without a persistent selection model."
            ],
            [
                "When the detail pane starts stacking multiple dense regions, place `Tabs` inside the detail pane instead of growing one very tall view."
            ]),
        ["SectionHead"] = new(
            [
                "Section titles, lead copy, and compact header blocks above a panel."
            ],
            [
                "Decorative hero shells that should be handled by page-level layout components."
            ],
            [
                "Use `SectionHead` to keep heading rhythm consistent before adding panel-local typography markup.",
                "Let `SectionHead` own the section introduction so the panel body can focus on content."
            ]),
        ["StatsGrid"] = new(
            [
                "Dashboard-style metric rows and summary tiles."
            ],
            [
                "Arbitrary card mosaics with mixed content density."
            ],
            [
                "Use `StatsGrid` when the content is truly metric-first, not for supporting numbers above another primary task.",
                "Use `CompactStatStrip` or badges when metrics are contextual; if cards become multi-purpose sections, switch to `Grid` plus panel components."
            ]),
        ["OverlayWindow"] = new(
            [
                "Persistent floating inspectors, tool windows, and bounded workbench utilities that users may move, resize, minimize, or restore."
            ],
            [
                "A short confirmation or form that should focus the user; use `Dialog`.",
                "Generic page layout or a window without a concrete container boundary."
            ],
            [
                "Load `<OverlayLibHeadAssets />` and `<OverlayLibBodyAssets />` once, then use controlled `OverlayWindowState` plus `StateChanged` to persist state.",
                "Set `ContainerSelector` and `SafeTopSelector` for the consumer's actual host; CanvasLib already loads these assets when its generated asset components are present.",
                "Persist state within the consuming view's current lifetime. Component disposal releases browser resources and callback references; it does not authorize writes after the view retires.",
                "Disposal tolerates cancellation from the retiring JavaScript bridge. Errors from active initialization, updates, and consumer callbacks remain observable."
            ]),
        ["AccessibilityMirrorLayer"] = new(
            [
                "Assistive navigation and live announcements for CanvasLib workbench and calendar scenes."
            ],
            [
                "A second accessibility layer beside the mirror already rendered by `CanvasWorkbench` or `CanvasCalendar`."
            ],
            [
                "The owning canvas surface supplies the typed `AccessibilityMirrorSnapshot`; keep application state and persistence in that owner.",
                "Disposal retires the browser mirror once and tolerates teardown cancellation. Active create and update failures remain observable."
            ]),
        ["OverlayComponentToolbox"] = new(
            [
                "Searchable, grouped component choices in a workbench or overlay tool window."
            ],
            [
                "Primary application navigation or an unbounded generic catalog page."
            ],
            [
                "Provide typed sections, groups, and items. Keep consumer selection behavior in callbacks instead of changing the shared toolbox runtime."
            ]),
        ["QrCodeView"] = new(
            [
                "An inline generic QR payload with accessible labeling and an optional caption."
            ],
            [
                "Domain-specific address, ticket, transaction, or business-object UI semantics."
            ],
            [
                "Register QRCode services, pass typed rendering options when needed, and describe the payload context with `AriaLabel`."
            ]),
        ["QrCodeButton"] = new(
            [
                "A compact page-header or toolbar action that opens a generic typed QR payload in a dialog."
            ],
            [
                "Inline display when the QR code must remain visible; use `QrCodeView` there."
            ],
            [
                "Mount `DialogHost` once and keep the display label and payload semantics in the application."
            ]),
        ["QrScanButton"] = new(
            [
                "A standard QR-scanning action with camera support, manual fallback, typed result, and shared dialog behavior."
            ],
            [
                "A scanner flow that discards manual entry or assumes a camera is available."
            ],
            [
                "Handle `Scanned` and validate browser camera permission denial, unavailable camera hardware, and manual entry.",
                "Mount `DialogHost` once and keep domain acceptance rules outside the generic QR component library."
            ])
    };
    private static readonly IReadOnlyDictionary<string, ComponentGuidanceDocument> DefaultGuidanceByLibrary = new Dictionary<string, ComponentGuidanceDocument>(StringComparer.OrdinalIgnoreCase)
    {
        ["BaseLib"] = new(
            [
                "Shared app structure and interactive UI surfaces."
            ],
            [
                "Page-local structural CSS when the shared component parameters already express the intent."
            ],
            [
                "Prefer component parameters, variants, and layout primitives before custom structural classes.",
                "When content naturally breaks into modes or stages, use tabs, split layouts, or list-detail shells before stacking one long page.",
                "Use sandbox and product examples to mirror established composition patterns."
            ]),
        ["CanvasLib"] = new(
            [
                "Workbench, canvas, and runtime preview surfaces."
            ],
            [
                "Generic page shells that should stay in BaseLib."
            ],
            [
                "Keep runtime and preview surfaces aligned with the shared canvas contracts and token pack."
            ]),
        ["Charts"] = new(
            [
                "Operational charts that should stay behind the CanDoItAll wrapper while currently rendering through ApexCharts."
            ],
            [
                "Decorative chart chrome or direct use of raw Apex component markup in consuming pages."
            ],
            [
                "Use `AddCanDoItAllCharts()`, mount `<ChartsHeadAssets />` in the head, and mount `<ChartsBodyAssets />` once before Blazor starts.",
                "Set `ChartsBodyAssets IncludeThemeTokens=false` only when an earlier body-assets component already loads BaseLib's shared theme script; the chart theme bridge must still load.",
                "Build data with `CdaChartSeries`, `CdaChartPoint`, and `CdaChartOptions` so future renderer swaps stay localized.",
                "Validate chart routes with browser assertions and screenshots because build success cannot prove nonblank Apex SVG output. Check legends, axis labels, grid lines, and tooltips in both theme-switching directions on the mounted chart."
            ]),
        ["Mermaid"] = new(
            [
                "Mermaid diagrams that need official Mermaid.js rendering behind a CanDoItAll-owned Blazor wrapper.",
                "Flowcharts, architecture-beta diagrams, and documentation diagrams that need pan/zoom or node-click events."
            ],
            [
                "Direct page-level Mermaid.js imports or ad-hoc SVG injection in consuming pages.",
                "Diagram grammars that need custom layout semantics beyond Mermaid's parser and renderer."
            ],
            [
                "Use `AddCanDoItAllMermaid()` in hosts and render diagrams with `<MermaidDiagram Source=\"...\" />`.",
                "Use `NodeClicked`, `Rendered`, and `Error` callbacks for interaction and diagnostics rather than DOM-specific page scripts.",
                "Validate diagram routes in a browser because build success cannot prove Mermaid parsing, SVG output, pan/zoom, or node-click wiring."
            ]),
        ["OverlayLib"] = new(
            [
                "Generic floating tool windows and component toolboxes inside a bounded interactive surface."
            ],
            [
                "Ordinary dialogs, notifications, or page shells.",
                "Copying drag, resize, geometry normalization, or placement logic into consuming components."
            ],
            [
                "Load `<OverlayLibHeadAssets />` and `<OverlayLibBodyAssets />` once unless CanvasLib generated assets already load them.",
                "Use `OverlayWindowState` and the real container/safe-top selectors as the shared runtime boundary."
            ]),
        ["QRCode"] = new(
            [
                "Generic QR rendering, QR dialog actions, camera scanning, and manual decoded-text fallback."
            ],
            [
                "Domain-specific payload policy or a scanner flow that requires a camera without a fallback."
            ],
            [
                "Use `AddCanDoItAllQrCode()` after BaseLib registration and mount `DialogHost` before QR dialog actions.",
                "Keep payload classification, acceptance, and domain language in the consumer; validate browser permission and manual fallback behavior."
            ])
    };
    private static readonly IReadOnlySet<string> ConsumerProjectExclusions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "CanDoItAll.ComponentKit",
        "CanDoItAll.Components.BaseLib",
        "CanDoItAll.Components.CanvasLib",
        "CanDoItAll.Components.Charts",
        "CanDoItAll.Components.Common",
        "CanDoItAll.Components.Mermaid",
        "CanDoItAll.Components.OverlayLib",
        "CanDoItAll.Components.QRCode",
        "CanDoItAll.Components.Sandbox",
        "CanDoItAll.Mcp.Components",
        "CanDoItAll.Mcp.Core",
        "CanDoItAll.Mcp.DotNetWatch",
        "CanDoItAll.Mcp.LocalRuntime",
        "CanDoItAll.Mcp.SshOps"
    };
}
