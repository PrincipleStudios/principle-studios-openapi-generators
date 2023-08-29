using Json.More;
using Json.Pointer;
using PrincipleStudios.OpenApi.Transformations.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0;

internal class OpenApi3_0Document : IOpenApiDocument
{
	public static readonly Uri jsonSchemaDialect = new Uri("https://spec.openapis.example.org/oas/3.0/dialect/base");
	private JsonNode rootNode;

	public OpenApi3_0Document(Uri id, JsonNode rootNode)
	{
		this.Id = id;
		this.rootNode = rootNode;
	}

	public OpenApiSpecVersion OpenApiSpecVersion => new OpenApiSpecVersion("openapi", rootNode["openapi"]?.GetValue<string>() ?? "3.0.3");

	public IOpenApiInfo Info => rootNode["info"] is JsonObject obj
		? new Info(Id.AppendPointer("info"), obj)
		: new MissingRequiredFieldDefaults.PlaceholderInfo(Id.AppendPointer("info"));

	public Uri JsonSchemaDialect => jsonSchemaDialect;

	public Uri Id { get; }
}
