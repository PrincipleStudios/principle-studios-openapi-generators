using System.Collections.Generic;
using System;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
using PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0;

public class NullableKeyword(string keyword, bool isNullable) : IJsonSchemaAnnotation
{
	public static readonly IJsonSchemaKeyword Instance = new JsonSchemaKeyword(Parse);

	private static DiagnosableResult<IJsonSchemaAnnotation> Parse(string keyword, ResolvableNode nodeInfo, JsonSchemaParserOptions options)
	{
		if (nodeInfo.Node is JsonValue val && val.TryGetValue<bool>(out var value))
			return DiagnosableResult<IJsonSchemaAnnotation>.Pass(new NullableKeyword(keyword, value));
		return DiagnosableResult<IJsonSchemaAnnotation>.Fail(new UnableToParseKeyword(keyword, options.Registry.ResolveLocation(nodeInfo)));
	}

	public string Keyword => keyword;

	/// <summary>
	/// Whether to use the maximumValue as an exclusive value.
	/// </summary>
	public bool IsNullable => isNullable;

	public IEnumerable<DiagnosticBase> Evaluate(ResolvableNode nodeMetadata, AnnotatedJsonSchema context, EvaluationContext evaluationContext)
	{
		yield break;
	}
}
