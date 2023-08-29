using Json.More;
using Json.Pointer;
using PrincipleStudios.OpenApi.Transformations.Abstractions;
using System;
using System.Text.Json.Nodes;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0;

internal class License : IOpenApiLicense
{
	private readonly JsonObject jsonNode;

	public License(Uri id, JsonObject jsonNode)
	{
		Id = id;
		this.jsonNode = jsonNode;
	}

	public string Name => jsonNode["name"]?.GetValue<string>() ?? MissingRequiredFieldDefaults.LicenseName;

	public Uri? Url =>
		jsonNode?["url"]?.GetValue<string>() is string url
		&& Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var result)
			? result
			: null;

	public string? Identifier => null;

	public Uri Id { get; }
}