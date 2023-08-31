﻿using Json.More;
using Json.Pointer;
using Json.Schema;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PrincipleStudios.OpenApi.Transformations;

public delegate IDocumentReference? DocumentResolver(Uri baseUri, IDocumentReference? currentDocument);

public class DocumentRegistry
{
	private EvaluationOptions evaluationOptions;

	private record DocumentRegistryEntry(
		IDocumentReference Document,
		IReadOnlyDictionary<string, JsonPointer> Anchors
	);
	private readonly IDictionary<Uri, DocumentRegistryEntry> entries = new Dictionary<Uri, DocumentRegistryEntry>();
	private DocumentResolver? fetch;

	public DocumentRegistry()
	{
		evaluationOptions = new EvaluationOptions()
		{
			SchemaRegistry = { Fetch = SchemaRegistryFetch },
			// TODO - any other settings here?
		};
	}

	private IBaseDocument? SchemaRegistryFetch(Uri uri)
	{
		return ResolveDocument(uri, null);
	}

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
		var uri = document.BaseUri;
		// TODO: should this be a warning and instead just use the retrieval uri?
		if (uri.Fragment is { Length: > 0 }) throw new ArgumentException(Errors.InvalidDocumentBaseUri, nameof(document));

		var visitor = new DocumentRefVisitor();
		visitor.Visit(document.RootNode);

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

	public JsonNode? ResolveNode(IDocumentReference document, Uri refUri) =>
		ResolveNode(refUri.IsAbsoluteUri ? refUri : new Uri(document.BaseUri, refUri), document);

	public JsonNode? ResolveNode(Uri uri) => ResolveNode(uri, relativeDocument: null);

	private JsonNode? ResolveNode(Uri uri, IDocumentReference? relativeDocument)
	{
		var docUri = uri.Fragment is { Length: > 0 }
			? new UriBuilder(uri) { Fragment = "" }.Uri
			: uri;
		var document = InternalResolveDocumentEntry(docUri, relativeDocument);

		if (uri.Fragment is not { Length: > 0 })
			return document.Document.RootNode;

		var element = uri.Fragment.StartsWith("#/")
			// pointer
			? !JsonPointer.TryParse(uri.Fragment, out var pointer)
				? throw new DiagnosticException(InvalidFragmentDiagnostic.Builder())
				: pointer!.TryEvaluate(document.Document.RootNode, out var node)
					? node
					: throw new ResolveNodeException(uri)
			// anchor
			: document.Anchors[uri.Fragment.Substring(1)].TryEvaluate(document.Document.RootNode, out var nodeFromAnchor)
				? nodeFromAnchor
				: throw new ResolveNodeException(uri);
		return element;
	}

	public IDocumentReference ResolveDocument(Uri uri, IDocumentReference? relativeDocument) =>
		InternalResolveDocumentEntry(uri, relativeDocument).Document;

	private DocumentRegistryEntry InternalResolveDocumentEntry(Uri docUri, IDocumentReference? relativeDocument)
	{
		// .NET's Uri type doesn't include the Fragment in equality, so we don't need to check until we fetch
		if (!entries.TryGetValue(docUri, out var document))
		{
			document = InternalFetch(relativeDocument, docUri);
		}

		return document;
	}

	private DocumentRegistryEntry InternalFetch(IDocumentReference? relativeDocument, Uri docUri)
	{
		if (docUri.Fragment is { Length: > 0 })
			docUri = new UriBuilder(docUri) { Fragment = "" }.Uri;

		var document = fetch?.Invoke(docUri, relativeDocument);
		if (document == null)
			throw new ResolveDocumentException(docUri);

		return InternalAddDocument(document);
	}

	public JsonSchema? ResolveSchema(Uri schemaUri, IDocumentReference? relativeDocument)
	{
		var docRef = ResolveDocument(schemaUri, relativeDocument);
		var pointer = JsonPointer.Parse(Uri.UnescapeDataString(schemaUri.Fragment.Substring(1)));
		var schema = docRef.FindSubschema(pointer, SchemaEvaluationOptions);
		return schema;
	}

	public EvaluationOptions SchemaEvaluationOptions => evaluationOptions;

	private class DocumentRefVisitor : JsonNodeVisitor
	{
		public Dictionary<string, JsonPointer> Anchors { get; } = new Dictionary<string, JsonPointer>();
		public Dictionary<string, JsonPointer> BundledSchemas { get; } = new Dictionary<string, JsonPointer>();

		protected override void VisitObject(JsonObject obj, JsonPointer elementPointer)
		{
			if (obj.TryGetPropertyValue("$anchor", out var elem) && elem?.GetValue<string>() is string anchorId)
				Anchors.Add(anchorId, elementPointer);
			if (elementPointer != JsonPointer.Empty && obj.TryGetPropertyValue("$id", out elem) && elem?.GetValue<string>() is string bundledSchemaId)
				BundledSchemas.Add(bundledSchemaId, elementPointer);
			base.VisitObject(obj, elementPointer);
		}
	}
}

public static class JsonDocumentUtils
{
	public static Uri GetDocumentBaseUri(IDocumentReference document) =>
		document.RootNode.GetBaseUri(document.RetrievalUri);


	public static Uri GetBaseUri(this JsonNode? jsonNode, Uri retrievalUri) =>
		jsonNode is JsonObject obj && obj.TryGetPropertyValue("$id", out var id) && id?.GetValue<string>() is string baseId
			? new Uri(retrievalUri, baseId)
			: retrievalUri;

}

public record InvalidFragmentDiagnostic(Location Location) : DiagnosticBase(Location)
{
	public static DiagnosticException.ToDiagnostic Builder() => (Location) => new InvalidFragmentDiagnostic(Location);
}

public record UnresolvedNodeDiagnostic(Location Location) : DiagnosticBase(Location)
{
	public static DiagnosticException.ToDiagnostic Builder() => (Location) => new UnresolvedNodeDiagnostic(Location);
}
