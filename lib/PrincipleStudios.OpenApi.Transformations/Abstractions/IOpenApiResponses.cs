using System;
using System.Collections.Generic;

namespace PrincipleStudios.OpenApi.Transformations.Abstractions;

/// <summary>
/// See https://spec.openapis.org/oas/v3.1.0#responsesObject
/// </summary>
public record OpenApiResponses(
	Uri Id,
	OpenApiResponse? Default,
	IReadOnlyDictionary<int, OpenApiResponse> StatusCodeResponses
) : IReferenceableDocument;
