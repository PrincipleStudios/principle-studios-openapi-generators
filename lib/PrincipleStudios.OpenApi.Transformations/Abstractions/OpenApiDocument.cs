using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations.Abstractions;

/// <summary>
/// See https://spec.openapis.org/oas/v3.1.0#openapi-object
/// </summary>
public record OpenApiDocument(
	Uri Id,
	OpenApiSpecVersion OpenApiSpecVersion,
	OpenApiInfo Info,
	Uri JsonSchemaDialect,
	IReadOnlyDictionary<string, OpenApiPath> Paths
) : IReferenceableDocument
{
	// TODO:
	// servers?
	// webhooks?

	// We don't use the following internally (yet?) directly, so we probably won't map them... at least for now
	// components
	// security
	// tags
	// externalDocs
}
