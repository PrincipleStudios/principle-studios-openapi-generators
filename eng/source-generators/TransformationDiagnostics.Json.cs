using Microsoft.CodeAnalysis;

namespace PrincipleStudios.OpenApiCodegen;

public partial class TransformationDiagnostics
{
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.DocumentTypes.UnableToParseSchema")]
	public static readonly DiagnosticDescriptor UnableToParseSchemaDiagnostic =
		new DiagnosticDescriptor(id: "PS-JSON-001",
								title: "Unable to parse schema; it must be either an object or a boolean value",
								messageFormat: "Unable to parse schema; it must be either an object or a boolean value",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.Specifications.FalseJsonSchemasFailDiagnostic")]
	public static readonly DiagnosticDescriptor FalseJsonSchemasFailDiagnostic =
		new DiagnosticDescriptor(id: "PS-JSON-002",
								title: "Unable to match against a 'false' schema",
								messageFormat: "Unable to match against a 'false' schema",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.DocumentTypes.YamlLoadDiagnostic")]
	public static readonly DiagnosticDescriptor YamlLoadDiagnosticDiagnostic =
		new DiagnosticDescriptor(id: "PS-JSON-003",
								title: "Unable to parse document",
								messageFormat: "Unable to parse document",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
}
