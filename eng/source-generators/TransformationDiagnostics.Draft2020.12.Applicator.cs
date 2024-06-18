using Microsoft.CodeAnalysis;
using static PrincipleStudios.OpenApiCodegen.CommonDiagnostics;

namespace PrincipleStudios.OpenApiCodegen;

public partial class TransformationDiagnostics
{
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Applicator.MustNotMatch")]
	public static readonly DiagnosticDescriptor MustNotMatchDiagnostic =
		new DiagnosticDescriptor(id: "PS_JSON_2020_12_APP_001",
								title: "Value matched 'not'-constrained schema",
								messageFormat: PrincipleStudios_OpenApi_Transformations_Specifications_Keywords_Draft2020_12Applicator_MustNotMatch,
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);
	[TransformationDiagnostic("PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Applicator.OnlyOneMustMatch")]
	public static readonly DiagnosticDescriptor OnlyOneMustMatchDiagnostic =
		new DiagnosticDescriptor(id: "PS_JSON_2020_12_APP_002",
								title: "Value matched multiple 'oneOf'-constrained schemas",
								messageFormat: PrincipleStudios_OpenApi_Transformations_Specifications_Keywords_Draft2020_12Applicator_OnlyOneMustMatch,
								category: "PrincipleStudios.OpenApiCodegen",
								DiagnosticSeverity.Error,
								isEnabledByDefault: true);

}
