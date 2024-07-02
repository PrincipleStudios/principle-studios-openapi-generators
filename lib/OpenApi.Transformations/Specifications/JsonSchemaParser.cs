using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;

namespace PrincipleStudios.OpenApi.Transformations.Specifications;

public static class JsonSchemaParser
{
	internal static DiagnosableResult<JsonSchema> Deserialize(ResolvableNode nodeInfo, JsonSchemaParserOptions options)
	{
		switch (nodeInfo.Node)
		{
			case JsonObject obj:
				return DeserializeKeywords(obj);
			case JsonValue v when v.TryGetValue<bool>(out var boolValue):
				return DiagnosableResult<JsonSchema>.Pass(new JsonSchemaBool(nodeInfo.Id, boolValue));
			default:
				return DiagnosableResult<JsonSchema>.Fail(nodeInfo.Metadata, options.Registry, UnableToParseSchema.Builder());
		}

		DiagnosableResult<JsonSchema> DeserializeKeywords(JsonObject obj)
		{
			var keywords =
				from kvp in obj
				select DeserializeKeyword(kvp.Key, nodeInfo.Navigate(kvp.Key));
			var diagnostics = keywords.OfType<DiagnosableResult<IJsonSchemaAnnotation>.Failure>().SelectMany(k => k.Diagnostics).ToArray();
			if (diagnostics.Length > 0) return DiagnosableResult<JsonSchema>.Fail(diagnostics);

			return DiagnosableResult<JsonSchema>.Pass(new AnnotatedJsonSchema(
				nodeInfo.Id,
				keywords.OfType<DiagnosableResult<IJsonSchemaAnnotation>.Success>().Select(k => k.Value)
			));
		}

		DiagnosableResult<IJsonSchemaAnnotation> DeserializeKeyword(string keyword, ResolvableNode nodeInfo)
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
