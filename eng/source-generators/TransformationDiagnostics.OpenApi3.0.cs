using Microsoft.CodeAnalysis;

namespace PrincipleStudios.OpenApiCodegen;

public partial class TransformationDiagnostics
{
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0.UnhandledExceptionDiagnostic")]
	public static readonly DiagnosticDescriptor OpenApi30UnhandledExceptionDiagnostic =
		new DiagnosticDescriptor(id: "PS_OPENAPI_3.0_UNK",
								title: "Unhandled exception during parsing of an OpenAPI 3.0 document",
								messageFormat: "Unhandled exception during parsing of an OpenAPI 3.0 document: [{0}] {1}",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0.TypeKeywordMismatch")]
	public static readonly DiagnosticDescriptor OpenApi30TypeKeywordMismatch =
		new DiagnosticDescriptor(id: "PS_OPENAPI_3.0_001",
								title: "Type validation failed",
								messageFormat: "Unhandled exception during parsing of an OpenAPI 3.0 document: [{0}] {1}",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0.InvalidNode")]
	public static readonly DiagnosticDescriptor OpenApi30InvalidNode =
		new DiagnosticDescriptor(id: "PS_OPENAPI_3.0_002",
								title: "Unable to parse node in OpenAPI 3.0 document",
								messageFormat: "Unable to parse node of type {0} in OpenAPI 3.0 document",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
}
