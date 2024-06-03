using System;
using System.Text.Json.Nodes;

namespace PrincipleStudios.OpenApi.Transformations.Specifications;

public static class JsonSchemaParser
{
	internal static JsonSchema Deserialize(NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{

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
	}
}

public record JsonSchemaParserOptions(
	DocumentRegistry Registry,
	IJsonSchemaDialect Dialect
);
