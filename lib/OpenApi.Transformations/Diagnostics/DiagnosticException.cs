using System;

namespace PrincipleStudios.OpenApi.Transformations.Diagnostics;

public class DiagnosticException : Exception
{
	public delegate DiagnosticBase ToDiagnostic(Location location);
	private readonly ToDiagnostic constructDiagnostic;

	public DiagnosticException(ToDiagnostic constructDiagnostic)
	{
		this.constructDiagnostic = constructDiagnostic;
	}

	public DiagnosticBase ConstructDiagnostic(Location location)
	{
		return constructDiagnostic(location);
	}
}
