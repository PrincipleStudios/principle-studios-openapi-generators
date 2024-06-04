using System.Collections.Generic;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;

/// <summary>
/// Holds a keyword that is provided but not specified by the dialect/vocabularies
/// </summary>
public class UnknownKeyword(string keyword, NodeMetadata nodeInfo) : IJsonSchemaKeyword
{
	public static readonly IJsonSchemaKeywordDefinition Instance = new JsonSchemaKeywordDefinition(Parse);

	private static ParseKeywordResult Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		return ParseKeywordResult.Success(new UnknownKeyword(keyword, nodeInfo));
	}

	public string Keyword => keyword;
	public JsonNode? Value => nodeInfo.Node;

	public IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, JsonSchemaViaKeywords context, EvaluationContext evaluationContext)
	{
		yield break;
	}
}
