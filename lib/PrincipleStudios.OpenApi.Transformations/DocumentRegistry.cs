using Json.More;
using Json.Pointer;
using Json.Schema;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PrincipleStudios.OpenApi.Transformations;

public record RelativeDocument(IDocumentReference Document, Uri RelativeUri);
public delegate IDocumentReference? DocumentResolver(Uri baseUri, RelativeDocument? relativeDocument);

internal record DocumentRegistryEntry(
	IDocumentReference Document
);

public class DocumentRegistry
{
	private readonly IDictionary<Uri, DocumentRegistryEntry> entries = new Dictionary<Uri, DocumentRegistryEntry>();
	private DocumentResolver? fetch;

	public DocumentResolver Fetch
	{
		get
		{
			return fetch ?? ((uri, relative) => null);
		}
		set
		{
			fetch = value;
		}
	}

	public void AddDocument(IDocumentReference document)
	{
		if (document is null) throw new ArgumentNullException(nameof(document));
		var uri = GetDocumentBaseUri(document);
		// TODO: should this be a warning and instead just use the retrieval uri?
		if (uri.Fragment is { Length: > 0 }) throw new ArgumentException(Errors.InvalidDocumentBaseUri, nameof(document));

		AddDocument(uri, new DocumentRegistryEntry(document));
	}

	private void AddDocument(Uri uri, DocumentRegistryEntry document)
	{
		if (uri is null) throw new ArgumentNullException(nameof(uri));
		if (uri.Fragment is { Length: > 0 }) throw new ArgumentException(Errors.InvalidDocumentBaseUri, nameof(uri));
		if (document is null) throw new ArgumentNullException(nameof(document));

		entries.Add(uri, document);
	}

	public bool HasDocument(Uri uri)
	{
		var docUri = new UriBuilder(uri) { Fragment = "" }.Uri;
		return entries.ContainsKey(docUri);
	}

	public IEnumerable<Uri> RegisteredDocumentIds => entries.Keys;

	public JsonElement ResolveNode(IDocumentReference document, Uri refUri) =>
		ResolveNode(refUri.IsAbsoluteUri ? refUri : new Uri(GetDocumentBaseUri(document), refUri), new(document, refUri));

	public JsonElement ResolveNode(Uri uri) => ResolveNode(uri, relativeDocument: null);

	private JsonElement ResolveNode(Uri uri, RelativeDocument? relativeDocument)
	{
		var docUri = uri.Fragment is { Length: > 0 }
			? new UriBuilder(uri) { Fragment = "" }.Uri
			: uri;

		if (!entries.TryGetValue(docUri, out var document))
		{
			document = InternalFetch(relativeDocument, docUri);
		}

		if (uri.Fragment is not { Length: > 0 })
			return document.Document.RootElement;

		var element = JsonPointer.Parse(uri.Fragment).Evaluate(document.Document.RootElement);
		if (element is not JsonElement result)
			throw new ResolveNodeException(uri);
		return result;
	}

	private DocumentRegistryEntry InternalFetch(RelativeDocument? relativeDocument, Uri docUri)
	{
		var document = fetch?.Invoke(docUri, relativeDocument);
		if (document == null)
			throw new ResolveDocumentException(docUri);
		var result = new DocumentRegistryEntry(document);
		AddDocument(GetDocumentBaseUri(document), result);
		return result;
	}

	private static Uri GetDocumentBaseUri(IDocumentReference document) =>
		document.RootElement.ValueKind == JsonValueKind.Object && document.RootElement.TryGetProperty("$id", out var id) && id.GetString() is string baseId
			? new Uri(document.RetrievalUri, baseId)
			: document.RetrievalUri;
}
