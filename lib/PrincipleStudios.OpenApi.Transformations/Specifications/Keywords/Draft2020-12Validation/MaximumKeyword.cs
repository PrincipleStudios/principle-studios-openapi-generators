
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;

/// <see href="https://json-schema.org/draft/2020-12/json-schema-validation#name-maximum">Draft 2020-12 maximum keyword</see>
public class MaximumKeyword(string keyword, decimal value) : IJsonSchemaKeyword
{
	public static readonly IJsonSchemaKeywordDefinition Instance = new JsonSchemaKeywordDefinition(Parse);

	private static ParseKeywordResult Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		if (nodeInfo.Node is JsonValue val && val.TryGetValue<decimal>(out var value))
			return ParseKeywordResult.Success(new MaximumKeyword(keyword, value));
		// TODO - parsing errors
		throw new NotImplementedException();
	}

	public string Keyword => keyword;
	public decimal Value => value;

	public IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, JsonSchemaViaKeywords context, EvaluationContext evaluationContext)
	{
		// TODO
		throw new System.NotImplementedException();
	}
}
