using PrincipleStudios.OpenApi.Transformations.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0;

internal class OpenApi3_0Document : IOpenApiDocument
{
	private JsonNode? rootNode;

	public OpenApi3_0Document(JsonNode? rootNode)
	{
		this.rootNode = rootNode;
	}

	public OpenApiSpecVersion OpenApiSpecVersion => throw new NotImplementedException();

	public IOpenApiInfo Info => throw new NotImplementedException();

	public Uri JsonSchemaDialect => throw new NotImplementedException();

	public Uri Id => throw new NotImplementedException();
}
