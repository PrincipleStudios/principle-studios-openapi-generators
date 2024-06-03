using System;
using System.Collections.Generic;
using System.Linq;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications;

public abstract class JsonSchema
{
	public abstract Uri Id { get; }
	public virtual IReadOnlyCollection<IJsonSchemaKeyword>? Keywords => null;
	public virtual bool? BoolValue => null;

	public abstract IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, EvaluationContext evaluationContext);
}

public record EvaluationContext(DocumentRegistry DocumentRegistry);

public class JsonSchemaBool(Uri id, bool value) : JsonSchema
{
	public override Uri Id => id;

	public override bool? BoolValue => value;

	public override IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, EvaluationContext evaluationContext)
	{
		return value
			? Enumerable.Empty<DiagnosticBase>()
			: [new FalseJsonSchemasFailDiagnostic(id, evaluationContext.DocumentRegistry.ResolveLocation(nodeMetadata))];
	}
}

public record FalseJsonSchemasFailDiagnostic(Uri OriginalSchema, Location Location) : DiagnosticBase(Location);

public class JsonSchemaViaKeywords : JsonSchema
{
	private readonly List<IJsonSchemaKeyword> keywords;

	public JsonSchemaViaKeywords(Uri id, IEnumerable<IJsonSchemaKeyword> keywords)
	{
		this.Id = id;
		this.keywords = keywords.ToList();
	}

	public override Uri Id { get; }

	public override IReadOnlyCollection<IJsonSchemaKeyword> Keywords => keywords.AsReadOnly();

	public override IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, EvaluationContext evaluationContext)
	{
		return (from keyword in Keywords
				let keywordResults = keyword.Evaluate(nodeMetadata, this, evaluationContext)
				from result in keywordResults
				select result);
	}
}

public interface IJsonSchemaKeywordDefinition
{
	// Defniition for a keyword - TODO: parse into IJsonSchemaKeyword
	ParseKeywordResult ParseKeyword(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options);
}

public record ParseKeywordResult(IJsonSchemaKeyword? Keyword, IReadOnlyList<DiagnosticBase> Diagnostics)
{
	public static ParseKeywordResult Success(IJsonSchemaKeyword Keyword) => new ParseKeywordResult(Keyword, Array.Empty<DiagnosticBase>());

	public static ParseKeywordResult Failure(NodeMetadata nodeInfo, JsonSchemaParserOptions options, params DiagnosticException.ToDiagnostic[] diagnostics) =>
		new ParseKeywordResult(null, diagnostics.Select(d => d(options.Registry.ResolveLocation(nodeInfo))).ToArray());
	public static ParseKeywordResult Failure(params DiagnosticBase[] diagnostics) => new ParseKeywordResult(null, diagnostics);
}

public delegate ParseKeywordResult ParseKeyword(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options);

public record JsonSchemaKeywordDefinition(ParseKeyword ParseKeyword) : IJsonSchemaKeywordDefinition
{
	ParseKeywordResult IJsonSchemaKeywordDefinition.ParseKeyword(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		return ParseKeyword(keyword, nodeInfo, options);
	}
}

public interface IJsonSchemaKeyword
{
	string Keyword { get; }

	IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, JsonSchemaViaKeywords context, EvaluationContext evaluationContext);
}
