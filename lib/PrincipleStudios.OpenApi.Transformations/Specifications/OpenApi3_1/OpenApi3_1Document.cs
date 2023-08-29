using PrincipleStudios.OpenApi.Transformations.Abstractions;
using System;
using System.Text.Json.Nodes;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_1;

public class OpenApi3_1Document : IOpenApiDocument
{
	// TODO - does this spec version need to be flxible on the patch version?
	public static readonly OpenApiSpecVersion specVersion = new OpenApiSpecVersion("openapi", "3.1.0");
	private JsonNode? rootNode;

	public OpenApi3_1Document(JsonNode? rootNode)
	{
		this.rootNode = rootNode;
	}

	public OpenApiSpecVersion OpenApiSpecVersion => specVersion;

	// TODO - remove this pragma
#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
	public IOpenApiInfo Info => throw new NotImplementedException();

	// Not required, default for 3.1 is https://spec.openapis.org/oas/3.1/dialect/base
	public Uri JsonSchemaDialect => throw new NotImplementedException();

	public Uri Id => throw new NotImplementedException();
}