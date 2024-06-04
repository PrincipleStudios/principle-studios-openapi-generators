using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;

namespace PrincipleStudios.OpenApi.Transformations.Specifications;

public static class JsonSchemaParser
{
	internal static JsonSchemaParseResult Deserialize(NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		switch (nodeInfo.Node)
		{
			case JsonObject obj:
				return DeserializeKeywords(obj);
			case JsonValue v when v.TryGetValue<bool>(out var boolValue):
				return JsonSchemaParseResult.Success(new JsonSchemaBool(nodeInfo.Id, boolValue));
			default:
				return JsonSchemaParseResult.Failure(nodeInfo, options, UnableToParseSchema.Builder());
		}

		JsonSchemaParseResult DeserializeKeywords(JsonObject obj)
		{

			var keywords =
				from kvp in obj
				select DeserializeKeyword(kvp.Key, nodeInfo.Navigate(kvp.Key));
			var diagnostics = keywords.SelectMany(k => k.Diagnostics).ToArray();
			if (diagnostics.Length > 0) return JsonSchemaParseResult.Failure(diagnostics);

			return JsonSchemaParseResult.Success(new AnnotatedJsonSchema(
				nodeInfo.Id,
				// TODO: find a way not to assert non-null here
				keywords.Select(k => k.Keyword!)
			));
		}

		ParseAnnotationResult DeserializeKeyword(string keyword, NodeMetadata nodeInfo)
		{
			foreach (var vocabulary in options.Dialect.Vocabularies)
			{
				if (vocabulary.Keywords.TryGetValue(keyword, out var def))
					return def.ParseAnnotation(keyword, nodeInfo, options);
			}
			return options.Dialect.UnknownKeyword.ParseAnnotation(keyword, nodeInfo, options);
		}
	}
}

public record JsonSchemaParserOptions(
	DocumentRegistry Registry,
	IJsonSchemaDialect Dialect
);

public record JsonSchemaParseResult(
	JsonSchema? JsonSchema,
	IReadOnlyList<DiagnosticBase> Diagnostics
)
{
	public static JsonSchemaParseResult Success(JsonSchema JsonSchema) => new JsonSchemaParseResult(JsonSchema, Array.Empty<DiagnosticBase>());

	public static JsonSchemaParseResult Failure(NodeMetadata nodeInfo, JsonSchemaParserOptions options, params DiagnosticException.ToDiagnostic[] diagnostics) =>
		new JsonSchemaParseResult(null, diagnostics.Select(d => d(options.Registry.ResolveLocation(nodeInfo))).ToArray());
	public static JsonSchemaParseResult Failure(params DiagnosticBase[] diagnostics) => new JsonSchemaParseResult(null, diagnostics);
}
