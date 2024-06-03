
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;

public class UniqueItemsKeyword(string keyword, bool mustBeUnique) : IJsonSchemaKeyword
{
	public static readonly IJsonSchemaKeywordDefinition Instance = new JsonSchemaKeywordDefinition(Parse);

	private static UniqueItemsKeyword Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		if (nodeInfo.Node is JsonValue val && val.TryGetValue<bool>(out var value))
			return new UniqueItemsKeyword(keyword, value);
		// TODO - parsing errors
		throw new NotImplementedException();
	}

	public string Keyword => keyword;
	public bool MustBeUnique => mustBeUnique;

	public IEnumerable<EvaluationResults> Evaluate(JsonNode? node, JsonPointer currentPosition, JsonSchemaViaKeywords context)
	{
		// TODO
		throw new System.NotImplementedException();
	}
}
