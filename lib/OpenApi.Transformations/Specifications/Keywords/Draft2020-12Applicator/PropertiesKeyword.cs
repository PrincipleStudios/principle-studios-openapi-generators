
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Applicator;

/// <see href="https://json-schema.org/draft/2020-12/json-schema-core#name-properties">Draft 2020-12 properties keyword</see>
public class PropertiesKeyword(string keyword, IReadOnlyDictionary<string, JsonSchema> properties) : IJsonSchemaAnnotation
{
	public static readonly IJsonSchemaKeyword Instance = new JsonSchemaKeyword(Parse);

	private static DiagnosableResult<IJsonSchemaAnnotation> Parse(string keyword, ResolvableNode nodeInfo, JsonSchemaParserOptions options)
	{
		if (nodeInfo.Node is not JsonObject obj)
			return DiagnosableResult<IJsonSchemaAnnotation>.Fail(new UnableToParseKeyword(keyword, options.Registry.ResolveLocation(nodeInfo)));

		var results = obj.ToDictionary(
				(kvp) => kvp.Key,
				(kvp) => JsonSchemaParser.Deserialize(nodeInfo.Navigate(kvp.Key), options)
			);
		var failures = results.Values.OfType<DiagnosableResult<JsonSchema>.Failure>().ToArray();
		if (failures.Length > 0)
			return DiagnosableResult<IJsonSchemaAnnotation>.Fail(failures.SelectMany(v => v.Diagnostics).ToArray());

		return DiagnosableResult<IJsonSchemaAnnotation>.Pass(new PropertiesKeyword(
			keyword,
			results.ToDictionary(
				(kvp) => kvp.Key,
				(kvp) => ((DiagnosableResult<JsonSchema>.Success)kvp.Value).Value
			)
		));
	}

	public string Keyword => keyword;

	// TODO: an array of schemas is supported for each property in a later version of this keyword
	public IReadOnlyDictionary<string, JsonSchema> Properties => properties;

	public IEnumerable<DiagnosticBase> Evaluate(ResolvableNode nodeMetadata, AnnotatedJsonSchema context, EvaluationContext evaluationContext)
	{
		if (nodeMetadata.Node is not JsonObject obj) yield break;

		foreach (var kvp in obj)
		{
			if (!properties.TryGetValue(kvp.Key, out var valueSchema))
				// Ignore properties not defined
				continue;
			foreach (var entry in valueSchema.Evaluate(nodeMetadata.Navigate(kvp.Key), evaluationContext))
				yield return entry;
		}
	}
}
