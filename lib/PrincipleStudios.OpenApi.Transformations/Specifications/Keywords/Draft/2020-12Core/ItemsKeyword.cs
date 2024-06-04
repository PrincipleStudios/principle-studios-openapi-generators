
using System.Collections.Generic;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Core;

/// <see href="https://json-schema.org/draft/2020-12/json-schema-core#section-10.3.1.2">Draft 2020-12 items keyword</see>
public class ItemsKeyword(string keyword, JsonSchema schema) : IJsonSchemaAnnotation
{
	public static readonly IJsonSchemaKeyword Instance = new JsonSchemaKeyword(Parse);

	private static ParseAnnotationResult Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		var schemaResult = JsonSchemaParser.Deserialize(nodeInfo, options);
		if (schemaResult.JsonSchema is JsonSchema schema)
			return ParseAnnotationResult.Success(new ItemsKeyword(keyword, schema));
		return new ParseAnnotationResult(Keyword: null, Diagnostics: schemaResult.Diagnostics);
	}

	public string Keyword => keyword;

	public JsonSchema Schema => schema;

	public IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, AnnotatedJsonSchema context, EvaluationContext evaluationContext)
	{
		// TODO - leverage prefixItems and contains
		if (nodeMetadata.Node is not JsonArray array)
			yield break;

		for (var i = 0; i < array.Count; i++)
		{
			foreach (var entry in Schema.Evaluate(nodeMetadata.Navigate(i), evaluationContext))
				yield return entry;
		}
	}
}
