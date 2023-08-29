using Json.More;
using Json.Pointer;
using PrincipleStudios.OpenApi.Transformations.Abstractions;
using System;
using System.Text.Json.Nodes;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0;

internal class Contact : IOpenApiContact
{
	private readonly JsonObject jsonNode;

	public Contact(Uri uri, JsonObject jsonNode)
	{
		Id = uri;
		this.jsonNode = jsonNode;
	}

	public string? Name => jsonNode?["name"]?.GetValue<string>();

	public Uri? Url =>
		jsonNode?["url"]?.GetValue<string>() is string url
		&& Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var result)
			? result
			: null;

	public string? Email => jsonNode?["email"]?.GetValue<string>();

	public Uri Id { get; }
}