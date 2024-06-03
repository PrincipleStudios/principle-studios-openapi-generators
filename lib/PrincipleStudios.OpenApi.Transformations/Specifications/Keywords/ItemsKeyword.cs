
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;

public class ItemsKeyword(string keyword, JsonSchema shema) : IJsonSchemaKeyword
{
	public static readonly IJsonSchemaKeywordDefinition Instance = new JsonSchemaKeywordDefinition(Parse);

	private static ItemsKeyword Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		return new ItemsKeyword(keyword, JsonSchemaParser.Deserialize(nodeInfo, options));
	}

	public string Keyword => keyword;

	// TODO: array of schemas
	public JsonSchema SingleSchema => shema;

	public IEnumerable<EvaluationResults> Evaluate(JsonNode? node, JsonPointer currentPosition, JsonSchemaViaKeywords context)
	{
		// TODO - leverage prefixItems and contains
		throw new System.NotImplementedException();
	}
}
