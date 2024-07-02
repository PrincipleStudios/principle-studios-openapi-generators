
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Applicator;

/// <see href="https://json-schema.org/draft/2020-12/json-schema-core#section-10.2.1.4">Draft 2020-12 not keyword</see>
public class NotKeyword(string keyword, JsonSchema schema) : IJsonSchemaAnnotation
{
	public static readonly IJsonSchemaKeyword Instance = new JsonSchemaKeyword(Parse);

	private static DiagnosableResult<IJsonSchemaAnnotation> Parse(string keyword, ResolvableNode nodeInfo, JsonSchemaParserOptions options)
	{
		var schemaResult = JsonSchemaParser.Deserialize(nodeInfo, options);
		return schemaResult.Select<IJsonSchemaAnnotation>(schema => new NotKeyword(keyword, schema));
	}

	public string Keyword => keyword;

	public JsonSchema Schema => schema;

	public IEnumerable<DiagnosticBase> Evaluate(ResolvableNode nodeMetadata, AnnotatedJsonSchema context, EvaluationContext evaluationContext)
	{
		if (!Schema.Evaluate(nodeMetadata, evaluationContext).Any())
			yield return new MustNotMatch(evaluationContext.DocumentRegistry.ResolveLocation(nodeMetadata));
	}
}

public record MustNotMatch(Location Location) : DiagnosticBase(Location)
{
	public static DiagnosticException.ToDiagnostic Builder() => (Location) => new MustNotMatch(Location);
}
