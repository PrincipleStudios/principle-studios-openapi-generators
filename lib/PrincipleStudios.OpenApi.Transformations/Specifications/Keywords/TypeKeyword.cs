
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;

public class TypeKeyword : IJsonSchemaKeyword
{
	public static readonly IJsonSchemaKeywordDefinition Instance = new JsonSchemaKeywordDefinition();

	public string Keyword => throw new System.NotImplementedException();

	public string Value => throw new System.NotImplementedException();

	public IEnumerable<EvaluationResults> Evaluate(JsonNode? node, JsonPointer currentPosition, JsonSchemaViaKeywords context)
	{
		// TODO
		throw new System.NotImplementedException();
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
