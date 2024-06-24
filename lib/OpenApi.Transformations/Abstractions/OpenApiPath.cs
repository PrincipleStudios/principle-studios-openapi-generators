using System;
using System.Collections.Generic;
using System.Net.Http;

namespace PrincipleStudios.OpenApi.Transformations.Abstractions;

/// <summary>
/// See https://spec.openapis.org/oas/v3.1.0#pathItemObject
/// </summary>
public record OpenApiPath(
	Uri Id,
	string? Summary,
	string? Description,

	IReadOnlyDictionary<string, OpenApiOperation> Operations
) : IReferenceableDocument
{
	// TODO: parameters
	// servers?
}