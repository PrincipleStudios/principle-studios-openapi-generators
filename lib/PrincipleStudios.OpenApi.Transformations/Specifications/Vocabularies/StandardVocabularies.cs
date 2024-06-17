
using System;
using PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;
using PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Applicator;
using PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Metadata;
using PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Validation;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Vocabularies;

public static class StandardVocabularies
{
	public static readonly IJsonSchemaVocabulary Core202012 = new JsonSchemaVocabulary(
		// https://json-schema.org/draft/2020-12/meta/core
		new Uri("https://json-schema.org/draft/2020-12/vocab/core"),
		// TODO
		[
		// ("$id", null),
		// ("$schema", null),
		// ("$ref", null),
		// ("$anchor", null),
		// ("$dynamicRef", null),
		// ("$dynamicAnchor", null),
		// ("$vocabulary", null),
		// ("$comment", null),
		// ("$defs", null),
		]
	);

	public static readonly IJsonSchemaVocabulary Applicator202012 = new JsonSchemaVocabulary(
		// https://json-schema.org/draft/2020-12/meta/applicator
		new Uri("https://json-schema.org/draft/2020-12/vocab/applicator"),
		// TODO
		[
			// ("prefixItems", null),
			("items", ItemsKeyword.Instance),
			// ("contains", null),
			// ("additionalProperties", null),
			("properties", PropertiesKeyword.Instance),
			// ("patternProperties", null),
			// ("dependentSchemas", null),
			// ("propertyNames", null),
			// ("if", null),
			// ("then", null),
			// ("else", null),
			("allOf", AllOfKeyword.Instance),
			("anyOf", AnyOfKeyword.Instance),
			("oneOf", OneOfKeyword.Instance),
			("not", NotKeyword.Instance),
		]
	);

	public static readonly IJsonSchemaVocabulary Unevaluated202012 = new JsonSchemaVocabulary(
		// https://json-schema.org/draft/2020-12/meta/unevaluated
		new Uri("https://json-schema.org/draft/2020-12/vocab/unevaluated"),
		// TODO
		[
		// ("unevaluatedItems", null),
		// ("unevaluatedProperties", null),
		]
	);

	public static readonly IJsonSchemaVocabulary Validation202012 = new JsonSchemaVocabulary(
		// https://json-schema.org/draft/2020-12/meta/validation
		new Uri("https://json-schema.org/draft/2020-12/vocab/validation"),
		// TODO
		[
			// ("type", null),
			// ("const", null),
			("enum", EnumKeyword.Instance),
			("multipleOf", MultipleOfKeyword.Instance),
			("maximum", MaximumKeyword.Instance),
			// ("exclusiveMaximum", null),
			("minimum", MinimumKeyword.Instance),
			// ("exclusiveMinimum", null),
			("maxLength", MaxLengthKeyword.Instance),
			("minLength", MinLengthKeyword.Instance),
			("pattern", PatternKeyword.Instance),
			("maxItems", MaxItemsKeyword.Instance),
			("minItems", MinItemsKeyword.Instance),
			("uniqueItems", UniqueItemsKeyword.Instance),
			// ("maxContains", null),
			// ("minContains", null),
			("maxProperties", MaxPropertiesKeyword.Instance),
			("minProperties", MinPropertiesKeyword.Instance),
			("required", RequiredKeyword.Instance),
			// ("dependentRequired", null),
		]
	);

	public static readonly IJsonSchemaVocabulary Metadata202012 = new JsonSchemaVocabulary(
		// https://json-schema.org/draft/2020-12/meta/meta-data
		new Uri("https://json-schema.org/draft/2020-12/vocab/meta-data"),
		// TODO
		[
			("title", TitleKeyword.Instance),
			("description", DescriptionKeyword.Instance),
			// ("default", null),
			("deprecated", DeprecatedKeyword.Instance),
			("readOnly", ReadOnlyKeyword.Instance),
			("writeOnly", WriteOnlyKeyword.Instance),
			// ("examples", null),
		]
	);

	public static readonly IJsonSchemaVocabulary FormatAnnotation202012 = new JsonSchemaVocabulary(
		// https://json-schema.org/draft/2020-12/meta/format-annotation
		new Uri("https://json-schema.org/draft/2020-12/vocab/format-annotation"),
		// TODO
		[
		// ("format", null),
		]
	);

	public static readonly IJsonSchemaVocabulary Content202012 = new JsonSchemaVocabulary(
		// https://json-schema.org/draft/2020-12/meta/content
		new Uri("https://json-schema.org/draft/2020-12/vocab/content"),
		// TODO
		[
		// ("contentEncoding", null),
		// ("contentMediaType", null),
		// ("contentSchema", null),
		]
	);
}
