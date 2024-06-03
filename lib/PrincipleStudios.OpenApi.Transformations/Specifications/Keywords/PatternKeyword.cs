
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;

public class PatternKeyword(string keyword, string pattern) : IJsonSchemaKeyword
{
	public static readonly IJsonSchemaKeywordDefinition Instance = new JsonSchemaKeywordDefinition(Parse);

	private static ParseKeywordResult Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		if (nodeInfo.Node is JsonValue val && val.TryGetValue<string>(out var s))
			return ParseKeywordResult.Success(new PatternKeyword(keyword, s));
		// TODO - parsing errors
		throw new NotImplementedException();
	}

	public string Keyword => keyword;

	public string Pattern => pattern;
	public Regex PatternRegex { get; } = new Regex(pattern);

	public IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, JsonSchemaViaKeywords context, EvaluationContext evaluationContext)
	{
		if (nodeMetadata.Node is not JsonValue value || !value.TryGetValue<string>(out var s))
		{
			// TODO: pattern applied to non-string?
			yield break;
		}

		if (!PatternRegex.IsMatch(s))
			yield return new JsonSchemaPatternMismatchDiagnostic(Pattern, evaluationContext.DocumentRegistry.ResolveLocation(nodeMetadata));
	}
}

public record JsonSchemaPatternMismatchDiagnostic(string Pattern, Location Location) : DiagnosticBase(Location);

