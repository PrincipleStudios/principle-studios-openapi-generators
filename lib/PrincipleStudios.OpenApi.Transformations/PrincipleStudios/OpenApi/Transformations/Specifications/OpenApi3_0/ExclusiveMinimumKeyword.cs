using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;
using System;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0;

// [SchemaKeyword(Name)]
[JsonConverter(typeof(ExclusiveMinimumKeywordJsonConverter))]
public class ExclusiveMinimumKeyword : IJsonSchemaKeyword
{
	public static readonly IJsonSchemaKeywordDefinition Instance = new JsonSchemaKeywordDefinition();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "exclusiveMinimum";

	public string Keyword => Name;

	/// <summary>
	/// Whether to use the minimumValue as an exclusive value.
	/// </summary>
	public bool Value { get; }

	/// <summary>
	/// Creates a new <see cref="ExclusiveMinimumKeyword"/>.
	/// </summary>
	/// <param name="value">Whether to use the minimumValue as an exclusive value.</param>
	public ExclusiveMinimumKeyword(bool value)
	{
		Value = value;
	}

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

#pragma warning disable CA1812 // apparently not instantiated
internal class ExclusiveMinimumKeywordJsonConverter : JsonConverter<ExclusiveMinimumKeyword>
{
	public override ExclusiveMinimumKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.True && reader.TokenType != JsonTokenType.False)
			throw new JsonException("Expected number");

		var isExclusive = reader.GetBoolean();

		return new ExclusiveMinimumKeyword(isExclusive);
	}
	public override void Write(Utf8JsonWriter writer, ExclusiveMinimumKeyword value, JsonSerializerOptions options)
	{
		writer.WriteBoolean(ExclusiveMinimumKeyword.Name, value.Value);
	}
}
