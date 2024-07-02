using System.Collections.Generic;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;

/// <summary>
/// Holds a keyword that is provided but not specified by the dialect/vocabularies
/// </summary>
public class UnknownKeyword(string keyword, ResolvableNode nodeInfo) : IJsonSchemaAnnotation
{
	public static readonly IJsonSchemaKeyword Instance = new JsonSchemaKeyword(Parse);

	private static DiagnosableResult<IJsonSchemaAnnotation> Parse(string keyword, ResolvableNode nodeInfo, JsonSchemaParserOptions options)
	{
		return DiagnosableResult<IJsonSchemaAnnotation>.Pass(new UnknownKeyword(keyword, nodeInfo));
	}

	public string Keyword => keyword;
	public JsonNode? Value => nodeInfo.Node;

	public IEnumerable<DiagnosticBase> Evaluate(ResolvableNode nodeMetadata, AnnotatedJsonSchema context, EvaluationContext evaluationContext)
	{
		yield break;
	}
}
