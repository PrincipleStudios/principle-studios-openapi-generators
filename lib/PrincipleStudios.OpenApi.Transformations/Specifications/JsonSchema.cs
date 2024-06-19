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
	DiagnosableResult<IJsonSchemaAnnotation> ParseAnnotation(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options);
}

public delegate DiagnosableResult<IJsonSchemaAnnotation> ParseAnnotation(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options);

public record JsonSchemaKeyword(ParseAnnotation ParseAnnotation) : IJsonSchemaKeyword
{
	DiagnosableResult<IJsonSchemaAnnotation> IJsonSchemaKeyword.ParseAnnotation(string keyword, NodeMetadata nodeInfo, JsonSchemaParserOptions options)
	{
		return ParseAnnotation(keyword, nodeInfo, options);
	}
}

public interface IJsonSchemaAnnotation
{
	string Keyword { get; }

	IEnumerable<DiagnosticBase> Evaluate(NodeMetadata nodeMetadata, AnnotatedJsonSchema context, EvaluationContext evaluationContext);
}
