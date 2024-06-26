using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;
using PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0;
using System.Linq;
using Xunit;
using static PrincipleStudios.OpenApiCodegen.TestUtils.DocumentHelpers;

namespace PrincipleStudios.OpenApi.Transformations;

public class OpenApi3_0ParserShould
{
	[InlineData("all-of.yaml")]
	[InlineData("annotations.yaml")]
	[InlineData("enum.yaml")]
	[InlineData("controller-extension.yaml")]
	[InlineData("regex-escape.yaml")]
	[InlineData("validation-min-max.yaml")]
	[InlineData("headers.yaml")]
	[InlineData("oauth.yaml")]
	[InlineData("form.yaml")]
	[InlineData("one-of.yaml")]
	[InlineData("nullable-vs-optional.yaml")]
	[InlineData("nullable-vs-optional-legacy.yaml")]
	[InlineData("exclusive-range-openapi-3_0.yaml")]
	[Theory]
	public void Loads_all_yaml(string yamlName)
	{
		var result = GetOpenApiDocument(yamlName);

		Assert.Empty(result.Diagnostics);
		Assert.NotNull(result.Document);
	}

	[Fact]
	public void Loads_petstore_yaml()
	{
		var result = GetOpenApiDocument("petstore.yaml");

		Assert.Empty(result.Diagnostics);
		Assert.NotNull(result.Document);
		// .NET URI does not compare fragments, so we need to use OriginalString here
		Assert.Equal("proj://embedded/petstore.yaml#/info", result.Document.Info.Id.OriginalString);
		Assert.Equal("Swagger Petstore", result.Document.Info.Title);
		Assert.Equal("proj://embedded/petstore.yaml#/info/contact", result.Document.Info.Contact?.Id.OriginalString);
		Assert.Equal("apiteam@swagger.io", result.Document.Info.Contact?.Email);
		Assert.Equal("proj://embedded/petstore.yaml#/info/license", result.Document.Info.License?.Id.OriginalString);
		Assert.Equal("https://www.apache.org/licenses/LICENSE-2.0.html", result.Document.Info.License?.Url?.OriginalString);
		Assert.Collection(result.Document.Paths,
			(path) =>
			{
				Assert.Equal("/pets", path.Key);
				Assert.Equal("proj://embedded/petstore.yaml#/paths/~1pets", path.Value.Id.OriginalString);
				Assert.Collection(path.Value.Operations,
					(operation) =>
					{
						Assert.Equal("get", operation.Key);
						Assert.Equal("findPets", operation.Value.OperationId);
						Assert.Equal("proj://embedded/petstore.yaml#/paths/~1pets/get", operation.Value.Id.OriginalString);
						Assert.Collection(operation.Value.Parameters,
							(param) =>
							{
								Assert.Equal("tags", param.Name);
								Assert.Equal("proj://embedded/petstore.yaml#/paths/~1pets/get/parameters/0", param.Id.OriginalString);
								Assert.Equal("tags", param.Name);
								Assert.Equal(Abstractions.ParameterLocation.Query, param.In);
								Assert.Equal("tags to filter by", param.Description);
								Assert.Equal("form", param.Style);
								Assert.NotNull(param.Schema);
								Assert.Equal("proj://embedded/petstore.yaml#/paths/~1pets/get/parameters/0/schema", param.Schema.Id.OriginalString);
								Assert.NotNull(param.Schema.Annotations);
								var schemaType = Assert.Single(param.Schema.Annotations.OfType<TypeKeyword>());
								Assert.Equal(TypeKeyword.Common.Array, schemaType.Value);
								var itemsType = Assert.Single(param.Schema.Annotations.OfType<Specifications.Keywords.Draft2020_12Applicator.ItemsKeyword>());
								Assert.NotNull(itemsType.Schema?.Annotations);
								Assert.Equal("proj://embedded/petstore.yaml#/paths/~1pets/get/parameters/0/schema/items", itemsType.Schema.Id.OriginalString);
								var itemSchemaType = Assert.Single(itemsType.Schema.Annotations.OfType<TypeKeyword>());
								Assert.Equal(TypeKeyword.Common.String, itemSchemaType.Value);
							},
							(param) =>
							{
								Assert.Equal("limit", param.Name);
								Assert.Equal("proj://embedded/petstore.yaml#/paths/~1pets/get/parameters/1", param.Id.OriginalString);
							});
						Assert.NotNull(operation.Value.Responses);
						Assert.Collection(operation.Value.Responses.StatusCodeResponses,
							(response) =>
							{
								Assert.Equal(200, response.Key);
								Assert.Equal("proj://embedded/petstore.yaml#/paths/~1pets/get/responses/200", response.Value.Id.OriginalString);
								Assert.Equal("pet response", response.Value.Description);
								Assert.Empty(response.Value.Headers);
								Assert.NotNull(response.Value.Content);
								Assert.Collection(response.Value.Content,
									(jsonContent) =>
									{
										Assert.Equal("application/json", jsonContent.Key);
										Assert.Equal("proj://embedded/petstore.yaml#/paths/~1pets/get/responses/200/content/application~1json", jsonContent.Value.Id.OriginalString);
										Assert.NotNull(jsonContent.Value.Schema);
										Assert.Equal("proj://embedded/petstore.yaml#/paths/~1pets/get/responses/200/content/application~1json/schema", jsonContent.Value.Schema.Id.OriginalString);
									});
							});
						Assert.NotNull(operation.Value.Responses.Default);
					},
					(operation) =>
					{
						Assert.Equal("post", operation.Key);
						Assert.Equal("addPet", operation.Value.OperationId);
						Assert.Equal("proj://embedded/petstore.yaml#/paths/~1pets/post", operation.Value.Id.OriginalString);
						Assert.Empty(operation.Value.Parameters);
						Assert.NotNull(operation.Value.RequestBody);
						Assert.Equal("proj://embedded/petstore.yaml#/paths/~1pets/post/requestBody", operation.Value.RequestBody.Id.OriginalString);
						Assert.True(operation.Value.RequestBody.Required);
						Assert.NotNull(operation.Value.RequestBody.Content);
						Assert.Collection(operation.Value.RequestBody.Content,
							(jsonContent) =>
							{
								Assert.Equal("application/json", jsonContent.Key);
								Assert.Equal("proj://embedded/petstore.yaml#/paths/~1pets/post/requestBody/content/application~1json", jsonContent.Value.Id.OriginalString);
								Assert.NotNull(jsonContent.Value.Schema);
								Assert.Equal("proj://embedded/petstore.yaml#/components/schemas/NewPet", jsonContent.Value.Schema.Id.OriginalString);
							});
					});
			},
			(path) =>
			{
				Assert.Equal("/pets/{id}", path.Key);
				Assert.Equal("proj://embedded/petstore.yaml#/paths/~1pets~1{id}", path.Value.Id.OriginalString);
			});
	}

	[Fact]
	public void Reports_diagnostics_for_bad_yaml()
	{
		var result = GetOpenApiDocument("bad.yaml");
		Assert.Contains(result.Diagnostics, (d) => d is CouldNotFindTargetNodeDiagnostic && d.Location.Range?.Start.Line == 75);
		Assert.Contains(result.Diagnostics, (d) => d is UnableToParseKeyword parseError && parseError.Keyword == "required" && d.Location.Range?.Start.Line == 26);
	}
}
