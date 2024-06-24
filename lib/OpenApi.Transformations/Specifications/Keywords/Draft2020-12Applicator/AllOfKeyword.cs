
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Applicator;

/// <see href="https://json-schema.org/draft/2020-12/json-schema-core#section-10.2.1.1">Draft 2020-12 allOf keyword</see>
public class AllOfKeyword(string keyword, IReadOnlyList<JsonSchema> schemas) : IJsonSchemaAnnotation
{
	public static readonly IJsonSchemaKeyword Instance = new JsonSchemaKeyword(Parse);

	private static DiagnosableResult<IJsonSchemaAnnotation> Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		if (nodeInfo.Node is not JsonArray arr)
			return DiagnosableResult<IJsonSchemaAnnotation>.Fail(new UnableToParseKeyword(keyword, options.Registry.ResolveLocation(nodeInfo)));

		var results = arr.Select((_, i) => JsonSchemaParser.Deserialize(nodeInfo.Navigate(i), options))
			.Aggregate(DiagnosableResult<IEnumerable<JsonSchema>>.Pass(Enumerable.Empty<JsonSchema>()),
				(prev, next) =>
					prev.Fold(
						list => next.Fold(
							(schema) => DiagnosableResult<IEnumerable<JsonSchema>>.Pass(list.ConcatOne(schema)),
							(diagnostics) => DiagnosableResult<IEnumerable<JsonSchema>>.Fail(diagnostics.ToArray())
						),
						oldDiagnotics => next.Fold(
							_ => prev,
							(diagnostics) => DiagnosableResult<IEnumerable<JsonSchema>>.Fail(oldDiagnotics.Concat(diagnostics).ToArray())
						))
			)
			.Select<IJsonSchemaAnnotation>((schemas) => new AllOfKeyword(keyword, schemas.ToArray()));
		return results;
	}

	public string Keyword => keyword;

	public IReadOnlyList<JsonSchema> Schemas => schemas;

	public IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, AnnotatedJsonSchema context, EvaluationContext evaluationContext)
	{
		for (var i = 0; i < Schemas.Count; i++)
		{
			foreach (var entry in Schemas[i].Evaluate(nodeMetadata, evaluationContext))
				yield return entry;
		}
	}
}
