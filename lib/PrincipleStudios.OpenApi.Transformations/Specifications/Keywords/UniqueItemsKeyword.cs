
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;

public class UniqueItemsKeyword(string keyword, bool mustBeUnique) : IJsonSchemaKeyword
{
	public static readonly IJsonSchemaKeywordDefinition Instance = new JsonSchemaKeywordDefinition(Parse);

	private static ParseKeywordResult Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		if (nodeInfo.Node is JsonValue val && val.TryGetValue<bool>(out var value))
			return ParseKeywordResult.Success(new UniqueItemsKeyword(keyword, value));
		// TODO - parsing errors
		throw new NotImplementedException();
	}

	public string Keyword => keyword;
	public bool MustBeUnique => mustBeUnique;

	public IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, JsonSchemaViaKeywords context, EvaluationContext evaluationContext)
	{
		if (nodeMetadata.Node is not JsonArray array) yield break;

		// TODO: .NET 9 has JsonNode.DeepEquals
		var set = new HashSet<string>();
		foreach (var (node, index) in array.Select((node, i) => (node, i)))
		{
			var text = node?.ToJsonString() ?? "null";
			if (set.Contains(text))
			{
				yield return new UniqueItemsKeywordNotUnique(evaluationContext.DocumentRegistry.ResolveLocation(nodeMetadata.Navigate(index)));
				continue;
			}
			set.Add(text);
		}
	}
}

public record UniqueItemsKeywordNotUnique(Location Location) : DiagnosticBase(Location)
{
	public static DiagnosticException.ToDiagnostic Builder() => (Location) => new UniqueItemsKeywordNotUnique(Location);
}
