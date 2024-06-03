using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace PrincipleStudios.OpenApi.Transformations.Specifications;

public static class JsonSchemaParser
{
	internal static JsonSchema Deserialize(NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		switch (nodeInfo.Node)
		{
			case JsonObject obj:
				return DeserializeKeywords(obj);
			case JsonValue v when v.TryGetValue<bool>(out var boolValue):
				return new JsonSchemaBool(nodeInfo.Id, boolValue);
		}
		// TODO
		// return JsonSerializer.Deserialize<JsonSchema>(data, new JsonSerializerOptions
		// {
		// 	Converters =
		// 	{
		// 		new JsonSchemaWithIdConverter(uriByBytes, keywords),
		// 		new ItemsKeywordJsonConverter(),
		// 		new PropertiesKeywordJsonConverter(),
		// 		new AllOfKeywordJsonConverter(),
		// 		new OneOfKeywordJsonConverter(),
		// 	}
		// });
		throw new NotImplementedException();

		// throw new Diagnostics.DiagnosticException(UnableToParseSchema.Builder(ex));
		JsonSchemaViaKeywords DeserializeKeywords(JsonObject obj)
		{

			var keywords =
				from kvp in obj
				select DeserializeKeyword(kvp.Key, nodeInfo.Navigate(kvp.Key));
			return new JsonSchemaViaKeywords(
				nodeInfo.Id,
				keywords
			);
		}

		IJsonSchemaKeyword DeserializeKeyword(string keyword, NodeMetadata nodeInfo)
		{
			foreach (var vocabulary in options.Dialect.Vocabularies)
			{
				if (vocabulary.Keywords.TryGetValue(keyword, out var def))
					return def.ParseKeyword(keyword, nodeInfo, options);
			}
			return options.Dialect.UnknownKeyword.ParseKeyword(keyword, nodeInfo, options);
		}
	}
}

public record JsonSchemaParserOptions(
	DocumentRegistry Registry,
	IJsonSchemaDialect Dialect
);
