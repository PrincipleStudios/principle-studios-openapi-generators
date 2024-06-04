
using System;
using System.Collections.Generic;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Validation;

/// <see href="https://json-schema.org/draft/2020-12/json-schema-validation#name-multipleof">Draft 2020-12 multipleOf keyword</see>
public class MultipleOfKeyword : IJsonSchemaAnnotation
{
	public static readonly IJsonSchemaKeyword Instance = new JsonSchemaKeyword(Parse);

	private static ParseAnnotationResult Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		// TODO
		throw new NotImplementedException();
	}

	public string Keyword => throw new System.NotImplementedException();

	public IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, AnnotatedJsonSchema context, EvaluationContext evaluationContext)
	{
		// TODO
		throw new System.NotImplementedException();
	}
}
