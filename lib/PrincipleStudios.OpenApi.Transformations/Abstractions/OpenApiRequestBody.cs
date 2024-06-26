using System;
using System.Collections.Generic;

namespace PrincipleStudios.OpenApi.Transformations.Abstractions;

/// <summary>
/// See https://spec.openapis.org/oas/v3.1.0#requestBodyObject
/// </summary>
public record OpenApiRequestBody(
	Uri Id,
	string? Description,
	IReadOnlyDictionary<string, OpenApiMediaTypeObject>? Content,
	bool Required
) : IReferenceableDocument;
