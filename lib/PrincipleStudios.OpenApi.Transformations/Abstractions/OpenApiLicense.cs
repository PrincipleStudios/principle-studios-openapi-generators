using System;

namespace PrincipleStudios.OpenApi.Transformations.Abstractions;

/// <summary>
/// See https://spec.openapis.org/oas/v3.1.0#license-object
/// </summary>
public record OpenApiLicense(
	Uri Id,
	string Name,
	Uri? Url,
	string? Identifier
) : IReferenceableDocument;
