using Json.Schema;
using PrincipleStudios.OpenApi.Transformations.Abstractions;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace PrincipleStudios.OpenApi.Transformations.Specifications;

public abstract class SchemaValidatingParser<TInterface> : IParser<TInterface>
	where TInterface : class, IReferenceableDocument
{
	private readonly JsonSchema schema;

	protected SchemaValidatingParser(JsonSchema schema)
	{
		this.schema = schema;
	}

	public abstract bool CanParse(IDocumentReference documentReference);

	public ParseResult<TInterface>? Parse(IDocumentReference documentReference, DocumentRegistry documentRegistry)
	{
		if (!CanParse(documentReference)) throw new ArgumentException(Errors.ParserCannotHandleDocument, nameof(documentReference));

		var evaluationResults = schema.Evaluate(documentReference.RootNode, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });
		return new ParseResult<TInterface>(
			Construct(documentReference, evaluationResults, documentRegistry),
			evaluationResults.IsValid
				? Array.Empty<Diagnostics.DiagnosticBase>()
				: ConvertEvaluationToDiagnostics(documentReference, evaluationResults).ToArray()
		);
	}

	private static IEnumerable<Diagnostics.DiagnosticBase> ConvertEvaluationToDiagnostics(IDocumentReference documentReference, EvaluationResults evaluationResults)
	{
		return from entry in Inner(evaluationResults)
			   let range = documentReference.GetLocation(entry.Pointer)
			   let location = range == null
					? new Location(documentReference.RetrievalUri)
					: new Location(documentReference.RetrievalUri, range)
			   select new SchemaValidationDiagnostic(entry.ErrorKey, entry.ErrorMessage, location);

		IEnumerable<(Json.Pointer.JsonPointer Pointer, string ErrorKey, string ErrorMessage)> Inner(EvaluationResults evaluationResults)
		{
			if (evaluationResults.IsValid) yield break;

			if (evaluationResults.Errors != null)
			{
				if (evaluationResults.Errors.ContainsKey("oneOf"))
				{
					var details = evaluationResults.Details.Select(Inner).Select(d => d.ToArray()).ToArray();
					// "oneOf" rules could be that the user was going for one or the other
					// The one with deeper errors is probably more correct
					var resultingDetails = details.OrderByDescending(d => d.Max(err => err.Pointer.ToString().Length)).First();
					foreach (var entry in resultingDetails)
						yield return entry;
					yield break;
				}

				foreach (var error in evaluationResults.Errors)
				{
					// TODO: depending on the error type, maybe need to highlight the key instead of the full object
					// For instance, in case of missing "required" fields, highlighting the whole object may not be useful
					yield return (evaluationResults.InstanceLocation, error.Key, error.Value);
				}
			}

			foreach (var entry in evaluationResults.Details)
			{
				foreach (var result in Inner(entry))
					yield return result;
			}
		}
	}

	protected abstract TInterface? Construct(IDocumentReference documentReference, EvaluationResults evaluationResults, DocumentRegistry documentRegistry);
}

public record SchemaValidationDiagnostic(string SchemaValidationRule, string SchemaValidationMessage, Location Location) : DiagnosticBase(Location);