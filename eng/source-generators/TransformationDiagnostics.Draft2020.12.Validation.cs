using Microsoft.CodeAnalysis;

namespace PrincipleStudios.OpenApiCodegen;

public partial class TransformationDiagnostics
{
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Validation.JsonSchemaPatternMismatchDiagnostic")]
	public static readonly DiagnosticDescriptor JsonSchemaPatternMismatchDiagnostic =
		new DiagnosticDescriptor(id: "PS_JSON_2020_12_VAL_001",
								title: "Value did not match pattern",
								messageFormat: "Value did not match pattern: {0}",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Validation.MissingRequiredProperties")]
	public static readonly DiagnosticDescriptor JsonSchemaMissingRequiredPropertiesDiagnostic =
		new DiagnosticDescriptor(id: "PS_JSON_2020_12_VAL_002",
								title: "Required properties are missing from object",
								messageFormat: "Value did not match pattern: {0}",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Validation.UniqueItemsKeywordNotUnique")]
	public static readonly DiagnosticDescriptor JsonSchemaUniqueItemsKeywordNotUniqueDiagnostic =
		new DiagnosticDescriptor(id: "PS_JSON_2020_12_VAL_003",
								title: "Array items must be unique",
								messageFormat: "Array items must be unique; detected duplicate array",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
}
