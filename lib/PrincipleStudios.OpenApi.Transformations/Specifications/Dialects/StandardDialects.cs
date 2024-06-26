using PrincipleStudios.OpenApi.Transformations.Specifications.Vocabularies;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.Dialects;

public static class StandardDialects
{
	public static readonly IJsonSchemaDialect CoreNext = new JsonSchemaDialect(
		new System.Uri("https://json-schema.org/draft/2020-12/schema"),
		"$id",
		[
			StandardVocabularies.Core202012,
			StandardVocabularies.Applicator202012,
			StandardVocabularies.Unevaluated202012,
			StandardVocabularies.Validation202012,
			StandardVocabularies.Metadata202012,
			StandardVocabularies.FormatAnnotation202012,
			StandardVocabularies.Content202012,
		],
		Keywords.UnknownKeyword.Instance
	);
}
