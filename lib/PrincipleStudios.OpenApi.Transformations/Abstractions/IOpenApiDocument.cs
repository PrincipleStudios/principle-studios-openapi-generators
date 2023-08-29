using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations.Abstractions;

/// <summary>
/// See https://spec.openapis.org/oas/v3.1.0#openapi-object
/// </summary>
public interface IOpenApiDocument : IReferenceableDocument
{
	public OpenApiSpecVersion OpenApiSpecVersion { get; }
	public IOpenApiInfo Info { get; }

	public Uri JsonSchemaDialect { get; }

	// servers?
	// webhooks?
	// paths?
	// components?
	// security?
	// tags?
	// externalDocs?
}
