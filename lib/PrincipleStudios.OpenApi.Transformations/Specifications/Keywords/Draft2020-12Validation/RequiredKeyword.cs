using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Validation;

/// <see href="https://json-schema.org/draft/2020-12/json-schema-validation#name-required">Draft 2020-12 required keyword</see>
public class RequiredKeyword(string keyword, IReadOnlyList<string> requiredProperties) : IJsonSchemaAnnotation
{
	public static readonly IJsonSchemaKeyword Instance = new JsonSchemaKeyword(Parse);

	public string Keyword => keyword;
	public IReadOnlyList<string> RequiredProperties => requiredProperties;

	private static DiagnosableResult<IJsonSchemaAnnotation> Parse(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		if (nodeInfo.Node is not JsonArray array) return DiagnosableResult<IJsonSchemaAnnotation>.Fail(nodeInfo, options.Registry, UnableToParseRequiredKeyword.Builder());

		var requiredProperties = array.Select(entry => entry is JsonValue v && v.TryGetValue<string>(out var s) ? s : null).ToArray();

		var nullProps = (from e in requiredProperties.Select((p, i) => (prop: p, i))
						 where e.prop == null
						 select UnableToParseRequiredKeyword.Builder()(options.Registry.ResolveLocation(nodeInfo.Navigate(e.i)))).ToArray();
		if (nullProps.Length > 0)
			return DiagnosableResult<IJsonSchemaAnnotation>.Fail(nullProps);

		return DiagnosableResult<IJsonSchemaAnnotation>.Pass(new RequiredKeyword(
			keyword,
			requiredProperties!
		));
	}

	public IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, AnnotatedJsonSchema context, EvaluationContext evaluationContext)
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
	public override IReadOnlyList<string> GetTextArguments() => [string.Join(", ", Missing)];
	public static DiagnosticException.ToDiagnostic Builder(IReadOnlyList<string> Missing) => (Location) => new MissingRequiredProperties(Missing, Location);
}
