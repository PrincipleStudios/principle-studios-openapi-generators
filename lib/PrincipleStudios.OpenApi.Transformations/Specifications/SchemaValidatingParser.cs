using PrincipleStudios.OpenApi.Transformations.Abstractions;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using System;
using System.Collections.Generic;

namespace PrincipleStudios.OpenApi.Transformations.Specifications;

public abstract class SchemaValidatingParser<TInterface> : IParser<TInterface>
	where TInterface : class, IReferenceableDocument
{
	private readonly Func<DocumentRegistry, JsonSchema> schemaResolver;

	protected SchemaValidatingParser(Func<DocumentRegistry, JsonSchema> schemaResolver)
	{
		this.schemaResolver = schemaResolver;
	}

	public abstract bool CanParse(IDocumentReference documentReference);

	public ParseResult<TInterface> Parse(IDocumentReference documentReference, DocumentRegistry documentRegistry)
	{
		if (!CanParse(documentReference)) throw new ArgumentException(Errors.ParserCannotHandleDocument, nameof(documentReference));

		var schema = schemaResolver(documentRegistry);
		var evaluationResults = schema.Evaluate(NodeMetadata.FromRoot(documentReference), new EvaluationContext(documentRegistry));
		return Construct(documentReference, evaluationResults, documentRegistry);
	}

	protected abstract ParseResult<TInterface> Construct(IDocumentReference documentReference, IEnumerable<DiagnosticBase> diagnostics, DocumentRegistry documentRegistry);
}

public record SchemaValidationDiagnostic(string SchemaValidationRule, string SchemaValidationMessage, Location Location) : DiagnosticBase(Location);