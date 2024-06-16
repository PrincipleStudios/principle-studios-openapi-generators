
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
using PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0;

// This follows OpenApi 3.0 TypeKeyword, not the actual standard at https://json-schema.org/draft/2020-12/json-schema-validation#name-type

public class TypeKeyword(string keyword, string value) : IJsonSchemaAnnotation
{
	public static readonly IJsonSchemaKeyword Instance = new JsonSchemaKeyword(Parse);

	private static DiagnosableResult<IJsonSchemaAnnotation> Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		if (nodeInfo.Node is JsonValue val && val.TryGetValue<string>(out var s))
			return DiagnosableResult<IJsonSchemaAnnotation>.Pass(new TypeKeyword(keyword, s));
		return DiagnosableResult<IJsonSchemaAnnotation>.Fail(new UnableToParseKeyword(keyword, options.Registry.ResolveLocation(nodeInfo)));
	}

	public string Keyword => keyword;

	public string Value => value;

	public IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, AnnotatedJsonSchema context, EvaluationContext evaluationContext)
	{
		switch (value)
		{
			case Common.Array:
				if (nodeMetadata.Node is JsonArray) yield break;
				break;
			case Common.Object:
				if (nodeMetadata.Node is JsonObject) yield break;
				break;
			case Common.Boolean:
				if ((nodeMetadata.Node as JsonValue)?.TryGetValue<bool>(out var _) ?? false) yield break;
				break;
			case Common.String:
				if ((nodeMetadata.Node as JsonValue)?.TryGetValue<string>(out var _) ?? false) yield break;
				break;
			case Common.Number:
				if ((nodeMetadata.Node as JsonValue)?.TryGetValue<double>(out var _) ?? false) yield break;
				break;
			case Common.Integer:
				if ((nodeMetadata.Node as JsonValue)?.TryGetValue<int>(out var _) ?? false) yield break;
				break;
		}
		yield return new TypeKeywordMismatch(Value, evaluationContext.DocumentRegistry.ResolveLocation(nodeMetadata));
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

public record TypeKeywordMismatch(string TypeValue, Location Location) : DiagnosticBase(Location)
{
	public override IReadOnlyList<string> GetTextArguments() => [TypeValue];
	public static DiagnosticException.ToDiagnostic Builder(string TypeValue) => (Location) => new TypeKeywordMismatch(TypeValue, Location);
}
