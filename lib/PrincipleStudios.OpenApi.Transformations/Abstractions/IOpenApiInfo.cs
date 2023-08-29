using System;

namespace PrincipleStudios.OpenApi.Transformations.Abstractions;

/// <summary>
/// See https://spec.openapis.org/oas/v3.1.0#info-object
/// </summary>
public interface IOpenApiInfo : IReferenceableDocument
{
	public string Title { get; }
	public string? Summary { get; }
	public string? Description { get; }
	public Uri? TermsOfService { get; }
	public IOpenApiContact? Contact { get; }
	public IOpenApiLicense? License { get; }
	public string Version { get; }
}
