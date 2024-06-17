
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Applicator;

/// <see href="https://json-schema.org/draft/2020-12/json-schema-core#section-10.2.1.1">Draft 2020-12 anyOf keyword</see>
public class AnyOfKeyword(string keyword, IReadOnlyList<JsonSchema> schemas) : IJsonSchemaAnnotation
{
	public static readonly IJsonSchemaKeyword Instance = new JsonSchemaKeyword(Parse);

	private static DiagnosableResult<IJsonSchemaAnnotation> Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		if (nodeInfo.Node is not JsonArray arr)
			return DiagnosableResult<IJsonSchemaAnnotation>.Fail(new UnableToParseKeyword(keyword, options.Registry.ResolveLocation(nodeInfo)));

		var results = arr.Select((_, i) => JsonSchemaParser.Deserialize(nodeInfo.Navigate(i), options))
			.AggregateAll()
			.Select<IJsonSchemaAnnotation>((schemas) => new AnyOfKeyword(keyword, schemas.ToArray()));
		return results;
	}

	public string Keyword => keyword;

	public IReadOnlyList<JsonSchema> Schemas => schemas;

	public IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, AnnotatedJsonSchema context, EvaluationContext evaluationContext)
	{
		var results = Schemas.Select(s => s.Evaluate(nodeMetadata, evaluationContext).ToArray()).ToArray();
		var matches = results.Count(r => r.Length == 0);

		if (matches >= 1) return Enumerable.Empty<DiagnosticBase>();
		return results.OrderBy(r => r.Length).First();
	}
}

