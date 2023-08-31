using System;

namespace PrincipleStudios.OpenApi.Transformations.Abstractions;

/// <summary>
/// See https://spec.openapis.org/oas/v3.1.0#contactObject
/// </summary>
public record OpenApiContact(
	Uri Id,
	string? Name,
	Uri? Url,
	string? Email
) : IReferenceableDocument;
