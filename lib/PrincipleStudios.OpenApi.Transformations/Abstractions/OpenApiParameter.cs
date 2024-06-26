using System;
using PrincipleStudios.OpenApi.Transformations.Specifications;

namespace PrincipleStudios.OpenApi.Transformations.Abstractions;

/// <summary>
/// See https://spec.openapis.org/oas/v3.1.0#parameterObject
/// </summary>
public record OpenApiParameter(
	Uri Id,

	string Name,
	ParameterLocation In,
	string? Description,
	bool Required,
	bool Deprecated,
	bool AllowEmptyValue,

	// TODO - there's a bunch of ways to serialize parameters, this is only the simplest way
	string Style,
	bool Explode,
	JsonSchema? Schema
) : IReferenceableDocument
{
}

public enum ParameterLocation
{
	Query,
	Header,
	Path,
	Cookie,
}