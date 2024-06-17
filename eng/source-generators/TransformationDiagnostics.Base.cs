using Microsoft.CodeAnalysis;

namespace PrincipleStudios.OpenApiCodegen;

public partial class TransformationDiagnostics
{
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.InvalidRetrievalUri")]
	public static readonly DiagnosticDescriptor InvalidRetrievalUriDiagnostic =
		new DiagnosticDescriptor(id: "PS_PARSE_002",
								title: "An invalid URI was provided to retrieve a document",
								messageFormat: "An invalid URI was provided to retrieve a document: {0}",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.InvalidDocumentBaseUri")]
	public static readonly DiagnosticDescriptor InvalidDocumentBaseUriDiagostic =
		new DiagnosticDescriptor(id: "PS_PARSE_004",
								title: "An invalid base URI was provided by a document",
								messageFormat: "An invalid base URI was provided by a document: {0}",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.InvalidFragmentDiagnostic")]
	public static readonly DiagnosticDescriptor InvalidFragmentDiagostic =
		new DiagnosticDescriptor(id: "PS_PARSE_005",
								title: "The fragment provided was not a valid JSON pointer",
								messageFormat: "The fragment provided was not a valid JSON pointer: {0}",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.InvalidRefDiagnostic")]
	public static readonly DiagnosticDescriptor InvalidRefDiagnostic =
		new DiagnosticDescriptor(id: "PS_PARSE_006",
								title: "Invalid URI provided for ref",
								messageFormat: "Unable to parse provided URI for ref: {0}",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.CouldNotFindTargetNodeDiagnostic")]
	public static readonly DiagnosticDescriptor CouldNotFindTargetNodeDiagnostic =
		new DiagnosticDescriptor(id: "PS_PARSE_007",
								title: "Target node did not exist in given document",
								messageFormat: "Unable to locate node: {0}",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.UnknownAnchorDiagnostic")]
	public static readonly DiagnosticDescriptor UnknownAnchorDiagnostic =
		new DiagnosticDescriptor(id: "PS_PARSE_008",
								title: "Unknown anchor provided in ref",
								messageFormat: "Unable to locate anchor referenced from fragment: {0}",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.ResolveDocumentDiagnostic")]
	public static readonly DiagnosticDescriptor ResolveDocumentDiagnosticDiagnostic =
		new DiagnosticDescriptor(id: "PS_PARSE_009",
								title: "Could not retrieve the specified document",
								messageFormat: "Could not resolve the document with the URI {0}",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
}
