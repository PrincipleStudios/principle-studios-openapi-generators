using Microsoft.OpenApi.Readers;
using System;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using PrincipleStudios.OpenApi.Transformations.Specifications;
using PrincipleStudios.OpenApi.Transformations.Abstractions;

namespace PrincipleStudios.OpenApiCodegen.TestUtils
{
	public static class DocumentHelpers
	{
		public static ParseResult<OpenApi.Transformations.Abstractions.OpenApiDocument> GetOpenApiDocument(string name)
		{
			var registry = DocumentLoader.CreateRegistry();
			var documentReference = GetDocumentReference(registry, name);
			var parseResult = CommonParsers.DefaultParsers.Parse(documentReference, registry);
			if (parseResult == null)
				throw new InvalidOperationException("No parser found");

			return parseResult;
		}

		public static Microsoft.OpenApi.Models.OpenApiDocument GetDocument(string name)
		{
			GetOpenApiDocument(name);

			using (var documentStream = typeof(DocumentHelpers).Assembly.GetManifestResourceStream($"PrincipleStudios.OpenApiCodegen.TestUtils.schemas.{name}"))
			{
				var reader = new OpenApiStreamReader();
				return reader.Read(documentStream, out var openApiDiagnostic);
			}
		}

		public static IDocumentReference GetDocumentReference(string name)
			=> GetDocumentReference(DocumentLoader.CreateRegistry(), name);

		public static IDocumentReference GetDocumentReference(OpenApi.Transformations.DocumentRegistry registry, string name)
		{
			var uri = new Uri($"proj://embedded/{name}");
			return GetDocumentByUri(registry, uri);
		}

		public static IDocumentReference GetDocumentByUri(Uri uri)
		{
			return GetDocumentByUri(DocumentLoader.CreateRegistry(), uri);
		}

		public static IDocumentReference GetDocumentByUri(OpenApi.Transformations.DocumentRegistry registry, Uri uri)
		{
			return registry.ResolveDocument(new UriBuilder(uri) { Fragment = "" }.Uri, null) ?? throw new InvalidOperationException("Embeded document not found");
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
