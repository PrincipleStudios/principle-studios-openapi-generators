using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.Pointer;
using Microsoft.Win32.SafeHandles;
using YamlDotNet.Core.Tokens;

namespace PrincipleStudios.OpenApi.Transformations.Specifications;

public abstract class JsonSchema
{
	public abstract Uri Id { get; }
	public virtual IReadOnlyCollection<IJsonSchemaKeyword>? Keywords => null;
	public virtual bool? BoolValue => null;

	public EvaluationResults Evaluate(JsonNode? node) => Evaluate(node, JsonPointer.Empty);
	public abstract EvaluationResults Evaluate(JsonNode? node, JsonPointer position);
}

public record EvaluationResults(
	JsonPointer InstanceLocation,
	bool IsValid,
	Uri SchemaId,
	string? Message,
	IReadOnlyDictionary<string, IReadOnlyList<EvaluationResults>> Errors
);

public class JsonSchemaBool(Uri id, bool value) : JsonSchema
{
	public override Uri Id => id;

	private static readonly IReadOnlyDictionary<string, IReadOnlyList<EvaluationResults>> EmptyErrors =
		Enumerable.Empty<KeyValuePair<string, EvaluationResults>>()
			.GroupBy(k => k.Key, k => k.Value)
			.ToDictionary(k => k.Key, v => v.ToArray() as IReadOnlyList<EvaluationResults>);
	public override bool? BoolValue => value;

	public override EvaluationResults Evaluate(JsonNode? node, JsonPointer position)
	{
		return new EvaluationResults(
			InstanceLocation: position,
			IsValid: value,
			SchemaId: Id,
			Message: value ? null : Errors.FalseJsonSchemasFail,
			Errors: EmptyErrors
		);
	}
}

public class JsonSchemaViaKeywords : JsonSchema
{
	private readonly List<IJsonSchemaKeyword> keywords;

	public JsonSchemaViaKeywords(Uri id, IEnumerable<IJsonSchemaKeyword> keywords)
	{
		this.Id = id;
		this.keywords = keywords.ToList();
	}

	public override Uri Id { get; }

	public override IReadOnlyCollection<IJsonSchemaKeyword> Keywords => keywords.AsReadOnly();

	public override EvaluationResults Evaluate(JsonNode? node, JsonPointer position)
	{
		var errors = (from keyword in Keywords
					  let keywordResults = keyword.Evaluate(node, position, this)
					  from result in keywordResults
					  where !result.IsValid
					  group result by keyword.Keyword)
				.ToDictionary(k => k.Key, v => v.ToArray() as IReadOnlyList<EvaluationResults>);
		return new EvaluationResults(
			InstanceLocation: JsonPointer.Empty,
			IsValid: errors.Count == 0,
			SchemaId: Id,
			Message: null,
			Errors: errors);
	}
}

public interface IJsonSchemaKeywordDefinition
{
	// Defniition for a keyword - TODO: parse nito IJsonSchemaKeyword
}

public record JsonSchemaKeywordDefinition() : IJsonSchemaKeywordDefinition;

public interface IJsonSchemaKeyword
{
	string Keyword { get; }

	IEnumerable<EvaluationResults> Evaluate(JsonNode? node, JsonPointer currentPosition, JsonSchemaViaKeywords context);
}
