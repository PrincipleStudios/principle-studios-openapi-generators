using System;
using PrincipleStudios.OpenApiCodegen;

namespace PrincipleStudios.OpenApi.Transformations.Diagnostics;

public static class DiagnosticsConversion
{
	public static DiagnosticInfo ToDiagnosticInfo(DiagnosticBase diagnostic) =>
		new DiagnosticInfo(
			Id: diagnostic.GetType().FullName,
			Location: ToDiagnosticLocation(diagnostic.Location),
			Metadata: diagnostic.GetTextArguments()
		);

	private static DiagnosticLocation ToDiagnosticLocation(Location location)
	{
		return new DiagnosticLocation(
			location.RetrievalUri is { Scheme: "file" } ? location.RetrievalUri.LocalPath : location.RetrievalUri.OriginalString,
			location.Range == null ? null : new DiagnosticLocationRange(ToDiagnosticLocationMark(location.Range.Start), ToDiagnosticLocationMark(location.Range.End))
		);
	}

	private static DiagnosticLocationMark ToDiagnosticLocationMark(FileLocationMark mark) =>
		new DiagnosticLocationMark(mark.Line, mark.Column);
}
