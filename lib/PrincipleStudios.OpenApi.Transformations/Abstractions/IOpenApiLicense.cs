using System;

namespace PrincipleStudios.OpenApi.Transformations.Abstractions;

/// <summary>
/// See https://spec.openapis.org/oas/v3.1.0#license-object
/// </summary>
public interface IOpenApiLicense : IReferenceableDocument
{
	public string? Name { get; }
	public Uri? Url { get; }
	public string? Identifier { get; }
}