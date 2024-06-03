
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;

public class PropertiesKeyword(string keyword, IReadOnlyDictionary<string, JsonSchema> properties) : IJsonSchemaKeyword
{
	public static readonly IJsonSchemaKeywordDefinition Instance = new JsonSchemaKeywordDefinition(Parse);

	private static PropertiesKeyword Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		if (nodeInfo.Node is not JsonObject obj)
			// TODO - parsing errors
			throw new NotImplementedException();

		return new PropertiesKeyword(
			keyword,
			obj.ToDictionary(
				(kvp) => kvp.Key,
				(kvp) => JsonSchemaParser.Deserialize(nodeInfo.Navigate(kvp.Key), options)
			)
		);
	}

	public string Keyword => keyword;

	// TODO: array of schemas
	public IReadOnlyDictionary<string, JsonSchema> Properties => properties;

	public IEnumerable<EvaluationResults> Evaluate(JsonNode? node, JsonPointer currentPosition, JsonSchemaViaKeywords context)
	{
		if (node is not JsonObject obj) yield break;

		foreach (var kvp in obj)
		{
			if (!properties.TryGetValue(kvp.Key, out var valueSchema))
				// Ignore properties not defined
				continue;
			var result = valueSchema.Evaluate(kvp.Value, currentPosition.Combine(kvp.Key));
			if (!result.IsValid)
				yield return result;
		}
	}
}
