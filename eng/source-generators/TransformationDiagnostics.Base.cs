using Microsoft.CodeAnalysis;
using static PrincipleStudios.OpenApiCodegen.CommonDiagnostics;

namespace PrincipleStudios.OpenApiCodegen;

public partial class TransformationDiagnostics
{
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.InvalidRetrievalUri")]
	public static readonly DiagnosticDescriptor InvalidRetrievalUriDiagnostic =
		new DiagnosticDescriptor(id: "PS_PARSE_002",
								title: "An invalid URI was provided to retrieve a document",
								messageFormat: PrincipleStudios_OpenApi_Transformations_InvalidRetrievalUri,
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.InvalidDocumentBaseUri")]
	public static readonly DiagnosticDescriptor InvalidDocumentBaseUriDiagostic =
		new DiagnosticDescriptor(id: "PS_PARSE_004",
								title: "An invalid base URI was provided by a document",
								messageFormat: PrincipleStudios_OpenApi_Transformations_InvalidDocumentBaseUri,
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.InvalidFragmentDiagnostic")]
	public static readonly DiagnosticDescriptor InvalidFragmentDiagostic =
		new DiagnosticDescriptor(id: "PS_PARSE_005",
								title: "The fragment provided was not a valid JSON pointer",
								messageFormat: PrincipleStudios_OpenApi_Transformations_InvalidFragmentDiagnostic,
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.InvalidRefDiagnostic")]
	public static readonly DiagnosticDescriptor InvalidRefDiagnostic =
		new DiagnosticDescriptor(id: "PS_PARSE_006",
								title: "Invalid URI provided for ref",
								messageFormat: PrincipleStudios_OpenApi_Transformations_InvalidRefDiagnostic,
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.CouldNotFindTargetNodeDiagnostic")]
	public static readonly DiagnosticDescriptor CouldNotFindTargetNodeDiagnostic =
		new DiagnosticDescriptor(id: "PS_PARSE_007",
								title: "Target node did not exist in given document",
								messageFormat: PrincipleStudios_OpenApi_Transformations_CouldNotFindTargetNodeDiagnostic,
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.UnknownAnchorDiagnostic")]
	public static readonly DiagnosticDescriptor UnknownAnchorDiagnostic =
		new DiagnosticDescriptor(id: "PS_PARSE_008",
								title: "Unknown anchor provided in ref",
								messageFormat: PrincipleStudios_OpenApi_Transformations_UnknownAnchorDiagnostic,
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.ResolveDocumentDiagnostic")]
	public static readonly DiagnosticDescriptor ResolveDocumentDiagnostic =
		new DiagnosticDescriptor(id: "PS_PARSE_009",
								title: "Could not retrieve the specified document",
								messageFormat: PrincipleStudios_OpenApi_Transformations_ResolveDocumentDiagnostic,
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
}
