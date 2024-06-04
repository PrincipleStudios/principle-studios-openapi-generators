
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Core;

/// <see href="https://json-schema.org/draft/2020-12/json-schema-core#name-properties">Draft 2020-12 properties keyword</see>
public class PropertiesKeyword(string keyword, IReadOnlyDictionary<string, JsonSchema> properties) : IJsonSchemaKeyword
{
	public static readonly IJsonSchemaKeywordDefinition Instance = new JsonSchemaKeywordDefinition(Parse);

	private static ParseKeywordResult Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		if (nodeInfo.Node is not JsonObject obj)
			// TODO - parsing errors
			throw new NotImplementedException();

		var results = obj.ToDictionary(
				(kvp) => kvp.Key,
				(kvp) => JsonSchemaParser.Deserialize(nodeInfo.Navigate(kvp.Key), options)
			);
		var diagnostics = results.Values.SelectMany(v => v.Diagnostics).ToArray();
		if (diagnostics.Length > 0) return ParseKeywordResult.Failure(diagnostics);

		return ParseKeywordResult.Success(new PropertiesKeyword(
			keyword,
			results.ToDictionary(
				(kvp) => kvp.Key,
				(kvp) => kvp.Value.JsonSchema!
			)
		));
	}

	public string Keyword => keyword;

	// TODO: an array of schemas is supported for each property in a later version of this keyword
	public IReadOnlyDictionary<string, JsonSchema> Properties => properties;

	public IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, JsonSchemaViaKeywords context, EvaluationContext evaluationContext)
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
