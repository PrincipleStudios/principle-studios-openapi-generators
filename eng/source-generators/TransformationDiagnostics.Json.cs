using Microsoft.CodeAnalysis;
using static PrincipleStudios.OpenApiCodegen.CommonDiagnostics;

namespace PrincipleStudios.OpenApiCodegen;

public partial class TransformationDiagnostics
{
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.DocumentTypes.UnableToParseSchema")]
	public static readonly DiagnosticDescriptor UnableToParseSchemaDiagnostic =
		new DiagnosticDescriptor(id: "PS_JSON_001",
								title: "Unable to parse schema; it must be either an object or a boolean value",
								messageFormat: PrincipleStudios_OpenApi_Transformations_DocumentTypes_UnableToParseSchema,
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.Specifications.FalseJsonSchemasFailDiagnostic")]
	public static readonly DiagnosticDescriptor FalseJsonSchemasFailDiagnostic =
		new DiagnosticDescriptor(id: "PS_JSON_002",
								title: "Unable to match against a 'false' schema",
								messageFormat: PrincipleStudios_OpenApi_Transformations_Specifications_FalseJsonSchemasFailDiagnostic,
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.DocumentTypes.YamlLoadDiagnostic")]
	public static readonly DiagnosticDescriptor YamlLoadDiagnosticDiagnostic =
		new DiagnosticDescriptor(id: "PS_JSON_003",
								title: "Unable to parse document",
								messageFormat: PrincipleStudios_OpenApi_Transformations_DocumentTypes_YamlLoadDiagnostic,
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.UnableToParseKeyword")]
	public static readonly DiagnosticDescriptor UnableToParseKeywordDiagnostic =
		new DiagnosticDescriptor(id: "PS_JSON_004",
								title: "Could not parse the keyword",
								messageFormat: PrincipleStudios_OpenApi_Transformations_Specifications_Keywords_UnableToParseKeyword,
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
}
