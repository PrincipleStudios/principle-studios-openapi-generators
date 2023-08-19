using Json.More;
using Json.Pointer;
using Json.Schema;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using PrincipleStudios.OpenApi.Transformations.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace PrincipleStudios.OpenApi.Transformations;

public record RelativeDocument(IDocumentReference Document, Uri RelativeUri);
public delegate IDocumentReference? DocumentResolver(Uri baseUri, RelativeDocument? relativeDocument);

public class DocumentRegistry
{
	private record DocumentRegistryEntry(
		IDocumentReference Document,
		IReadOnlyDictionary<string, JsonPointer> Anchors
	);
	private readonly IDictionary<Uri, DocumentRegistryEntry> entries = new Dictionary<Uri, DocumentRegistryEntry>();
	private DocumentResolver? fetch;

	public DocumentResolver Fetch
	{
		get => fetch ?? ((uri, relative) => null);
		set => fetch = value;
	}

	public void AddDocument(IDocumentReference document)
	{
		if (document is null) throw new ArgumentNullException(nameof(document));
		if (!document.RetrievalUri.IsAbsoluteUri) throw new ArgumentException(Errors.InvalidRetrievalUri, nameof(document));

		InternalAddDocument(document);
	}

	private DocumentRegistryEntry InternalAddDocument(IDocumentReference document)
	{
		var uri = GetDocumentBaseUri(document);
		// TODO: should this be a warning and instead just use the retrieval uri?
		if (uri.Fragment is { Length: > 0 }) throw new ArgumentException(Errors.InvalidDocumentBaseUri, nameof(document));

		var visitor = new DocumentRefVisitor();
		visitor.Visit(document.RootElement);

		var result = new DocumentRegistryEntry(document, visitor.Anchors);
		entries.Add(uri, result);
		return result;
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

		var element = uri.Fragment.StartsWith("#/")
			// pointer
			? JsonPointer.Parse(uri.Fragment).Evaluate(document.Document.RootElement)
			// anchor
			: document.Anchors[uri.Fragment.Substring(1)].Evaluate(document.Document.RootElement);
		if (element is not JsonElement result)
			throw new ResolveNodeException(uri);
		return result;
	}

	private DocumentRegistryEntry InternalFetch(RelativeDocument? relativeDocument, Uri docUri)
	{
		var document = fetch?.Invoke(docUri, relativeDocument);
		if (document == null)
			throw new ResolveDocumentException(docUri);

		return InternalAddDocument(document);
	}

	private static Uri GetDocumentBaseUri(IDocumentReference document) =>
		document.RootElement.ValueKind == JsonValueKind.Object && document.RootElement.TryGetProperty("$id", out var id) && id.GetString() is string baseId
			? new Uri(document.RetrievalUri, baseId)
			: document.RetrievalUri;

	private class DocumentRefVisitor : JsonElementVisitor
	{
		public Dictionary<string, JsonPointer> Anchors { get; } = new Dictionary<string, JsonPointer>();
		public Dictionary<string, JsonPointer> BundledSchemas { get; } = new Dictionary<string, JsonPointer>();

		protected override void VisitObject(JsonElement element, JsonPointer elementPointer)
		{
			if (element.TryGetProperty("$anchor", out var elem) && elem.ToString() is string anchorId)
				Anchors.Add(anchorId, elementPointer);
			if (elementPointer != JsonPointer.Empty && element.TryGetProperty("$id", out elem) && elem.ToString() is string bundledSchemaId)
				BundledSchemas.Add(bundledSchemaId, elementPointer);
			base.VisitObject(element, elementPointer);
		}
	}
}
