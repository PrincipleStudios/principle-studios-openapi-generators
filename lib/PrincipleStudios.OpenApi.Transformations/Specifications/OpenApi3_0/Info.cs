using Json.More;
using Json.Pointer;
using PrincipleStudios.OpenApi.Transformations.Abstractions;
using System;
using System.Text.Json.Nodes;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0;

internal class Info : IOpenApiInfo
{
	private readonly JsonNode jsonNode;

	public Info(Uri id, JsonNode jsonNode)
	{
		this.Id = id;
		this.jsonNode = jsonNode;
	}

	public string Title => jsonNode?["title"]?.GetValue<string>() ?? MissingRequiredFieldDefaults.InfoTitle;

	public string? Summary => null;

	public string? Description => jsonNode?["description"]?.GetValue<string>();

	public Uri? TermsOfService =>
		jsonNode?["termsOfService"]?.GetValue<string>() is string tos
		&& Uri.TryCreate(tos, UriKind.RelativeOrAbsolute, out var result)
			? result
			: null;

	public IOpenApiContact? Contact => jsonNode?["contact"] is JsonObject obj
		? new Contact(Id.AppendPointer("contact"), obj)
		: null;

	public IOpenApiLicense? License => jsonNode?["license"] is JsonObject obj
		? new License(Id.AppendPointer("license"), obj)
		: null;

	public string Version => jsonNode?["version"]?.GetValue<string>() ?? MissingRequiredFieldDefaults.InfoVersion;

	public Uri Id { get; }
}