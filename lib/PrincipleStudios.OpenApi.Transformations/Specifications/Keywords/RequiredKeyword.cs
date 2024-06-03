
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Pointer;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;

public class RequiredKeyword(string keyword, IReadOnlyList<string> requiredProperties) : IJsonSchemaKeyword
{
	public static readonly IJsonSchemaKeywordDefinition Instance = new JsonSchemaKeywordDefinition(Parse);

	public string Keyword => keyword;
	public IReadOnlyList<string> RequiredProperties => requiredProperties;

	private static RequiredKeyword Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		return new RequiredKeyword(
			keyword,
			JsonSerializer.Deserialize<string[]>(
				nodeInfo.Node
			) ?? throw new DiagnosticException(UnableToParseRequiredKeyword.Builder())
		);
	}

	public IEnumerable<EvaluationResults> Evaluate(JsonNode? node, JsonPointer currentPosition, JsonSchemaViaKeywords context)
	{
		if (node is not JsonObject obj) yield break;

		var missing = RequiredProperties.Except(obj.Select(x => x.Key)).ToArray();
		if (missing.Length > 0)
			yield return new EvaluationResults(currentPosition, false, context.Id, string.Format(Errors.SchemaKeywordRequired_Missing, string.Join(", ", missing)));
	}
}

public record UnableToParseRequiredKeyword(Location Location) : DiagnosticBase(Location)
{
	public static DiagnosticException.ToDiagnostic Builder() => (Location) => new UnableToParseRequiredKeyword(Location);
}
