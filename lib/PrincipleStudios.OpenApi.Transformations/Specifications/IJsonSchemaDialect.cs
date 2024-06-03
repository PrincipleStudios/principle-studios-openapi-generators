using System;
using System.Collections.Generic;
using System.Linq;

namespace PrincipleStudios.OpenApi.Transformations.Specifications;

public interface IJsonSchemaDialect
{
	Uri Id { get; }
	IReadOnlyCollection<IJsonSchemaVocabulary> Vocabularies { get; }
	IJsonSchemaKeywordDefinition UnknownKeyword { get; }
}

public record JsonSchemaDialect(
	Uri Id,
	IReadOnlyCollection<IJsonSchemaVocabulary> Vocabularies,
	IJsonSchemaKeywordDefinition UnknownKeyword
) : IJsonSchemaDialect;

public interface IJsonSchemaVocabulary
{
	Uri Id { get; }
	IReadOnlyDictionary<string, IJsonSchemaKeywordDefinition> Keywords { get; }
}

public record JsonSchemaVocabulary(
	Uri Id,
	IReadOnlyDictionary<string, IJsonSchemaKeywordDefinition> Keywords
) : IJsonSchemaVocabulary
{
	public JsonSchemaVocabulary(Uri id, (string Keyword, IJsonSchemaKeywordDefinition Definition)[] keywords)
		: this(id, keywords.ToDictionary(e => e.Keyword, e => e.Definition))
	{
	}
}
