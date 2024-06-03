
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;

public class EnumKeyword(string keyword, JsonArray values) : IJsonSchemaKeyword
{
	public static readonly IJsonSchemaKeywordDefinition Instance = new JsonSchemaKeywordDefinition(Parse);

	private static EnumKeyword Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{

		if (nodeInfo.Node is JsonArray values)
			return new EnumKeyword(keyword, values);
		// TODO - parsing errors
		throw new NotImplementedException();
	}

	public string Keyword => keyword;
	public JsonArray Values => values;

	public IEnumerable<EvaluationResults> Evaluate(JsonNode? node, JsonPointer currentPosition, JsonSchemaViaKeywords context)
	{
		// TODO
		throw new System.NotImplementedException();
	}
}
