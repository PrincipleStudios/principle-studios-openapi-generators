using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using PrincipleStudios.OpenApi.Transformations;
using System;
using System.Collections.Generic;
using System.Text;
using Bogus.DataSets;

namespace PrincipleStudios.OpenApiCodegen.TestUtils;

internal class DocumentLoader
{
	private static readonly YamlDocumentLoader docLoader = new YamlDocumentLoader();

	private static IDocumentReference? DocumentResolver(Uri baseUri, RelativeDocument? relativeDocument = null)
	{
		switch (baseUri)
		{
			case { Scheme: "proj", Host: "embedded", LocalPath: var embeddedName }:
				using (var documentStream = typeof(DocumentHelpers).Assembly.GetManifestResourceStream($"PrincipleStudios.OpenApiCodegen.TestUtils.schemas.{embeddedName.Substring(1)}"))
				{
					return docLoader.LoadDocument(baseUri, documentStream);
				}
			default:
				return null;
		}
	}

	public static DocumentRegistry CreateRegistry()
	{
		return new DocumentRegistry { Fetch = DocumentResolver };
	}
}
