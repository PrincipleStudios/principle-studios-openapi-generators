using Bogus.DataSets;
using Json.Schema;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static PrincipleStudios.OpenApiCodegen.TestUtils.DocumentHelpers;

namespace PrincipleStudios.OpenApi.Transformations;

public class OpenApi3_0ParserShould
{
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
								Assert.Equal("proj://embedded/petstore.yaml#/paths/~1pets/get/parameters/0/schema", param.Schema.BaseUri.OriginalString);
								Assert.NotNull(param.Schema.Keywords);
								var schemaType = Assert.Single(param.Schema.Keywords.OfType<TypeKeyword>());
								Assert.Equal(SchemaValueType.Array, schemaType.Type);
								var itemsType = Assert.Single(param.Schema.Keywords.OfType<ItemsKeyword>());
								Assert.NotNull(itemsType.SingleSchema?.Keywords);
								// TODO: normalize the schema BaseUris
								// Assert.Equal("proj://embedded/petstore.yaml#/paths/~1pets/get/parameters/0/schema/items", itemsType.SingleSchema.BaseUri.OriginalString);
								var itemSchemaType = Assert.Single(itemsType.SingleSchema.Keywords.OfType<TypeKeyword>());
								Assert.Equal(SchemaValueType.String, itemSchemaType.Type);
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
										Assert.Equal("proj://embedded/petstore.yaml#/paths/~1pets/get/responses/200/content/application~1json/schema", jsonContent.Value.Schema.BaseUri.OriginalString);
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
								Assert.Equal("proj://embedded/petstore.yaml#/components/schemas/NewPet", jsonContent.Value.Schema.BaseUri.OriginalString);
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
		// TODO - check more diagnostics here
		Assert.Contains(result.Diagnostics, (d) => d is UnableToParseSchema);
	}
}
