﻿using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;
using System;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0;

public class ExclusiveMaximumKeyword(string keyword, bool isExclusive) : IJsonSchemaKeyword
{
	public static readonly IJsonSchemaKeywordDefinition Instance = new JsonSchemaKeywordDefinition(Parse);

	private static ExclusiveMaximumKeyword Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		if (nodeInfo.Node is JsonValue val && val.TryGetValue<bool>(out var value))
			return new ExclusiveMaximumKeyword(keyword, value);
		// TODO - parsing errors
		throw new NotImplementedException();
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

	public IEnumerable<EvaluationResults> Evaluate(JsonNode? node, JsonPointer currentPosition, JsonSchemaViaKeywords context)
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
