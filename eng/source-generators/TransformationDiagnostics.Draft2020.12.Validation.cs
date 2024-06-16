using Microsoft.CodeAnalysis;

namespace PrincipleStudios.OpenApiCodegen;

public partial class TransformationDiagnostics
{
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Validation.UnableToParseRequiredKeyword")]
	public static readonly DiagnosticDescriptor UnableToParseRequiredKeyword =
		new DiagnosticDescriptor(id: "PS-JSON-2020-12-VAL-001",
								title: "Could not parse the 'required' property; is it a string?",
								messageFormat: "Could not parse the 'required' property; is it a string?",
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
}
