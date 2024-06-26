using Json.Pointer;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations.Abstractions;


/// <summary>
/// Includes defaults for required fields - for when OpenAPI validation is failing, but we still are "doing our best" to generate
/// 
/// </summary>
// TODO - do we want to swap these out so that it captures the id in exceptions?
internal static class MissingRequiredFieldDefaults
{
	public static string InfoTitle => "";
	public static string InfoVersion => "0";
	public static string LicenseName => "N/A";

	public static string OperationTag => "unknown-tag";
	public static string ParameterName => "unknown-param";
	public static string HeaderName => "unknown-header";
	public static string ResponseDescription => "unknown-description";

	private record PlaceholderInfo(Uri Id) : OpenApiInfo(
		Id,
		Title: InfoTitle,
		Summary: null,
		Description: null,
		TermsOfService: null,
		Contact: null,
		License: null,
		Version: InfoVersion
	);

	public static OpenApiInfo ConstructPlaceholderInfo(Uri id) => new PlaceholderInfo(id);

	/// <summary>
	/// Path has no required properties
	/// </summary>
	private record EmptyPath(Uri Id) : OpenApiPath(
		Id,
		Summary: null,
		Description: null,
		Operations: new Dictionary<string, OpenApiOperation>()
	);
	public static OpenApiPath ConstructPlaceholderPath(Uri id) => new EmptyPath(id);

	private record PlaceholderOperation(Uri Id) : OpenApiOperation(
		Id,
		Tags: Array.Empty<string>(),
		Summary: null,
		Description: null,
		OperationId: null,
		Parameters: Array.Empty<OpenApiParameter>(),
		RequestBody: null,
		Responses: null,
		Deprecated: false
	);
	public static OpenApiOperation ConstructPlaceholderOperation(Uri id) => new PlaceholderOperation(id);

	private record PlaceholderParameter(Uri Id) : OpenApiParameter(
		Id,
		Name: ParameterName,
		In: ParameterLocation.Query,
		Description: null,
		Required: false,
		Deprecated: false,
		AllowEmptyValue: false,
		Style: "form",
		Explode: false,
		Schema: null
	);
	public static OpenApiParameter ConstructPlaceholderParameter(Uri id) => new PlaceholderParameter(id);

	private record PlaceholderHeaderParameter(Uri Id) : OpenApiParameter(
		Id,
		Name: HeaderName,
		In: ParameterLocation.Query,
		Description: null,
		Required: false,
		Deprecated: false,
		AllowEmptyValue: false,
		Style: "form",
		Explode: false,
		Schema: null
	);
	public static OpenApiParameter ConstructPlaceholderHeaderParameter(Uri id) => new PlaceholderHeaderParameter(id);

	private record PlaceholderMediaTypeObject(Uri Id) : OpenApiMediaTypeObject(
		Id,
		Schema: null
	);
	public static OpenApiMediaTypeObject ConstructPlaceholderMediaTypeObject(Uri id) => new PlaceholderMediaTypeObject(id);

	private record PlaceholderResponse(Uri Id) : OpenApiResponse(
		Id,
		Description: ResponseDescription,
		Headers: Array.Empty<OpenApiParameter>(),
		Content: new Dictionary<string, OpenApiMediaTypeObject>()
	);
	public static OpenApiResponse ConstructPlaceholderResponse(Uri id) => new PlaceholderResponse(id);
}
