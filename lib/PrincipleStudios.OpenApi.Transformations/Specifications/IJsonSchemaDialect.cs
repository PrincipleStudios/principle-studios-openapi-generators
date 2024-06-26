using System;
using System.Collections.Generic;
using System.Linq;

namespace PrincipleStudios.OpenApi.Transformations.Specifications;

public interface IJsonSchemaDialect
{
	Uri Id { get; }
	string IdField { get; }
	IReadOnlyCollection<IJsonSchemaVocabulary> Vocabularies { get; }
	IJsonSchemaKeyword UnknownKeyword { get; }
}

public record JsonSchemaDialect(
	Uri Id,
	string IdField,
	IReadOnlyCollection<IJsonSchemaVocabulary> Vocabularies,
	IJsonSchemaKeyword UnknownKeyword
) : IJsonSchemaDialect;

public interface IJsonSchemaVocabulary
{
	Uri Id { get; }
	IReadOnlyDictionary<string, IJsonSchemaKeyword> Keywords { get; }
}

public record JsonSchemaVocabulary(
	Uri Id,
	IReadOnlyDictionary<string, IJsonSchemaKeyword> Keywords
) : IJsonSchemaVocabulary
{
	public JsonSchemaVocabulary(Uri id, (string Keyword, IJsonSchemaKeyword Definition)[] keywords)
		: this(id, keywords.ToDictionary(e => e.Keyword, e => e.Definition))
	{
	}
}
