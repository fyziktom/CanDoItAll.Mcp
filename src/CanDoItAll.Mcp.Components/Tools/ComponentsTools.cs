using System.ComponentModel;
using CanDoItAll.Mcp.Components.Catalog;
using CanDoItAll.Mcp.Core.Contracts;
using CanDoItAll.Mcp.Core.Hosting;
using CanDoItAll.Mcp.Core.Identity;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace CanDoItAll.Mcp.Components.Tools;

[McpServerToolType]
public sealed class ComponentsTools(
    ComponentCatalogService catalogService,
    IMcpIdleActivityTracker idleActivityTracker,
    ILogger<ComponentsTools> logger)
{
    [McpServerTool(Name = "components_search", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("Searches shared components, sandbox examples, and sandbox groups by component name, keyword, prop, or scenario.")]
    public Task<McpToolEnvelope<ComponentsSearchData>> ComponentsSearchAsync(
        string? query = null,
        string? library = null,
        string? group = null,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return ExecuteAsync(
            "components_search",
            () => catalogService.Search(query, library, group, limit));
    }

    [McpServerTool(Name = "component_get", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("Returns a shared component summary, namespace, semantic parameters and defaults, events, dependencies, composition guidance, and sandbox-linked metadata.")]
    public Task<McpToolEnvelope<ComponentDocument>> ComponentGetAsync(string component, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return ExecuteAsync("component_get", () => catalogService.GetComponent(component));
    }

    [McpServerTool(Name = "component_examples", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("Returns curated sandbox examples linked to a shared component.")]
    public Task<McpToolEnvelope<ComponentExamplesData>> ComponentExamplesAsync(string component, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return ExecuteAsync("component_examples", () => catalogService.GetExamples(component));
    }

    [McpServerTool(Name = "component_usage_examples", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("Returns bounded opening-tag usage examples from curated sandbox and consumer Razor files, prioritizing sandbox proof patterns over incidental product usage.")]
    public Task<McpToolEnvelope<ComponentUsageExamplesData>> ComponentUsageExamplesAsync(
        string component,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return ExecuteAsync("component_usage_examples", () => catalogService.GetUsageExamples(component, limit));
    }

    [McpServerTool(Name = "component_groups_list", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("Lists sandbox component groups with page routes, summaries, and proof notes.")]
    public Task<McpToolEnvelope<IReadOnlyList<ComponentGroupDocument>>> ComponentGroupsListAsync(CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return ExecuteAsync("component_groups_list", catalogService.GetGroups);
    }

    [McpServerTool(Name = "component_css_tokens_get", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("Returns stylesheet locations and CSS/token notes for a shared component.")]
    public Task<McpToolEnvelope<ComponentCssTokensData>> ComponentCssTokensGetAsync(string component, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return ExecuteAsync("component_css_tokens_get", () => catalogService.GetCssTokens(component));
    }

    [McpServerTool(Name = "canvas_contract_get", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("Returns typed canvas contract models, event args, requests, and state definitions from CanvasLib.")]
    public Task<McpToolEnvelope<CanvasContractsData>> CanvasContractGetAsync(string? query = null, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return ExecuteAsync("canvas_contract_get", () => catalogService.GetCanvasContracts(query));
    }

    [McpServerTool(Name = "components_libraries_list", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("Lists every supported non-WebGL Components library with its role, dependencies, setup, assets, styles, and sandbox proof route. Use this before adding a new Components package or hand-loading assets.")]
    public Task<McpToolEnvelope<IReadOnlyList<ComponentLibraryDocument>>> ComponentsLibrariesListAsync(CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return ExecuteAsync("components_libraries_list", catalogService.GetLibraries);
    }

    [McpServerTool(Name = "app_shell_guide_get", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("Returns the canonical BaseLib app-shell recipe for ThemeHost, Layout, SideMenu, Body, PageScaffold, overlay hosts, scrolling, route synchronization, overflow, and responsive behavior. Call this before creating or refactoring an application layout or primary menu.")]
    public Task<McpToolEnvelope<AppShellGuideData>> AppShellGuideGetAsync(
        string? mode = null,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return ExecuteAsync("app_shell_guide_get", () => catalogService.GetAppShellGuide(mode));
    }

    [McpServerTool(Name = "components_recommend", ReadOnly = true, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("Recommends the best non-WebGL shared components for a concrete use case, including compact page density, disclosure, metric hierarchy, card anatomy, form and dialog sizing. Returns required setup, per-component composition rules, an operational composition plan, alternatives, and exact follow-up MCP calls. Use this before choosing components or inventing custom UI structure.")]
    public Task<McpToolEnvelope<ComponentRecommendationData>> ComponentsRecommendAsync(
        string useCase,
        string? constraints = null,
        string? library = null,
        int limit = 6,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return ExecuteAsync(
            "components_recommend",
            () => catalogService.RecommendComponents(useCase, constraints, library, limit));
    }

    private Task<McpToolEnvelope<T>> ExecuteAsync<T>(string toolName, Func<T> callback)
    {
        var correlationId = CorrelationIdFactory.Create();
        using var activity = idleActivityTracker.BeginOperation();

        try
        {
            var data = callback();
            return Task.FromResult(McpToolEnvelope<T>.Success(toolName, correlationId, data));
        }
        catch (ToolInvocationException ex)
        {
            logger.LogWarning(ex, "{ToolName} failed with a deterministic tool error {Code}.", toolName, ex.Code);
            return Task.FromResult(McpToolEnvelope<T>.Failure(toolName, correlationId, new ToolError(ex.Code, ex.Message, ex.Details), status: "failed", summary: ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{ToolName} failed unexpectedly.", toolName);
            return Task.FromResult(McpToolEnvelope<T>.Failure(toolName, correlationId, new ToolError("InternalError", ex.Message), status: "failed", summary: "The tool failed unexpectedly."));
        }
    }
}
