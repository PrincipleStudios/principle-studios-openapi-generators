
using System.Collections.Generic;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;

public class ItemsKeyword(string keyword, JsonSchema shema) : IJsonSchemaKeyword
{
	public static readonly IJsonSchemaKeywordDefinition Instance = new JsonSchemaKeywordDefinition(Parse);

	private static ParseKeywordResult Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		var schemaResult = JsonSchemaParser.Deserialize(nodeInfo, options);
		if (schemaResult.JsonSchema is JsonSchema schema)
			return ParseKeywordResult.Success(new ItemsKeyword(keyword, schema));
		return new ParseKeywordResult(Keyword: null, Diagnostics: schemaResult.Diagnostics);
	}

	public string Keyword => keyword;

	// TODO: an array of schemas is allowed in later versions of this keyword
	public JsonSchema SingleSchema => shema;

	public IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, JsonSchemaViaKeywords context, EvaluationContext evaluationContext)
	{
		// TODO - leverage prefixItems and contains
		if (nodeMetadata.Node is not JsonArray array)
			yield break;

		for (var i = 0; i < array.Count; i++)
		{
			foreach (var entry in SingleSchema.Evaluate(nodeMetadata.Navigate(i), evaluationContext))
				yield return entry;
		}
	}
}
