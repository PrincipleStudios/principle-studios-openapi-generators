using System.Collections.Generic;
using System;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Metadata;

public class DeprecatedKeyword(string keyword, bool isDeprecated) : IJsonSchemaAnnotation
{
	public static readonly IJsonSchemaKeyword Instance = new JsonSchemaKeyword(Parse);

	private static DiagnosableResult<IJsonSchemaAnnotation> Parse(string keyword, ResolvableNode nodeInfo, JsonSchemaParserOptions options)
	{
		if (nodeInfo.Node is JsonValue val && val.TryGetValue<bool>(out var value))
			return DiagnosableResult<IJsonSchemaAnnotation>.Pass(new DeprecatedKeyword(keyword, value));
		return DiagnosableResult<IJsonSchemaAnnotation>.Fail(new UnableToParseKeyword(keyword, options.Registry.ResolveLocation(nodeInfo)));
	}

	public string Keyword => keyword;

	/// <summary>
	/// Whether to use the maximumValue as an exclusive value.
	/// </summary>
	public bool IsDeprecated => isDeprecated;

	public IEnumerable<DiagnosticBase> Evaluate(ResolvableNode nodeMetadata, AnnotatedJsonSchema context, EvaluationContext evaluationContext)
	{
		yield break;
	}
}
