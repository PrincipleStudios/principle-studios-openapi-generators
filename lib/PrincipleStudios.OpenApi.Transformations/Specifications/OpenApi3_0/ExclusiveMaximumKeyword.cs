using System.Collections.Generic;
using System;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
using PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0;

public class ExclusiveMaximumKeyword(string keyword, bool isExclusive) : IJsonSchemaAnnotation
{
	public static readonly IJsonSchemaKeyword Instance = new JsonSchemaKeyword(Parse);

	private static DiagnosableResult<IJsonSchemaAnnotation> Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		if (nodeInfo.Node is JsonValue val && val.TryGetValue<bool>(out var value))
			return DiagnosableResult<IJsonSchemaAnnotation>.Pass(new ExclusiveMaximumKeyword(keyword, value));
		return DiagnosableResult<IJsonSchemaAnnotation>.Fail(new UnableToParseKeyword(keyword, options.Registry.ResolveLocation(nodeInfo)));
	}

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "exclusiveMaximum";

	public string Keyword => keyword;

	/// <summary>
	/// Whether to use the maximumValue as an exclusive value.
	/// </summary>
	public bool IsExclusive => isExclusive;

	public IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, AnnotatedJsonSchema context, EvaluationContext evaluationContext)
	{
		throw new NotImplementedException();
	}

	// /// <summary>
	// /// Builds a constraint object for a keyword.
	// /// </summary>
	// /// <param name="schemaConstraint">The <see cref="SchemaConstraint"/> for the schema object that houses this keyword.</param>
	// /// <param name="localConstraints">
	// /// The set of other <see cref="KeywordConstraint"/>s that have been processed prior to this one.
	// /// Will contain the constraints for keyword dependencies.
	// /// </param>
	// /// <param name="context">The <see cref="EvaluationContext"/>.</param>
	// /// <returns>A constraint object.</returns>
	// public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
	// 	IReadOnlyList<KeywordConstraint> localConstraints,
	// 	EvaluationContext context)
	// {
	// 	return new KeywordConstraint(Name, Evaluator);
	// }

	// private void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	// {
	// 	throw new NotImplementedException();
	// }
}
