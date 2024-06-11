using PrincipleStudios.OpenApiCodegen;

namespace PrincipleStudios.OpenApi.Transformations.Diagnostics;

public static class DiagnosticsConversion
{
	public static DiagnosticInfo ToResult(DiagnosticBase diagnostic) =>
		new DiagnosticInfo(diagnostic.GetType().FullName);
}
