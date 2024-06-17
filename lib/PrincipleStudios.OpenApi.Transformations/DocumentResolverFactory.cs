using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;

namespace PrincipleStudios.OpenApi.Transformations;

public record DocumentRegistryOptions(
	IReadOnlyList<DocumentResolver> Resolvers
);

public static class DocumentResolverFactory
{
	private static readonly YamlDocumentLoader docLoader = new YamlDocumentLoader();

	public static DocumentResolver RelativeFrom(IDocumentReference documentReference)
	{
		// TODO
		return (baseUri, _) =>
		{
			var relative = documentReference.BaseUri.MakeRelativeUri(baseUri);
			if (relative.IsAbsoluteUri) return null;
			var path = new Uri(documentReference.RetrievalUri, relative);
			if (path.Scheme != "file") return null;
			try
			{
				using var sr = new StreamReader(path.LocalPath);
				return docLoader.LoadDocument(baseUri, sr, documentReference.Dialect);
			}
			catch
			{
				// return null;
				throw;
			}
		};
	}

	public static readonly DocumentResolver RelativeFromCurrentDocument =
		// TODO
		(_, _) => null;

	public static (IDocumentReference, DocumentRegistry) FromInitialDocumentInMemory(Uri uri, string documentContents, DocumentRegistryOptions resolverOptions)
	{
		using var sr = new StringReader(documentContents);
		var doc = docLoader.LoadDocument(uri, sr, null);

		var registry = new DocumentRegistry(resolverOptions with
		{
			Resolvers = Enumerable.Concat([RelativeFrom(doc)], resolverOptions.Resolvers).ToArray()
		});
		registry.AddDocument(doc);
		return (doc, registry);
	}
}
