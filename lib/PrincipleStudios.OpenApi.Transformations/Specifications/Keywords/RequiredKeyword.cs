using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;

public class RequiredKeyword(string keyword, IReadOnlyList<string> requiredProperties) : IJsonSchemaKeyword
{
	public static readonly IJsonSchemaKeywordDefinition Instance = new JsonSchemaKeywordDefinition(Parse);

	public string Keyword => keyword;
	public IReadOnlyList<string> RequiredProperties => requiredProperties;

	private static ParseKeywordResult Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		if (nodeInfo.Node is not JsonArray array) return ParseKeywordResult.Failure(nodeInfo, options, UnableToParseRequiredKeyword.Builder());

		var requiredProperties = array.Select(entry => entry is JsonValue v && v.TryGetValue<string>(out var s) ? s : null).ToArray();

		var nullProps = (from e in requiredProperties.Select((p, i) => (prop: p, i))
						 where e.prop == null
						 select UnableToParseRequiredKeyword.Builder()(options.Registry.ResolveLocation(nodeInfo.Navigate(e.i)))).ToArray();
		if (nullProps.Length > 0)
			return ParseKeywordResult.Failure(nullProps);

		return ParseKeywordResult.Success(new RequiredKeyword(
			keyword,
			requiredProperties!
		));
	}

	public IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, JsonSchemaViaKeywords context, EvaluationContext evaluationContext)
	{
		if (nodeMetadata.Node is not JsonObject obj) yield break;

		var missing = RequiredProperties.Except(obj.Select(x => x.Key)).ToArray();
		if (missing.Length > 0)
			yield return new MissingRequiredProperties(missing, evaluationContext.DocumentRegistry.ResolveLocation(nodeMetadata));
	}
}

public record UnableToParseRequiredKeyword(Location Location) : DiagnosticBase(Location)
{
	public static DiagnosticException.ToDiagnostic Builder() => (Location) => new UnableToParseRequiredKeyword(Location);
}

public record MissingRequiredProperties(IReadOnlyList<string> Missing, Location Location) : DiagnosticBase(Location)
{
	public static DiagnosticException.ToDiagnostic Builder(IReadOnlyList<string> Missing) => (Location) => new MissingRequiredProperties(Missing, Location);
}
