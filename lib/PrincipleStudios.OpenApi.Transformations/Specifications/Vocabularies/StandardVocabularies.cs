
using System;
using PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Vocabularies;

public static class StandardVocabularies
{
	public static readonly IJsonSchemaVocabulary Core202012 = new JsonSchemaVocabulary(
		new Uri("https://json-schema.org/draft/2020-12/vocab/core"),
		// TODO
		[
		// ("$schema", null),
		// ("$vocabulary", null),
		// ("$id", null),
		// ("$anchor", null),
		// ("$ref", null),
		// ("$dynamicAnchor", null),
		// ("$dynamicRef", null),
		// ("$defs", null),
		// ("$comment", null),
		]
	);

	public static readonly IJsonSchemaVocabulary Applicator202012 = new JsonSchemaVocabulary(
		new Uri("https://json-schema.org/draft/2020-12/vocab/applicator"),
		// TODO
		[
			// ("allOf", null),
			// ("anyOf", null),
			// ("oneOf", null),
			// ("not", null),
			// ("if", null),
			// ("then", null),
			// ("else", null),
			// ("prefixItems", null),
			("items", ItemsKeyword.Instance),
			// ("contains", null),
			("properties", PropertiesKeyword.Instance),
			// ("patternProperties", null),
			// ("additionalProperties", null),
			// ("propertyNames", null),
		]
	);

	public static readonly IJsonSchemaVocabulary Unevaluated202012 = new JsonSchemaVocabulary(
		new Uri("https://json-schema.org/draft/2020-12/vocab/unevaluated"),
		// TODO
		[
		// ("unevaluatedItems", null),
		// ("unevaluatedProperties", null),
		]
	);

	public static readonly IJsonSchemaVocabulary Metadata202012 = new JsonSchemaVocabulary(
		new Uri("https://json-schema.org/draft/2020-12/vocab/meta-data"),
		// TODO
		[
		// ("title", null),
		// ("description", null),
		// ("default", null),
		// ("deprecated", null),
		// ("readOnly", null),
		// ("writeOnly", null),
		// ("examples", null),
		]
	);

	public static readonly IJsonSchemaVocabulary FormatAnnotation202012 = new JsonSchemaVocabulary(
		new Uri("https://json-schema.org/draft/2020-12/vocab/format-annotation"),
		// TODO
		[
		// ("format", null),
		]
	);

	public static readonly IJsonSchemaVocabulary Content202012 = new JsonSchemaVocabulary(
		new Uri("https://json-schema.org/draft/2020-12/vocab/content"),
		// TODO
		[
		// ("contentEncoding", null),
		// ("contentMediaType", null),
		// ("contentSchema", null),
		]
	);
}
