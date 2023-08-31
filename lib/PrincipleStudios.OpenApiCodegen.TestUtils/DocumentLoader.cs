using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using PrincipleStudios.OpenApi.Transformations;
using System;
using System.Collections.Generic;
using System.Text;
using Bogus.DataSets;
using PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_1;
using PrincipleStudios.OpenApi.Transformations.Specifications;
using System.Linq;
using PrincipleStudios.OpenApi.Transformations.Abstractions;

namespace PrincipleStudios.OpenApiCodegen.TestUtils;

public class DocumentLoader
{
	private static readonly YamlDocumentLoader docLoader = new YamlDocumentLoader();

	private static IDocumentReference? DocumentResolver(Uri baseUri, IDocumentReference? currentDocument = null)
	{
		switch (baseUri)
		{
			case { Scheme: "proj", Host: "embedded", LocalPath: var embeddedName }:
				return LoadEmbeddedDocument(baseUri, embeddedName);
			default:
				return null;
		}
	}


	private static IDocumentReference LoadEmbeddedDocument(Uri baseUri, string embeddedName)
	{
		using var documentStream = typeof(DocumentHelpers).Assembly.GetManifestResourceStream($"PrincipleStudios.OpenApiCodegen.TestUtils.schemas.{embeddedName.Substring(1)}");
		var result = docLoader.LoadDocument(baseUri, documentStream);
		return result;
	}

	public static DocumentRegistry CreateRegistry()
	{
		return new DocumentRegistry { Fetch = DocumentResolver };
	}
}
