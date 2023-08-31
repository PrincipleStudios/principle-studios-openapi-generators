using Json.Schema;
using System;

namespace PrincipleStudios.OpenApi.Transformations.Abstractions;

/// <summary>
/// See https://spec.openapis.org/oas/v3.1.0#mediaTypeObject
/// </summary>
public record OpenApiMediaTypeObject(
	Uri Id,
	JsonSchema? Schema
) : IReferenceableDocument;
