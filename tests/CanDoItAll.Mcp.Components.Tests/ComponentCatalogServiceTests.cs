using CanDoItAll.Components.Charts;
using CanDoItAll.Mcp.Components.Catalog;
using CanDoItAll.Mcp.Components.Configuration;
using CanDoItAll.Mcp.Core.Contracts;
using Microsoft.Extensions.Options;

namespace CanDoItAll.Mcp.Components.Tests;

public sealed class ComponentCatalogServiceTests
{
    [Fact]
    public void Validator_Accepts_Repository_Component_Roots()
    {
        var options = CreateOptions();
        var validator = new McpServerOptionsValidator();

        var result = validator.Validate(name: null, options.Value);

        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Failures ?? []));
    }

    [Fact]
    public void Search_Finds_Button_Component_By_Name()
    {
        var service = CreateService();

        var result = service.Search("button");

        Assert.Contains(result.Components, component => string.Equals(component.Name, "Button", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Search_Finds_Tab_Sandbox_Examples_By_Scenario()
    {
        var service = CreateService();

        var result = service.Search("tabs");

        Assert.Contains(result.Examples, example => example.Scenario.Contains("Tabs", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Search_Finds_Tabs_When_Querying_Long_Scroll_Relief()
    {
        var service = CreateService();

        var result = service.Search("long scroll");

        Assert.Contains(result.Components, component => string.Equals(component.Name, "Tabs", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Component_Get_Returns_Workbench_Surface_Parameter_And_Examples()
    {
        var service = CreateService();

        var component = service.GetComponent("CanvasWorkbench");
        var examples = service.GetExamples("CanvasWorkbench");
        var usage = service.GetUsageExamples("CanvasWorkbench");

        Assert.Equal("CanvasLib", component.Library);
        Assert.Contains(component.Parameters, parameter => string.Equals(parameter.Name, "Surface", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(examples.Examples, example => string.Equals(example.GroupKey, "canvas", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(component.Guidance.CompositionRules, rule => rule.Contains("ToolbarActions", StringComparison.Ordinal));
        Assert.Contains(component.Guidance.CompositionRules, rule => rule.Contains("Ctrl+I", StringComparison.Ordinal) && rule.Contains("CopyIdentifier", StringComparison.Ordinal));
        Assert.Contains(component.Guidance.CompositionRules, rule => rule.Contains("ContextActionRequested", StringComparison.Ordinal));
        Assert.Contains(component.Guidance.CompositionRules, rule => rule.Contains("/canvas#canvas-workbench", StringComparison.Ordinal));
        Assert.Contains(usage.UsageExamples, example => example.FilePath.EndsWith(Path.Combine("Examples", "Canvas", "WorkbenchWithFloatingWindows.razor"), StringComparison.OrdinalIgnoreCase)
            && example.Snippet.Contains("ContextActionRequested", StringComparison.Ordinal));
    }

    [Fact]
    public void Css_Tokens_Return_Canvas_Stylesheet_Notes()
    {
        var service = CreateService();

        var result = service.GetCssTokens("CanvasWorkbench");

        Assert.Contains(result.Stylesheets, stylesheet => stylesheet.Contains("css/workbench/", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.SourceFiles, file => file.EndsWith(Path.Combine("wwwroot", "css", "workbench", "shell", "01-layout-and-shell.css"), StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Notes, note => note.Contains("CanvasThemeTokenPack", StringComparison.OrdinalIgnoreCase));

        var dialog = service.GetCssTokens("CanvasOverlayDialog");
        var examples = service.GetExamples("CanvasOverlayDialog");

        Assert.Contains(dialog.Notes, note => note.Contains("--ui-overlay-", StringComparison.Ordinal));
        Assert.Contains(dialog.Stylesheets, stylesheet => stylesheet.EndsWith("CanDoItAll.Components.CanvasLib.bundle.scp.css", StringComparison.Ordinal));
        Assert.Contains(examples.Examples, example => example.Id == "modals-canvas-overlay" && example.Route == "/modals#canvas-overlay-dialog");
    }

    [Fact]
    public void Canvas_Contracts_Return_Workbench_Surface_With_Nodes_Property()
    {
        var service = CreateService();

        var result = service.GetCanvasContracts("CanvasWorkbenchSurface");

        var contract = Assert.Single(result.Contracts, item => string.Equals(item.Name, "CanvasWorkbenchSurface", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(contract.Properties, property => string.Equals(property.Name, "Nodes", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Canvas_Contracts_Expose_Toolbar_Actions_And_Typed_Command_Values() {
        var service = CreateService();

        var nodes = service.GetCanvasContracts("CanvasWorkbenchNode");
        var node = Assert.Single(nodes.Contracts, item => item.Name == "CanvasWorkbenchNode");
        var toolbar = Assert.Single(node.Properties, property => property.Name == "ToolbarActions");
        Assert.Equal("List<CanvasWorkbenchAction>", toolbar.Type);
        Assert.Empty(toolbar.AllowedValues);

        var commands = service.GetCanvasContracts("CanvasWorkbenchKeyboardCommand");
        var action = Assert.Single(commands.Contracts, item => item.Name == "CanvasWorkbenchAction");
        var command = Assert.Single(action.Properties, property => property.Name == "KeyboardCommand");
        Assert.Equal("CanvasWorkbenchKeyboardCommand", command.Type);
        Assert.Equal(["CopyIdentifier", "None"], command.AllowedValues);

        var clipboardContracts = service.GetCanvasContracts("CanvasWorkbenchClipboardRequest");
        var clipboard = Assert.Single(clipboardContracts.Contracts, item => item.Name == "CanvasWorkbenchClipboardRequest");
        var clipboardAction = Assert.Single(clipboard.Properties, property => property.Name == "Action");
        Assert.Equal(["Copy", "Cut", "Duplicate", "Paste"], clipboardAction.AllowedValues);
    }

    [Fact]
    public void Groups_List_Returns_All_Required_Sandbox_Groups()
    {
        var service = CreateService();

        var groups = service.GetGroups();

        Assert.NotEmpty(groups);
        Assert.Contains(groups, group => string.Equals(group.Key, "charts", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(groups, group => string.Equals(group.Key, "mermaid", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(groups, group => string.Equals(group.Key, "canvas", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(groups, group => string.Equals(group.Key, "layout", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(groups, group => string.Equals(group.Key, "qr", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Component_Get_Returns_Mermaid_Metadata_Examples_And_Css()
    {
        var service = CreateService();

        var component = service.GetComponent("MermaidDiagram");
        var examples = service.GetExamples("MermaidDiagram");
        var css = service.GetCssTokens("MermaidDiagram");

        Assert.Equal("Mermaid", component.Library);
        Assert.Equal("CanDoItAll.Components.Mermaid", component.Namespace);
        Assert.EndsWith(Path.Combine("Components", "MermaidDiagram.razor"), component.SourcePath, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(component.Tags, tag => string.Equals(tag, "mermaid", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(component.GroupKeys, key => string.Equals(key, "mermaid", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(component.Guidance.CompositionRules, rule => rule.Contains("AddCanDoItAllMermaid", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(component.Parameters, parameter => string.Equals(parameter.Name, "Source", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(component.Events, @event => string.Equals(@event.Name, "NodeClicked", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(examples.Examples, example => string.Equals(example.GroupKey, "mermaid", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(css.Stylesheets, stylesheet => stylesheet.Contains("CanDoItAll.Components.Mermaid", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Component_Get_Returns_Charts_Metadata_Examples_And_Css()
    {
        var service = CreateService();

        var component = service.GetComponent("CdaChart");
        var examples = service.GetExamples("CdaChart");
        var css = service.GetCssTokens("CdaChart");
        var library = Assert.Single(service.GetLibraries(), library => library.Name == "Charts");
        var recommendation = service.RecommendComponents("operational line chart");

        Assert.Contains(library.HeadAssets, asset => asset.Contains(nameof(ChartsHeadAssets), StringComparison.Ordinal));
        Assert.Contains(library.BodyAssets, asset => asset.Contains(nameof(ChartsBodyAssets), StringComparison.Ordinal));
        Assert.Contains(recommendation.Recommendations, item => item.Name == nameof(ChartsBodyAssets));

        Assert.Equal("Charts", component.Library);
        Assert.Equal("CanDoItAll.Components.Charts", component.Namespace);
        Assert.EndsWith(Path.Combine("Components", "CdaChart.razor"), component.SourcePath, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(component.Tags, tag => string.Equals(tag, "chart-wrapper", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(component.GroupKeys, key => string.Equals(key, "charts", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(component.CssNotes, note => note.Contains("raw Apex component markup", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(component.Guidance.CompositionRules, rule => rule.Contains("AddCanDoItAllCharts", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(component.Guidance.CompositionRules, rule => rule.Contains(nameof(ChartsBodyAssets), StringComparison.Ordinal));
        Assert.Contains(component.Guidance.CompositionRules, rule => rule.Contains("browser assertions", StringComparison.OrdinalIgnoreCase));

        var seriesParameter = Assert.Single(component.Parameters, parameter => string.Equals(parameter.Name, "Series", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("CdaChartSeries", seriesParameter.Summary ?? string.Empty, StringComparison.OrdinalIgnoreCase);

        Assert.Contains(examples.Examples, example => string.Equals(example.GroupKey, "charts", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(css.Stylesheets, stylesheet => stylesheet.Contains("Blazor-ApexCharts", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Component_Get_Returns_Richer_Tabs_Metadata_And_Sandbox_Examples()
    {
        var service = CreateService();

        var component = service.GetComponent("Tabs");
        var usageExamples = service.GetUsageExamples("Tabs", limit: 20);
        var css = service.GetCssTokens("Tabs");

        Assert.Equal("BaseLib", component.Library);
        Assert.EndsWith(Path.Combine("Components", "Navigation", "Tabs.razor"), component.SourcePath, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(component.DependencyNames, dependency => string.Equals(dependency, "TabsItem", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(component.CssNotes, note => note.Contains("Tailwind-owned", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(component.Guidance.CompositionRules, rule => rule.Contains("SectionCard", StringComparison.OrdinalIgnoreCase));

        var toneParameter = Assert.Single(component.Parameters, parameter => string.Equals(parameter.Name, "Tone", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("Primary", toneParameter.DefaultValue);
        Assert.Contains("Info", toneParameter.AllowedValues);
        Assert.Contains("accent", toneParameter.Summary ?? string.Empty, StringComparison.OrdinalIgnoreCase);

        var classParameter = Assert.Single(component.Parameters, parameter => string.Equals(parameter.Name, "Class", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("Tailwind", classParameter.Summary ?? string.Empty, StringComparison.OrdinalIgnoreCase);

        Assert.Contains(css.SourceFiles, file => file.EndsWith(Path.Combine("Tailwind", "navigation", "tabs.css"), StringComparison.OrdinalIgnoreCase));
        Assert.True(usageExamples.TotalMatches >= 2);
        Assert.Contains(usageExamples.UsageExamples, example => example.FilePath.EndsWith("BasicTabs.razor", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Refactored_Controls_Expose_Current_Contracts_Without_Removed_Compatibility_Components() {
        var service = CreateService();
        var tabs = service.GetComponent("Tabs");
        var button = service.GetComponent("Button");
        var select = service.GetComponent("SelectInput");
        var textArea = service.GetComponent("TextAreaInput");
        var components = service.GetIndex().Components;

        var density = Assert.Single(tabs.Parameters, parameter => parameter.Name == "Density");
        Assert.Equal("Normal", density.DefaultValue);
        Assert.Equal(new[] { "Comfortable", "Compact", "Normal" }, density.AllowedValues);
        Assert.Contains(tabs.Parameters, parameter => parameter.Name == "Rounded");
        Assert.Contains(tabs.Parameters, parameter => parameter.Name == "NoContent");
        Assert.Contains(tabs.Parameters, parameter => parameter.Name == "PanelOverflowMode");
        Assert.DoesNotContain(tabs.Parameters, parameter => parameter.Name == "Variant");
        Assert.Contains(button.Parameters, parameter => parameter.Name == "Tone" && parameter.AllowedValues.Contains("Primary"));
        Assert.DoesNotContain(button.Parameters, parameter => parameter.Name == "ButtonStyle");
        Assert.Contains(select.Parameters, parameter => parameter.Name == "ChildContent");
        Assert.Contains(select.Parameters, parameter => parameter.Name == "ValueExpression");
        Assert.Contains(textArea.Parameters, parameter => parameter.Name == "Size" && parameter.AllowedValues.Contains("Extended"));
        Assert.DoesNotContain(components, component => component.Name is "TextBox" or "TextArea" or "Numeric" or "DropDown" or "ProfileField");
    }

    [Fact]
    public void Avatar_Recommendation_Preserves_Human_Default_And_Offers_Robot_As_An_Alternative() {
        var service = CreateService();
        var recommendation = service.RecommendComponents("agent colleague avatar with human portrait and optional robot identity");
        var avatar = service.GetComponent("Avatar");
        var robot = service.GetComponent("RoboAvatar");

        Assert.Equal("Avatar", recommendation.Recommendations[0].Name);
        Assert.Contains(recommendation.Alternatives, component => component.Name == "RoboAvatar");
        Assert.Contains(avatar.Parameters, parameter => parameter.Name == "UseDefaultImageWhenMissing" && parameter.DefaultValue == "true");
        Assert.Contains(avatar.Guidance.CompositionRules, rule => rule.Contains("human portrait as the default", StringComparison.Ordinal));
        Assert.Contains(robot.Guidance.AvoidFor, rule => rule.Contains("primary default", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("ThemeHost", "theme.css")]
    [InlineData("Button", "button.css")]
    [InlineData("TextAreaInput", "form-controls.css")]
    [InlineData("Dialog", "dialog.css")]
    [InlineData("CanvasOverlayDialog", "CanvasOverlayDialog.razor.css")]
    public void Css_Tokens_Resolve_Organized_Source_Files(string component, string expectedFile) {
        var service = CreateService();

        var css = service.GetCssTokens(component);

        Assert.Contains(css.SourceFiles, file => Path.GetFileName(file) == expectedFile);
        Assert.All(css.SourceFiles, file => Assert.True(File.Exists(file)));
    }

    [Fact]
    public void Component_Get_Returns_Editable_Metadata_And_Sandbox_Example()
    {
        var service = CreateService();

        var component = service.GetComponent("Editable");
        var examples = service.GetExamples("Editable");

        Assert.Equal("BaseLib", component.Library);
        Assert.EndsWith(Path.Combine("Components", "Forms", "Editable.razor"), component.SourcePath, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(component.Parameters, parameter => string.Equals(parameter.Name, "ParameterName", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(component.Events, @event => string.Equals(@event.Name, "ItemChanged", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(examples.Examples, example => string.Equals(example.Id, "forms-editable", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Component_Get_Returns_Overlay_Position_Guidance_For_Tooltip_And_Notification()
    {
        var service = CreateService();

        var notification = service.GetComponent("Notification");
        var tooltip = service.GetComponent("Tooltip");
        var tooltipTarget = service.GetComponent("TooltipTarget");

        Assert.Contains("per-message viewport positioning", notification.Summary, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(notification.Tags, tag => string.Equals(tag, "notification-position", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(notification.CssNotes, note => note.Contains("NotificationMessage.Position", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(notification.Guidance.CompositionRules, rule => rule.Contains("TopRight", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(notification.Guidance.CompositionRules, rule => rule.Contains("BottomCenter", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(notification.Guidance.CompositionRules, rule => rule.Contains("DialogService", StringComparison.OrdinalIgnoreCase));

        var notificationPosition = Assert.Single(notification.Parameters, parameter => string.Equals(parameter.Name, "Position", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("BottomCenter", notificationPosition.AllowedValues);
        Assert.Contains("per-message", notificationPosition.Summary ?? string.Empty, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("side, corner, and edge-aligned placement", tooltip.Summary, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(tooltip.CssNotes, note => note.Contains("TooltipOptions.Position", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(tooltip.Guidance.CompositionRules, rule => rule.Contains("TopLeft", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(tooltip.Guidance.CompositionRules, rule => rule.Contains("Playwright", StringComparison.OrdinalIgnoreCase));

        var tooltipTargetPosition = Assert.Single(tooltipTarget.Parameters, parameter => string.Equals(parameter.Name, "Position", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("RightBottom", tooltipTargetPosition.AllowedValues);
        Assert.Contains("viewport", tooltipTargetPosition.Summary ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Component_Get_Returns_Side_Menu_Metadata_Usage_And_Css()
    {
        var service = CreateService();

        var component = service.GetComponent("SideMenu");
        var usage = service.GetUsageExamples("SideMenu", limit: 20);
        var css = service.GetCssTokens("SideMenu");

        Assert.Equal("BaseLib", component.Library);
        Assert.EndsWith(Path.Combine("Components", "Navigation", "SideMenu.razor"), component.SourcePath, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(component.Parameters, parameter => string.Equals(parameter.Name, "MenuId", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(component.Parameters, parameter => string.Equals(parameter.Name, "MoreItems", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(component.Guidance.CompositionRules, rule => rule.Contains("SideMenuService", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(component.Guidance.CompositionRules, rule => rule.Contains("small breakpoint", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(usage.UsageExamples, example => example.FilePath.EndsWith("SideMenuWorkspace.razor", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(css.SourceFiles, file => file.EndsWith(Path.Combine("Tailwind", "navigation", "side-menu.css"), StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Component_Get_Explains_Compact_Page_Form_Dialog_And_Card_Semantics()
    {
        var service = CreateService();

        var pageHeader = service.GetComponent("PageHeader");
        var textArea = service.GetComponent("TextAreaInput");
        var dialog = service.GetComponent("Dialog");
        var card = service.GetComponent("Card");
        var selectionListItem = service.GetComponent("SelectionListItem");

        var compactParameter = Assert.Single(pageHeader.Parameters, parameter => string.Equals(parameter.Name, "Compact", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("operational-page", compactParameter.Summary ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(pageHeader.Guidance.CompositionRules, rule => rule.Contains("CompactStatStrip", StringComparison.OrdinalIgnoreCase));

        var rowsParameter = Assert.Single(textArea.Parameters, parameter => string.Equals(parameter.Name, "Rows", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("override", rowsParameter.Summary ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(textArea.Guidance.CompositionRules, rule => rule.Contains("TextAreaSize.Compact", StringComparison.OrdinalIgnoreCase));

        var sizeParameter = Assert.Single(dialog.Parameters, parameter => string.Equals(parameter.Name, "Size", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("single-column", sizeParameter.Summary ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(dialog.Guidance.CompositionRules, rule => rule.Contains("ModalSize.Compact", StringComparison.OrdinalIgnoreCase));

        Assert.Contains(card.Guidance.CompositionRules, rule => rule.Contains("Media", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(card.Guidance.CompositionRules, rule => rule.Contains("Actions", StringComparison.OrdinalIgnoreCase));

        var onSelectEvent = Assert.Single(selectionListItem.Events, @event => string.Equals(@event.Name, "OnSelect", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("noninteractive surface", onSelectEvent.Summary ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(selectionListItem.Guidance.CompositionRules, rule => rule.Contains("renders as a selection button", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(selectionListItem.Guidance.CompositionRules, rule => rule.Contains("does not disable", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(selectionListItem.Guidance.CompositionRules, rule => rule.Contains("consumer-owned", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Compound_Form_Controls_Require_Container_Width_Wrapping_And_Narrow_Column_Proof()
    {
        var service = CreateService();

        var secretField = service.GetComponent("SecretField");
        var recommendation = service.RecommendComponents("three-column form with compound secret fields");

        Assert.Contains(secretField.CssNotes, note => note.Contains("containing width", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(secretField.Guidance.CompositionRules, rule => rule.Contains("not only viewport", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(secretField.Guidance.CompositionRules, rule => rule.Contains("300–320px", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(secretField.Guidance.CompositionRules, rule => rule.Contains("horizontal overflow", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(recommendation.CompositionPlan, step => step.Contains("actual containing width", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(recommendation.CompositionPlan, step => step.Contains("300–320px", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Usage_Examples_Prioritize_Curated_Sandbox_And_Capture_Multiline_Opening_Tag()
    {
        var workspaceRoot = Path.Combine(Path.GetTempPath(), $"candoitall-components-usage-{Guid.NewGuid():N}");
        var productRoot = Path.Combine(workspaceRoot, "src", "CanDoItAll.Web");
        var sandboxRoot = Path.Combine(workspaceRoot, "sandbox");
        Directory.CreateDirectory(productRoot);
        Directory.CreateDirectory(sandboxRoot);

        File.WriteAllText(
            Path.Combine(productRoot, "ProductPage.razor"),
            "<TextAreaInput Rows=\"2\" />");
        File.WriteAllText(
            Path.Combine(sandboxRoot, "CompactTextAreaProof.razor"),
            """
            @page "/proof/compact-textarea"

            <TextAreaInput Value="@notes"
                      ValueChanged="HandleNotesChanged"
                      Size="TextAreaSize.Extended" />
            """);

        try
        {
            var service = new ComponentCatalogService(Options.Create(new McpServerOptions
            {
                Server = new ServerOptions
                {
                    WorkspaceRoot = workspaceRoot
                },
                Catalog = new CatalogOptions
                {
                    CommonRoot = "missing/common",
                    BaseLibRoot = "missing/base",
                    CanvasLibRoot = "missing/canvas",
                    ChartsRoot = "missing/charts",
                    MermaidRoot = "missing/mermaid",
                    OverlayLibRoot = "missing/overlay",
                    QrCodeRoot = "missing/qr",
                    SandboxRoot = Path.GetRelativePath(workspaceRoot, sandboxRoot)
                }
            }));

            var usage = service.GetUsageExamples("TextAreaInput", limit: 10);
            var first = Assert.IsType<ComponentUsageExampleDocument>(usage.UsageExamples[0]);

            Assert.Equal("sandbox", first.SourceKind);
            Assert.Equal("/proof/compact-textarea", first.Route);
            Assert.StartsWith("<TextAreaInput", first.Snippet, StringComparison.Ordinal);
            Assert.Contains("ValueChanged=", first.Snippet, StringComparison.Ordinal);
            Assert.Contains("Size=\"TextAreaSize.Extended\"", first.Snippet, StringComparison.Ordinal);
            Assert.EndsWith("/>", first.Snippet, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(workspaceRoot, recursive: true);
        }
    }

    [Fact]
    public void Catalog_Includes_Overlay_And_QrCode_Without_WebGl()
    {
        var service = CreateService();

        var overlay = service.GetComponent("OverlayWindow");
        var qrCode = service.GetComponent("QrCodeView");
        var qrScanner = service.GetComponent("QrScanButton");
        var components = service.GetIndex().Components;

        Assert.Equal("OverlayLib", overlay.Library);
        Assert.Contains(overlay.Parameters, parameter => string.Equals(parameter.Name, "ContainerSelector", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("QRCode", qrCode.Library);
        Assert.Contains(qrCode.Parameters, parameter => string.Equals(parameter.Name, "Payload", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("QRCode", qrScanner.Library);
        Assert.DoesNotContain(components, component => component.Library.Contains("WebGl", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Libraries_List_Explains_Setup_And_Excludes_WebGl()
    {
        var service = CreateService();

        var libraries = service.GetLibraries();
        var baseLib = Assert.Single(libraries, library => string.Equals(library.Name, "BaseLib", StringComparison.OrdinalIgnoreCase));
        var overlay = Assert.Single(libraries, library => string.Equals(library.Name, "OverlayLib", StringComparison.OrdinalIgnoreCase));
        var canvas = Assert.Single(libraries, library => string.Equals(library.Name, "CanvasLib", StringComparison.OrdinalIgnoreCase));
        var qrCode = Assert.Single(libraries, library => string.Equals(library.Name, "QRCode", StringComparison.OrdinalIgnoreCase));

        Assert.Equal(8, libraries.Count);
        Assert.True(baseLib.ComponentCount > 0);
        Assert.Contains(baseLib.Stylesheets, stylesheet => stylesheet.EndsWith("material-symbols.css", StringComparison.Ordinal));
        Assert.Contains(baseLib.ServiceRegistrations, registration => registration.Contains("AddCanDoItAllBaseLib", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(overlay.BodyAssets, asset => asset.Contains("OverlayLibBodyAssets", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(canvas.Stylesheets, stylesheet => stylesheet.EndsWith("CanDoItAll.Components.CanvasLib.bundle.scp.css", StringComparison.Ordinal));
        Assert.Contains(canvas.Notes, note => note.Contains("scoped CSS bundle", StringComparison.Ordinal));
        Assert.Equal("/qr", qrCode.SandboxRoute);
        Assert.Contains(qrCode.Notes, note => note.Contains("DialogHost", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(libraries, library => library.Name.Contains("WebGl", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void App_Shell_Guide_Contains_The_Canonical_Menu_And_Scroll_Model()
    {
        var service = CreateService();

        var guide = service.GetAppShellGuide("full-height");

        Assert.Equal("full-height", guide.Mode);
        Assert.Contains("SideMenu", guide.LayoutExample, StringComparison.Ordinal);
        Assert.Contains("DialogHost", guide.LayoutExample, StringComparison.Ordinal);
        Assert.Contains(guide.MenuRules, rule => rule.Contains("ItemSelected", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(guide.LayoutRules, rule => rule.Contains("one vertical scroll owner", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(guide.LayoutRules, rule => rule.Contains("opened-workspace tab strip", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(guide.ComponentRoles, role => role.Contains("IPageHeaderOutlet", StringComparison.Ordinal));
        Assert.Contains(guide.ResponsiveRules, rule => rule.Contains("768px", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("responsive application layout with side navigation", "SideMenu")]
    [InlineData("scan a QR code with camera fallback", "QrScanButton")]
    [InlineData("a floating resizable inspector window", "OverlayWindow")]
    [InlineData("multi step sequential wizard", "Steps")]
    public void Recommendations_Return_The_Right_Component_For_The_Use_Case(string useCase, string expectedComponent)
    {
        var service = CreateService();

        var recommendation = service.RecommendComponents(useCase);

        Assert.Contains(recommendation.Recommendations, component => string.Equals(component.Name, expectedComponent, StringComparison.OrdinalIgnoreCase));
        Assert.NotEmpty(recommendation.FollowUpTools);
    }

    [Fact]
    public void Compact_Recommendations_Return_Retained_Component_Rules_And_Task_First_Plans()
    {
        var service = CreateService();

        var compactPage = service.RecommendComponents("compact operational page with supporting metrics above an item list");
        var itemCrud = service.RecommendComponents("CRUD item list with create and edit dialog");
        var visualCards = service.RecommendComponents("visual card collection with media tags and item actions");
        var defaultBaseLib = service.RecommendComponents("button", library: "BaseLib");

        var pageHeader = Assert.Single(compactPage.Recommendations, component => string.Equals(component.Name, "PageHeader", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(pageHeader.CompositionRules, rule => rule.Contains("CompactStatStrip", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(compactPage.MatchedDecisionGuides, guide => guide.Contains("Compact task-first", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(compactPage.CompositionPlan, step => step.Contains("primary task", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(compactPage.Recommendations, component => string.Equals(component.Name, "CompactStatStrip", StringComparison.OrdinalIgnoreCase));

        var dialog = Assert.Single(itemCrud.Recommendations, component => string.Equals(component.Name, "Dialog", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(dialog.CompositionRules, rule => rule.Contains("ModalSize.Compact", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(itemCrud.CompositionPlan, step => step.Contains("load dialog-only data", StringComparison.OrdinalIgnoreCase));

        var card = Assert.Single(visualCards.Recommendations, component => string.Equals(component.Name, "Card", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(card.CompositionRules, rule => rule.Contains("Actions", StringComparison.OrdinalIgnoreCase));

        Assert.NotEmpty(defaultBaseLib.CompositionPlan);
    }

    [Fact]
    public void All_Discovered_Components_Resolve_To_Existing_Source_Paths()
    {
        var service = CreateService();
        var components = service.GetIndex().Components;

        Assert.All(components, component => Assert.True(File.Exists(component.SourcePath), $"Missing source path for {component.Name}: {component.SourcePath}"));
    }

    [Fact]
    public void Component_Summaries_Avoid_Generic_And_Sandbox_Derived_Fallbacks()
    {
        var service = CreateService();
        var components = service.GetIndex().Components;

        Assert.DoesNotContain(components, component => component.Summary.Contains("extracted component libraries", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(components, component => component.Summary.Contains("used in the sandbox", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Feedback_Component_Summaries_Describe_Their_Actual_Role()
    {
        var service = CreateService();

        var component = service.GetComponent("EmptyState");

        Assert.Contains("zero-data", component.Summary, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Actions group", component.Summary, StringComparison.OrdinalIgnoreCase);
    }

    private static ComponentCatalogService CreateService()
    {
        return new ComponentCatalogService(CreateOptions());
    }

    private static IOptions<McpServerOptions> CreateOptions()
    {
        var workspaceRoot = FindWorkspaceRoot();
        return Options.Create(new McpServerOptions
        {
            Server = new ServerOptions
            {
                WorkspaceRoot = workspaceRoot
            },
            Catalog = new CatalogOptions
            {
                CommonRoot = Path.Combine("..", "CanDoItAll.Components", "src", "CanDoItAll.Components.Common"),
                BaseLibRoot = Path.Combine("..", "CanDoItAll.Components", "src", "CanDoItAll.Components.BaseLib"),
                CanvasLibRoot = Path.Combine("..", "CanDoItAll.Components", "src", "CanDoItAll.Components.CanvasLib"),
                ChartsRoot = Path.Combine("..", "CanDoItAll.Components", "src", "CanDoItAll.Components.Charts"),
                MermaidRoot = Path.Combine("..", "CanDoItAll.Components", "src", "CanDoItAll.Components.Mermaid"),
                OverlayLibRoot = Path.Combine("..", "CanDoItAll.Components", "src", "CanDoItAll.Components.OverlayLib"),
                QrCodeRoot = Path.Combine("..", "CanDoItAll.Components", "src", "CanDoItAll.Components.QRCode"),
                SandboxRoot = Path.Combine("..", "CanDoItAll.Components", "samples", "CanDoItAll.Components.Sandbox")
            }
        });
    }

    private static string FindWorkspaceRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "CanDoItAll.Mcp.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new ToolInvocationException("WorkspaceNotFound", "Could not locate the CanDoItAll.Mcp workspace root for MCP component tests.");
    }
}
