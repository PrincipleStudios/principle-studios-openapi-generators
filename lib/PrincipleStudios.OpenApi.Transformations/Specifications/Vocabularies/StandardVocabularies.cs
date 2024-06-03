
using System;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Vocabularies;

public static class StandardVocabularies
{
	public static readonly IJsonSchemaVocabulary Core202012 = new JsonSchemaVocabulary(
		new Uri("https://json-schema.org/draft/2020-12/vocab/core"),
		// TODO
		[]
	);

	public static readonly IJsonSchemaVocabulary Applicator202012 = new JsonSchemaVocabulary(
		new Uri("https://json-schema.org/draft/2020-12/vocab/applicator"),
		// TODO
		[]
	);

	public static readonly IJsonSchemaVocabulary Unevaluated202012 = new JsonSchemaVocabulary(
		new Uri("https://json-schema.org/draft/2020-12/vocab/unevaluated"),
		// TODO
		[]
	);

	public static readonly IJsonSchemaVocabulary Metadata202012 = new JsonSchemaVocabulary(
		new Uri("https://json-schema.org/draft/2020-12/vocab/meta-data"),
		// TODO
		[]
	);

	public static readonly IJsonSchemaVocabulary FormatAnnotation202012 = new JsonSchemaVocabulary(
		new Uri("https://json-schema.org/draft/2020-12/vocab/format-annotation"),
		// TODO
		[]
	);

	public static readonly IJsonSchemaVocabulary Content202012 = new JsonSchemaVocabulary(
		new Uri("https://json-schema.org/draft/2020-12/vocab/content"),
		// TODO
		[]
	);
}
