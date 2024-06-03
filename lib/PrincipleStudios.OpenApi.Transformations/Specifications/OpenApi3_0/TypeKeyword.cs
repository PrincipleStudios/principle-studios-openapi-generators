
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0;

// This follows OpenApi 3.0 TypeKeyword, not the actual standard at https://json-schema.org/draft/2020-12/json-schema-validation#name-type
public class TypeKeyword(string keyword, string value) : IJsonSchemaKeyword
{
	public static readonly IJsonSchemaKeywordDefinition Instance = new JsonSchemaKeywordDefinition(Parse);

	private static TypeKeyword Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		if (nodeInfo.Node is JsonValue val && val.TryGetValue<string>(out var s))
			return new TypeKeyword(keyword, s);
		// TODO - parsing errors
		throw new NotImplementedException();
	}

	public string Keyword => keyword;

	public string Value => value;

	public IEnumerable<EvaluationResults> Evaluate(JsonNode? node, JsonPointer currentPosition, JsonSchemaViaKeywords context)
	{
		switch (value)
		{
			case Common.Array:
				if (node is JsonArray) yield break;
				break;
			case Common.Object:
				if (node is JsonObject) yield break;
				break;
			case Common.Boolean:
				if ((node as JsonValue)?.TryGetValue<bool>(out var _) ?? false) yield break;
				break;
			case Common.String:
				if ((node as JsonValue)?.TryGetValue<string>(out var _) ?? false) yield break;
				break;
			case Common.Number:
				if ((node as JsonValue)?.TryGetValue<double>(out var _) ?? false) yield break;
				break;
			case Common.Integer:
				if ((node as JsonValue)?.TryGetValue<int>(out var _) ?? false) yield break;
				break;
		}
		yield return new EvaluationResults(currentPosition, false, context.Id, Errors.SchemaKeywordType_DidNotMatch);
	}

	public static class Common
	{
#pragma warning disable CA1720 // Identifier contains type name
		public const string Array = "array";
		public const string Object = "object";
		public const string Boolean = "boolean";
		public const string String = "string";
		public const string Number = "number";
		public const string Integer = "integer";
		public const string Null = "null";
#pragma warning restore CA1720 // Identifier contains type name
	}
}
