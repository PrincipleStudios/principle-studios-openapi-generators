
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Validation;

/// <see href="https://json-schema.org/draft/2020-12/json-schema-validation#name-minimum">Draft 2020-12 minimum keyword</see>
public class MinimumKeyword(string keyword, decimal value) : IJsonSchemaAnnotation
{
	public static readonly IJsonSchemaKeyword Instance = new JsonSchemaKeyword(Parse);

	private static DiagnosableResult<IJsonSchemaAnnotation> Parse(string keyword, ResolvableNode nodeInfo, JsonSchemaParserOptions options)
	{
		if (nodeInfo.Node is JsonValue val && val.TryGetValue<decimal>(out var value))
			return DiagnosableResult<IJsonSchemaAnnotation>.Pass(new MinimumKeyword(keyword, value));
		return DiagnosableResult<IJsonSchemaAnnotation>.Fail(new UnableToParseKeyword(keyword, options.Registry.ResolveLocation(nodeInfo)));
	}

	public string Keyword => keyword;
	public decimal Value => value;

	public IEnumerable<DiagnosticBase> Evaluate(ResolvableNode nodeMetadata, AnnotatedJsonSchema context, EvaluationContext evaluationContext)
	{
		// TODO
		throw new System.NotImplementedException();
	}
}
