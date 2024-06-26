using Microsoft.OpenApi.Models;
using System;

namespace PrincipleStudios.OpenApi.Transformations.Abstractions;

/// <summary>
/// See https://spec.openapis.org/oas/v3.1.0#info-object
/// </summary>
public record OpenApiInfo(
	Uri Id,
	string Title,
	string? Summary,
	string? Description,
	Uri? TermsOfService,
	OpenApiContact? Contact,
	OpenApiLicense? License,
	string Version
) : IReferenceableDocument
{
}
