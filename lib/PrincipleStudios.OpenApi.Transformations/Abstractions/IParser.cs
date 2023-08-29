using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations.Abstractions;

public interface IParser<TResult>
	where TResult : class, IReferenceableDocument
{
	bool CanParse(IDocumentReference documentReference);
	ParseResult<TResult>? Parse(IDocumentReference documentReference);
}

public record ParseResult<TResult>(TResult? Document, IReadOnlyList<Diagnostics.DiagnosticBase> Diagnostics)
	where TResult : class, IReferenceableDocument
{
	public bool HasDocument => Document != null;
}
