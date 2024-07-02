
using System.Collections.Generic;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0;

public class ExampleKeyword(string keyword, JsonNode? example) : IJsonSchemaAnnotation
{
	public static readonly IJsonSchemaKeyword Instance = new JsonSchemaKeyword(Parse);

	private static DiagnosableResult<IJsonSchemaAnnotation> Parse(string keyword, ResolvableNode nodeInfo, JsonSchemaParserOptions options)
	{
		return DiagnosableResult<IJsonSchemaAnnotation>.Pass(new ExampleKeyword(keyword, nodeInfo.Node));
	}

	public string Keyword => keyword;

	public JsonNode? Example => example;

	public IEnumerable<DiagnosticBase> Evaluate(ResolvableNode nodeMetadata, AnnotatedJsonSchema context, EvaluationContext evaluationContext)
	{
		yield break;
	}
}
