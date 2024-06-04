﻿using System.Collections.Generic;
using System;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0;

public class ExclusiveMinimumKeyword(string keyword, bool isExclusive) : IJsonSchemaAnnotation
{
	public static readonly IJsonSchemaKeyword Instance = new JsonSchemaKeyword(Parse);

	private static DiagnosableResult<IJsonSchemaAnnotation> Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		if (nodeInfo.Node is JsonValue val && val.TryGetValue<bool>(out var value))
			return DiagnosableResult<IJsonSchemaAnnotation>.Pass(new ExclusiveMinimumKeyword(keyword, value));
		// TODO - parsing errors
		throw new NotImplementedException();
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
