
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Validation;

/// <see href="https://json-schema.org/draft/2020-12/json-schema-validation#name-enum">Draft 2020-12 enum keyword</see>
public class EnumKeyword(string keyword, JsonArray values) : IJsonSchemaAnnotation
{
	public static readonly IJsonSchemaKeyword Instance = new JsonSchemaKeyword(Parse);

	private static DiagnosableResult<IJsonSchemaAnnotation> Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		if (nodeInfo.Node is JsonArray values)
			return DiagnosableResult<IJsonSchemaAnnotation>.Pass(new EnumKeyword(keyword, values));
		// TODO - parsing errors
		throw new NotImplementedException();
	}

	public string Keyword => keyword;
	public JsonArray Values => values;

	public IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, AnnotatedJsonSchema context, EvaluationContext evaluationContext)
	{
		// TODO
		throw new System.NotImplementedException();
	}
}
