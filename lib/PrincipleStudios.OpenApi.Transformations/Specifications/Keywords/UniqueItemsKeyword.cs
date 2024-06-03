
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;

public class UniqueItemsKeyword : IJsonSchemaKeyword
{
	public static readonly IJsonSchemaKeywordDefinition Instance = new JsonSchemaKeywordDefinition(Parse);

	private static IJsonSchemaKeyword Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		// TODO
		throw new NotImplementedException();
	}

	public string Keyword => throw new System.NotImplementedException();

	public IEnumerable<EvaluationResults> Evaluate(JsonNode? node, JsonPointer currentPosition, JsonSchemaViaKeywords context)
	{
		// TODO
		throw new System.NotImplementedException();
	}
}
