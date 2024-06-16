using System.Collections.Generic;
using System;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
using PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0;

public class ExclusiveMinimumKeyword(string keyword, bool isExclusive) : IJsonSchemaAnnotation
{
	public static readonly IJsonSchemaKeyword Instance = new JsonSchemaKeyword(Parse);

	private static DiagnosableResult<IJsonSchemaAnnotation> Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		if (nodeInfo.Node is JsonValue val && val.TryGetValue<bool>(out var value))
			return DiagnosableResult<IJsonSchemaAnnotation>.Pass(new ExclusiveMinimumKeyword(keyword, value));
		return DiagnosableResult<IJsonSchemaAnnotation>.Fail(new UnableToParseKeyword(keyword, options.Registry.ResolveLocation(nodeInfo)));
	}

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "exclusiveMinimum";

	public string Keyword => keyword;

	/// <summary>
	/// Whether to use the minimumValue as an exclusive value.
	/// </summary>
	public bool IsExclusive => isExclusive;

	public IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, AnnotatedJsonSchema context, EvaluationContext evaluationContext)
	{
		throw new NotImplementedException();
	}
}
