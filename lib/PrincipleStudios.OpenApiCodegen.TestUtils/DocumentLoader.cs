using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using PrincipleStudios.OpenApi.Transformations;
using System;
using PrincipleStudios.OpenApi.Transformations.Specifications;
using System.IO;

namespace PrincipleStudios.OpenApiCodegen.TestUtils;

public class DocumentLoader
{
	private static readonly YamlDocumentLoader docLoader = new YamlDocumentLoader();

	private static IDocumentReference? DocumentResolver(Uri baseUri, IDocumentReference? currentDocument = null)
	{
		switch (baseUri)
		{
			case { Scheme: "proj", Host: "embedded", LocalPath: var embeddedName }:
				return LoadEmbeddedDocument(baseUri, embeddedName, currentDocument?.Dialect);
			default:
				return null;
		}
	}


	private static IDocumentReference LoadEmbeddedDocument(Uri baseUri, string embeddedName, IJsonSchemaDialect? dialect)
	{
		using var documentStream = typeof(DocumentHelpers).Assembly.GetManifestResourceStream($"PrincipleStudios.OpenApiCodegen.TestUtils.schemas.{embeddedName.Substring(1)}");
		using var sr = new StreamReader(documentStream);
		var result = docLoader.LoadDocument(baseUri, sr, dialect);
		return result;
	}

	public static DocumentRegistry CreateRegistry()
	{
		return new DocumentRegistry(new([DocumentResolver]));
	}
}
