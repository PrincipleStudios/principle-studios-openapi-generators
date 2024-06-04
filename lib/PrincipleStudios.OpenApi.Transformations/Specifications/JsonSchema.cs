using System;
using System.Collections.Generic;
using System.Linq;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;

namespace PrincipleStudios.OpenApi.Transformations.Specifications;

public abstract class JsonSchema
{
	public abstract Uri Id { get; }
	public virtual IReadOnlyCollection<IJsonSchemaAnnotation> Annotations => Array.Empty<IJsonSchemaAnnotation>();
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

public class AnnotatedJsonSchema : JsonSchema
{
	private readonly List<IJsonSchemaAnnotation> keywords;

	public AnnotatedJsonSchema(Uri id, IEnumerable<IJsonSchemaAnnotation> keywords)
	{
		this.Id = id;
		this.keywords = keywords.ToList();
	}

	public override Uri Id { get; }

	public override IReadOnlyCollection<IJsonSchemaAnnotation> Annotations => keywords.AsReadOnly();

	public override IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, EvaluationContext evaluationContext)
	{
		return (from keyword in Annotations
				let keywordResults = keyword.Evaluate(nodeMetadata, this, evaluationContext)
				from result in keywordResults
				select result);
	}
}

public interface IJsonSchemaKeyword
{
	// Defnition for a keyword - TODO: parse into IJsonSchemaKeyword
	ParseAnnotationResult ParseAnnotation(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options);
}

public record ParseAnnotationResult(IJsonSchemaAnnotation? Keyword, IReadOnlyList<DiagnosticBase> Diagnostics)
{
	public static ParseAnnotationResult Success(IJsonSchemaAnnotation Keyword) => new ParseAnnotationResult(Keyword, Array.Empty<DiagnosticBase>());

	public static ParseAnnotationResult Failure(NodeMetadata nodeInfo, JsonSchemaParserOptions options, params DiagnosticException.ToDiagnostic[] diagnostics) =>
		new ParseAnnotationResult(null, diagnostics.Select(d => d(options.Registry.ResolveLocation(nodeInfo))).ToArray());
	public static ParseAnnotationResult Failure(params DiagnosticBase[] diagnostics) => new ParseAnnotationResult(null, diagnostics);
}

public delegate ParseAnnotationResult ParseAnnotation(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options);

public record JsonSchemaKeyword(ParseAnnotation ParseAnnotation) : IJsonSchemaKeyword
{
	ParseAnnotationResult IJsonSchemaKeyword.ParseAnnotation(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		return ParseAnnotation(keyword, nodeInfo, options);
	}
}

public interface IJsonSchemaAnnotation
{
	string Keyword { get; }

	IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, AnnotatedJsonSchema context, EvaluationContext evaluationContext);
}
