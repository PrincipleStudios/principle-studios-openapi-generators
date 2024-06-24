using System;
using System.Collections.Generic;

namespace PrincipleStudios.OpenApi.Transformations.Abstractions;

/// <summary>
/// See https://spec.openapis.org/oas/v3.1.0#responseObject
/// </summary>
public record OpenApiResponse(
	Uri Id,
	string Description,
	IReadOnlyList<OpenApiParameter> Headers,
	IReadOnlyDictionary<string, OpenApiMediaTypeObject>? Content

) : IReferenceableDocument
{
	// links?
}