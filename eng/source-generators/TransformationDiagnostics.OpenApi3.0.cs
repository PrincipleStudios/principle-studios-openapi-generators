using Microsoft.CodeAnalysis;

namespace PrincipleStudios.OpenApiCodegen;

public partial class TransformationDiagnostics
{
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0.UnhandledExceptionDiagnostic")]
	public static readonly DiagnosticDescriptor OpenApi30UnhandledExceptionDiagnostic =
		new DiagnosticDescriptor(id: "PS-OPENAPI-3.0-001",
								title: "Unhandled exception during parsing of an OpenAPI 3.0 document",
								messageFormat: "Unhandled exception during parsing of an OpenAPI 3.0 document: [{0}] {1}",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
}
