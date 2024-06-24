using System;

namespace PrincipleStudios.OpenApi.Transformations.Diagnostics;

public class DocumentException : Exception
{
	public delegate DiagnosticBase ToDiagnostic(Uri retrievalUri);
	private readonly ToDiagnostic constructDiagnostic;

	public DocumentException(ToDiagnostic constructDiagnostic)
	{
		this.constructDiagnostic = constructDiagnostic;
	}
	public DocumentException(ToDiagnostic constructDiagnostic, string message) : base(message)
	{
		this.constructDiagnostic = constructDiagnostic;
	}
	public DocumentException(ToDiagnostic constructDiagnostic, string message, Exception inner) : base(message, inner)
	{
		this.constructDiagnostic = constructDiagnostic;
	}

	public DiagnosticBase ConstructDiagnostic(Uri retrievalUri)
	{
		return constructDiagnostic(retrievalUri);
	}
}
