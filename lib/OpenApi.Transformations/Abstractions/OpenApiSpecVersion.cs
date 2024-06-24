namespace PrincipleStudios.OpenApi.Transformations.Abstractions;

/// <summary>
/// Specification of the version this document uses
/// </summary>
/// <param name="DocumentType">'openapi' or other format specifier supported by the generator, such as possibly 'swagger'</param>
/// <param name="Version">Specification version used</param>
public record OpenApiSpecVersion(string DocumentType, string Version);
