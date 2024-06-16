
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Validation;

/// <see href="https://json-schema.org/draft/2020-12/json-schema-validation#name-pattern">Draft 2020-12 pattern keyword</see>
public class PatternKeyword(string keyword, string pattern) : IJsonSchemaAnnotation
{
	public static readonly IJsonSchemaKeyword Instance = new JsonSchemaKeyword(Parse);

	private static DiagnosableResult<IJsonSchemaAnnotation> Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		if (nodeInfo.Node is JsonValue val && val.TryGetValue<string>(out var s))
			return DiagnosableResult<IJsonSchemaAnnotation>.Pass(new PatternKeyword(keyword, s));
		// TODO - parsing errors
		throw new NotImplementedException();
	}

	public string Keyword => keyword;

	public string Pattern => pattern;
	public Regex PatternRegex { get; } = new Regex(pattern);

	public IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, AnnotatedJsonSchema context, EvaluationContext evaluationContext)
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

public record JsonSchemaPatternMismatchDiagnostic(string Pattern, Location Location) : DiagnosticBase(Location)
{
	public override IReadOnlyList<string> GetTextArguments() => [Pattern];
}

