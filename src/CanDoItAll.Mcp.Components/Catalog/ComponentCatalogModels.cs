namespace CanDoItAll.Mcp.Components.Catalog;

public sealed record ComponentCatalogIndex(
    IReadOnlyList<ComponentDocument> Components,
    IReadOnlyList<ComponentExampleDocument> Examples,
    IReadOnlyList<ComponentGroupDocument> Groups,
    IReadOnlyList<CanvasContractDocument> CanvasContracts,
    IReadOnlyDictionary<string, IReadOnlyList<ComponentUsageExampleDocument>> UsageExamplesByComponent);

public sealed record ComponentDocument(
    string Name,
    string FullName,
    string Namespace,
    string Library,
    string Summary,
    string SourcePath,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> GroupKeys,
    IReadOnlyList<string> DependencyNames,
    IReadOnlyList<ComponentParameterDocument> Parameters,
    IReadOnlyList<ComponentEventDocument> Events,
    IReadOnlyList<string> CssNotes,
    ComponentGuidanceDocument Guidance,
    int UsageExampleCount,
    IReadOnlyList<ComponentUsageExampleDocument> UsageExamples);

public sealed record ComponentGuidanceDocument(
    IReadOnlyList<string> UseFor,
    IReadOnlyList<string> AvoidFor,
    IReadOnlyList<string> CompositionRules);

public sealed record ComponentParameterDocument(
    string Name,
    string Type,
    bool Required,
    bool IsCascading,
    bool IsChildContent,
    string? Summary,
    string? DefaultValue,
    IReadOnlyList<string> AllowedValues);

public sealed record ComponentEventDocument(
    string Name,
    string Type,
    string? Summary);

public sealed record ComponentExampleDocument(
    string Id,
    string Title,
    string Route,
    string GroupKey,
    string GroupTitle,
    string Scenario,
    string Summary,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> ComponentNames);

public sealed record ComponentGroupDocument(
    string Key,
    string Title,
    string Route,
    string Summary,
    IReadOnlyList<string> FocusAreas,
    IReadOnlyList<string> ProofNotes,
    int ExampleCount);

public sealed record CanvasContractDocument(
    string Name,
    string FullName,
    string Kind,
    string Summary,
    IReadOnlyList<CanvasContractPropertyDocument> Properties);

public sealed record CanvasContractPropertyDocument(
    string Name,
    string Type);

public sealed record ComponentsSearchData(
    string Query,
    IReadOnlyList<ComponentSearchHit> Components,
    IReadOnlyList<ComponentExampleDocument> Examples,
    IReadOnlyList<ComponentGroupDocument> Groups);

public sealed record ComponentSearchHit(
    string Name,
    string Library,
    string Summary,
    double Score,
    IReadOnlyList<string> Groups,
    IReadOnlyList<string> MatchedParameters);

public sealed record ComponentExamplesData(
    string ComponentName,
    IReadOnlyList<ComponentExampleDocument> Examples);

public sealed record ComponentUsageExampleDocument(
    string SourceKind,
    string Project,
    string FilePath,
    int LineNumber,
    string Snippet,
    string? Route);

public sealed record ComponentUsageExamplesData(
    string ComponentName,
    int TotalMatches,
    IReadOnlyList<ComponentUsageExampleDocument> UsageExamples);

public sealed record ComponentCssTokensData(
    string ComponentName,
    string Library,
    IReadOnlyList<string> Stylesheets,
    IReadOnlyList<string> SourceFiles,
    IReadOnlyList<string> Notes);

public sealed record CanvasContractsData(
    string Query,
    IReadOnlyList<CanvasContractDocument> Contracts);

public sealed record ComponentLibraryDocument(
    string Name,
    string PackageId,
    string Purpose,
    bool ProductRuntimeLibrary,
    int ComponentCount,
    IReadOnlyList<string> Dependencies,
    IReadOnlyList<string> UseFor,
    IReadOnlyList<string> AvoidFor,
    IReadOnlyList<string> ServiceRegistrations,
    IReadOnlyList<string> HeadAssets,
    IReadOnlyList<string> BodyAssets,
    IReadOnlyList<string> Stylesheets,
    string? SandboxRoute,
    IReadOnlyList<string> Notes);

public sealed record AppShellGuideData(
    string Mode,
    IReadOnlyList<string> AvailableModes,
    IReadOnlyList<string> RequiredSetup,
    IReadOnlyList<string> ComponentRoles,
    string LayoutExample,
    string PageExample,
    IReadOnlyList<string> MenuRules,
    IReadOnlyList<string> LayoutRules,
    IReadOnlyList<string> ResponsiveRules,
    IReadOnlyList<string> ValidationChecklist);

public sealed record ComponentRecommendationData(
    string UseCase,
    string Constraints,
    IReadOnlyList<string> MatchedDecisionGuides,
    IReadOnlyList<ComponentRecommendationDocument> Recommendations,
    IReadOnlyList<ComponentRecommendationDocument> Alternatives,
    IReadOnlyList<string> RequiredSetup,
    IReadOnlyList<string> CompositionPlan,
    IReadOnlyList<string> FollowUpTools);

public sealed record ComponentRecommendationDocument(
    string Name,
    string Library,
    string Role,
    string Why,
    IReadOnlyList<string> UseFor,
    IReadOnlyList<string> AvoidFor,
    IReadOnlyList<string> CompositionRules,
    IReadOnlyList<string> Dependencies,
    double Score);
