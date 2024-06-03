
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.Pointer;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;

public class PatternKeyword(string keyword, string pattern) : IJsonSchemaKeyword
{
	public static readonly IJsonSchemaKeywordDefinition Instance = new JsonSchemaKeywordDefinition(Parse);

	private static PatternKeyword Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		if (nodeInfo.Node is JsonValue val && val.TryGetValue<string>(out var s))
			return new PatternKeyword(keyword, s);
		// TODO - parsing errors
		throw new NotImplementedException();
	}

	public string Keyword => keyword;

	public string Pattern => pattern;
	public Regex PatternRegex { get; } = new Regex(pattern);

	public IEnumerable<EvaluationResults> Evaluate(JsonNode? node, JsonPointer currentPosition, JsonSchemaViaKeywords context)
	{
		// TODO
		throw new System.NotImplementedException();
	}
}
