using Microsoft.CodeAnalysis;
using static PrincipleStudios.OpenApiCodegen.CommonDiagnostics;

namespace PrincipleStudios.OpenApiCodegen;

public partial class TransformationDiagnostics
{
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0.UnhandledExceptionDiagnostic")]
	public static readonly DiagnosticDescriptor OpenApi30UnhandledExceptionDiagnostic =
		new DiagnosticDescriptor(id: "PS_OPENAPI_3_0_UNK",
								title: "Unhandled exception during parsing of an OpenAPI 3.0 document",
								messageFormat: PrincipleStudios_OpenApi_Transformations_Specifications_OpenApi3_0_UnhandledExceptionDiagnostic,
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0.TypeKeywordMismatch")]
	public static readonly DiagnosticDescriptor OpenApi30TypeKeywordMismatch =
		new DiagnosticDescriptor(id: "PS_OPENAPI_3_0_001",
								title: "Type validation failed",
								messageFormat: PrincipleStudios_OpenApi_Transformations_Specifications_OpenApi3_0_TypeKeywordMismatch,
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0.InvalidNode")]
	public static readonly DiagnosticDescriptor OpenApi30InvalidNode =
		new DiagnosticDescriptor(id: "PS_OPENAPI_3_0_002",
								title: "Unable to parse node in OpenAPI 3.0 document",
								messageFormat: PrincipleStudios_OpenApi_Transformations_Specifications_OpenApi3_0_InvalidNode,
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
}
