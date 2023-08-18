using Json.Pointer;
using Json.Schema;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PrincipleStudios.OpenApi.Transformations;

internal record DocumentRegistryEntry(
	DocumentTypes.IDocumentReference Document
);

public class DocumentRegistry
{
	private readonly IDictionary<Uri, DocumentRegistryEntry> entries = new Dictionary<Uri, DocumentRegistryEntry>();
	private Func<Uri, DocumentTypes.IDocumentReference?>? fetch;

	public Func<Uri, DocumentTypes.IDocumentReference?> Fetch
	{
		get
		{
			return fetch ?? ((uri) => null);
		}
		set
		{
			fetch = value;
		}
	}

	public void AddDocument(Uri uri, DocumentTypes.IDocumentReference document)
	{
		if (uri is null) throw new ArgumentNullException(nameof(uri));
		if (uri.Fragment is { Length: > 0 }) throw new ArgumentException(Errors.InvalidDocumentBaseUri, nameof(uri));
		if (document is null) throw new ArgumentNullException(nameof(document));

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

	/// <summary>
	/// Resolves the given JsonElement or throws
	/// </summary>
	/// <param name="uri"></param>
	/// <returns>The </returns>
	/// <exception cref="ResolveDocumentException"></exception>
	/// <exception cref="ResolveNodeException"></exception>
	public System.Text.Json.JsonElement ResolveNode(Uri uri)
	{
		var docUri = uri.Fragment is { Length: > 0 }
			? new UriBuilder(uri) { Fragment = "" }.Uri
			: uri;

		if (!entries.TryGetValue(docUri, out var document))
		{
			document = new(fetch?.Invoke(docUri) ?? throw new ResolveDocumentException(docUri));
			AddDocument(docUri, document);
		}

		if (uri.Fragment is not { Length: > 0 })
			return document.Document.RootElement;

		var element = JsonPointer.Parse(uri.Fragment).Evaluate(document.Document.RootElement);
		if (element is not JsonElement result)
			throw new ResolveNodeException(uri);
		return result;
	}
}
