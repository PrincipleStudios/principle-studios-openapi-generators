using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations.Diagnostics;

public class DiagnosticException : Exception
{
	public delegate DiagnosticBase ToDiagnostic(Location location);
	private readonly ToDiagnostic constructDiagnostic;

	public DiagnosticException(ToDiagnostic constructDiagnostic)
	{
		this.constructDiagnostic = constructDiagnostic;
	}
	public DiagnosticException(ToDiagnostic constructDiagnostic, string message) : base(message)
	{
		this.constructDiagnostic = constructDiagnostic;
	}
	public DiagnosticException(ToDiagnostic constructDiagnostic, string message, Exception inner) : base(message, inner)
	{
		this.constructDiagnostic = constructDiagnostic;
	}

	public DiagnosticBase ConstructDiagnostic(Location location)
	{
		return constructDiagnostic(location);
	}
}
