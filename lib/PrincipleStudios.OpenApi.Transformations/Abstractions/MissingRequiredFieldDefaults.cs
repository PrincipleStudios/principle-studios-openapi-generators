using Json.Pointer;
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

	public class PlaceholderInfo : IOpenApiInfo
	{
		public PlaceholderInfo(Uri id)
		{
			Id = id;
		}

		public string Title => InfoTitle;

		public string? Summary => null;

		public string? Description => null;

		public Uri? TermsOfService => null;

		public IOpenApiContact? Contact => null;

		public IOpenApiLicense? License => null;

		public string Version => InfoVersion;

		public Uri Id { get; }
	}
}
