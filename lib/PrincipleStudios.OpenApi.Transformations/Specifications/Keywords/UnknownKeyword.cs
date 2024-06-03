
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;

public class UnknownKeyword(string keyword, NodeMetadata nodeInfo) : IJsonSchemaKeyword
{
	public static readonly IJsonSchemaKeywordDefinition Instance = new JsonSchemaKeywordDefinition(Parse);

	private static UnknownKeyword Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		return new UnknownKeyword(keyword, nodeInfo);
	}

	public string Keyword => keyword;
	public JsonNode? Value => nodeInfo.Node;

	public IEnumerable<EvaluationResults> Evaluate(JsonNode? node, JsonPointer currentPosition, JsonSchemaViaKeywords context)
	{
		yield break;
	}
}
