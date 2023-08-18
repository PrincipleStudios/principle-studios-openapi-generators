using Json.Schema;
using Json.Schema.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Yaml2JsonNode;

namespace PrincipleStudios.OpenApiCodegen.TestUtils
{
	public static class DocumentHelpers
	{
		static DocumentHelpers()
		{
			Json.Schema.OpenApi.Vocabularies.Register();
		}

		public static OpenApiDocument GetDocument(string name)
		{
			using var documentStream = typeof(DocumentHelpers).Assembly.GetManifestResourceStream($"PrincipleStudios.OpenApiCodegen.TestUtils.schemas.{name}");
			using var sr = new StreamReader(documentStream);
			var yamlStream = new YamlDotNet.RepresentationModel.YamlStream();
			yamlStream.Load(sr);

			var jsonSchema = JsonSerializer.Deserialize<JsonSchema>(yamlStream.Documents[0].ToJsonNode(), new JsonSerializerOptions
			{
				Converters = { new ValidatingJsonConverter() }
			});

			documentStream.Position = 0;

			var reader = new OpenApiStreamReader();
			return reader.Read(documentStream, out var openApiDiagnostic);
		}

		public static string GetDocumentString(string name)
		{
			using var documentStream = typeof(DocumentHelpers).Assembly.GetManifestResourceStream($"PrincipleStudios.OpenApiCodegen.TestUtils.schemas.{name}");
			using var reader = new System.IO.StreamReader(documentStream!);
			return reader.ReadToEnd();
		}

		public static Microsoft.OpenApi.OpenApiSpecVersion ToSpecVersion(string? inputVersion)
		{
			switch (inputVersion)
			{
				case string version when version == "2.0":
					return Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0;

				case string version when version.StartsWith("3.0"):
					return Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;

				default:
					throw new NotSupportedException(inputVersion);
			}
		}
	}
}
