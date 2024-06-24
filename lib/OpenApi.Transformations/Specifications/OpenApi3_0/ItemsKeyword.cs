
using System.Collections.Generic;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0;

/// OpenAPI 3.0 `items` schema
public static class ItemsKeyword
{
	// `items` does not allow an array in OpenAPI 3.0, but does in Draft 04 and newer. Can still use the same keyword instance.
	public static readonly IJsonSchemaKeyword Instance = new JsonSchemaKeyword(Parse);

	private static DiagnosableResult<IJsonSchemaAnnotation> Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		var schemaResult = JsonSchemaParser.Deserialize(nodeInfo, options);
		return schemaResult.Select<IJsonSchemaAnnotation>(schema => new Keywords.Draft2020_12Applicator.ItemsKeyword(keyword, schema));
	}
}
