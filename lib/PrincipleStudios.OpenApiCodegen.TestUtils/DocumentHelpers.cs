﻿using Json.Schema;
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
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;

namespace PrincipleStudios.OpenApiCodegen.TestUtils
{
	public static class DocumentHelpers
	{
		private static readonly YamlDocumentLoader docLoader = new YamlDocumentLoader();

		static DocumentHelpers()
		{
			Json.Schema.OpenApi.Vocabularies.Register();
		}

		public static OpenApiDocument GetDocument(string name)
		{
			GetDocumentReference(name);

			using (var documentStream = typeof(DocumentHelpers).Assembly.GetManifestResourceStream($"PrincipleStudios.OpenApiCodegen.TestUtils.schemas.{name}"))
			{
				var reader = new OpenApiStreamReader();
				return reader.Read(documentStream, out var openApiDiagnostic);
			}
		}

		public static IDocumentReference GetDocumentReference(string name)
		{
			var uri = new Uri($"proj://{name}");
			using (var documentStream = typeof(DocumentHelpers).Assembly.GetManifestResourceStream($"PrincipleStudios.OpenApiCodegen.TestUtils.schemas.{name}"))
			{
				return docLoader.LoadDocument(uri, documentStream);
			}
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
