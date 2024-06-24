using Json.Pointer;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using PrincipleStudios.OpenApi.Transformations.Specifications;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Nodes;

namespace PrincipleStudios.OpenApi.Transformations;

public delegate IDocumentReference? DocumentResolver(Uri baseUri, IDocumentReference? currentDocument);
public record NodeMetadata(Uri Id, JsonNode? Node, IDocumentReference Document)
{
	internal static NodeMetadata FromRoot(IDocumentReference documentReference)
	{
		return new NodeMetadata(documentReference.BaseUri, documentReference.RootNode, documentReference);
	}
}


public class DocumentRegistry(DocumentRegistryOptions registryOptions)
{
	private record DocumentRegistryEntry(
		IDocumentReference Document,
		IReadOnlyDictionary<string, JsonPointer> Anchors
	);
	private readonly ICollection<DocumentRegistryEntry> entries = new HashSet<DocumentRegistryEntry>();

	public void AddDocument(IDocumentReference document)
	{
		if (document is null) throw new ArgumentNullException(nameof(document));
		if (!document.RetrievalUri.IsAbsoluteUri) throw new DiagnosticException(InvalidRetrievalUri.Builder(document.RetrievalUri));

		InternalAddDocument(document);
	}

	private DocumentRegistryEntry InternalAddDocument(IDocumentReference document)
	{
		var uri = document.BaseUri;
		if (uri.Fragment is { Length: > 0 }) throw new DiagnosticException(InvalidDocumentBaseUri.Builder(retrievalUri: document.RetrievalUri, baseUri: document.BaseUri));

		if (entries.Any(e => e.Document.BaseUri == uri))
			throw new ArgumentException(string.Format(Errors.DuplicateDocumentBaseUri, uri), nameof(document));

		var visitor = new DocumentRefVisitor();
		visitor.Visit(document.RootNode);

		var result = new DocumentRegistryEntry(document, visitor.Anchors);
		entries.Add(result);
		return result;
	}

	public bool HasDocument(Uri uri)
	{
		return entries.Any(doc => doc.Document.BaseUri == uri);
	}

	public bool TryGetDocument(Uri uri, [NotNullWhen(true)] out IDocumentReference? doc)
	{
		doc = entries.FirstOrDefault(e => e.Document.BaseUri == uri)?.Document;
		return doc != null;
	}

	public JsonNode? ResolveNode(Uri uri, IDocumentReference? relativeDocument = null) => ResolveMetadata(uri, relativeDocument).Node;

	public NodeMetadata ResolveMetadata(Uri uri, IDocumentReference? relativeDocument)
	{
		var registryEntry = InternalResolveDocumentEntry(uri, relativeDocument);
		var fullUri = uri.IsAbsoluteUri ? uri
			: relativeDocument != null ? new Uri(registryEntry.Document.BaseUri, uri)
			// Throw an exception here because this is a problem with the usage of this class, not data
			: throw new InvalidOperationException(Errors.ReceivedRelativeUriWithoutDocument);

		return ResolveFragment(fullUri.Fragment, registryEntry);
	}

	private static NodeMetadata ResolveFragment(string fragment, DocumentRegistryEntry registryEntry)
	{
		// fragments must start with `#` or be empty
		if (fragment is not { Length: > 1 })
			return new(Id: registryEntry.Document.BaseUri, Node: registryEntry.Document.RootNode, Document: registryEntry.Document);

		var uri = new UriBuilder(registryEntry.Document.BaseUri) { Fragment = fragment }.Uri;
		if (!ResolvePointer(uri, registryEntry).TryEvaluate(registryEntry.Document.RootNode, out var node))
			throw new DiagnosticException(CouldNotFindTargetNodeDiagnostic.Builder(uri));

		return new(Id: uri, Node: node, Document: registryEntry.Document);
	}

	private static JsonPointer ResolvePointer(Uri uri, DocumentRegistryEntry registryEntry)
	{
		// Fragments always start with `#`
		if (uri.Fragment is not { Length: > 1 }) return JsonPointer.Empty;
		if (uri.Fragment.StartsWith("#/"))
		{
			if (!JsonPointer.TryParse(Uri.UnescapeDataString(uri.Fragment).Substring(1), out var pointer)) throw new DiagnosticException(InvalidFragmentDiagnostic.Builder(uri.Fragment));
			// needs not-null assertion because we're supporting .NET Standard 2.0, which was pre-NotNullWhenAttribute
			return pointer!;
		}
		else
		{
			if (!registryEntry.Anchors.TryGetValue(uri.Fragment.Substring(1), out var pointer)) throw new DiagnosticException(UnknownAnchorDiagnostic.Builder(uri));
			return pointer;
		}
	}


	public IDocumentReference ResolveDocument(Uri uri, IDocumentReference? relativeDocument) =>
		InternalResolveDocumentEntry(uri, relativeDocument).Document;

	private DocumentRegistryEntry InternalResolveDocumentEntry(Uri uri, IDocumentReference? relativeDocument)
	{
		var docUri = uri.IsAbsoluteUri ? uri
			: relativeDocument != null ? new Uri(relativeDocument.BaseUri, uri)
			// Throw an exception here because this is a problem with the usage of this class, not data
			: throw new InvalidOperationException(Errors.ReceivedRelativeUriWithoutDocument);

		// .NET's Uri type doesn't include the Fragment in equality, so we don't need to check until we fetch
		var document = entries.FirstOrDefault(e => e.Document.BaseUri == docUri)
			?? InternalFetch(relativeDocument, docUri);
		return document;
	}

	private DocumentRegistryEntry InternalFetch(IDocumentReference? relativeDocument, Uri docUri)
	{
		if (docUri.Fragment is { Length: > 0 })
			docUri = new UriBuilder(docUri) { Fragment = "" }.Uri;

		var document = (from resolver in registryOptions.Resolvers
						let doc = resolver(docUri, relativeDocument)
						where doc != null
						select doc).FirstOrDefault();
		if (document == null)
			throw new DiagnosticException(ResolveDocumentDiagnostic.Builder(docUri));

		return InternalAddDocument(document);
	}
	public DiagnosableResult<JsonSchema> ResolveSchema(NodeMetadata node)
	{
		var resolved = node.Node == null ? ResolveMetadata(node.Id, node.Document) : node;
		return JsonSchemaParser.Deserialize(resolved, new(
			Dialect: resolved.Document.Dialect,
			Registry: this
		));
	}

	public Location ResolveLocation(NodeMetadata key)
	{
		var registryEntry = InternalResolveDocumentEntry(key.Document.BaseUri, null);
		if (registryEntry.Document != key.Document) throw new ArgumentException(Errors.DocumentMismatch, nameof(key));
		var fileLocation = key.Document.GetLocation(ResolvePointer(key.Id, registryEntry));
		return new Location(key.Document.RetrievalUri, fileLocation);
	}

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
		document.RootNode.GetBaseUri(document.RetrievalUri, document.Dialect);


	public static Uri GetBaseUri(this JsonNode? jsonNode, Uri retrievalUri, IJsonSchemaDialect dialect) =>
		jsonNode is JsonObject obj && obj.TryGetPropertyValue(dialect.IdField, out var id) && id?.GetValue<string>() is string baseId
			? new Uri(retrievalUri, baseId)
			: retrievalUri;

}

public record InvalidRetrievalUri(Uri RetrievalUri, Location Location) : DiagnosticBase(Location)
{
	public override IReadOnlyList<string> GetTextArguments() => [RetrievalUri.OriginalString];
	public static DiagnosticException.ToDiagnostic Builder(Uri retrievalUri) => (Location) => new InvalidRetrievalUri(retrievalUri, Location);
}

public record InvalidDocumentBaseUri(Uri RetrievalUri, Uri BaseUri, Location Location) : DiagnosticBase(Location)
{
	public override IReadOnlyList<string> GetTextArguments() => [RetrievalUri.OriginalString, BaseUri.OriginalString];
	public static DiagnosticException.ToDiagnostic Builder(Uri retrievalUri, Uri baseUri) => (Location) => new InvalidDocumentBaseUri(retrievalUri, baseUri, Location);
}

public record InvalidFragmentDiagnostic(string ActualFragment, Location Location) : DiagnosticBase(Location)
{
	public override IReadOnlyList<string> GetTextArguments() => [ActualFragment];
	public static DiagnosticException.ToDiagnostic Builder(string actualFragment) => (Location) => new InvalidFragmentDiagnostic(actualFragment, Location);
}

public record InvalidRefDiagnostic(string RefValue, Location Location) : DiagnosticBase(Location)
{
	public override IReadOnlyList<string> GetTextArguments() => [RefValue];
	public static DiagnosticException.ToDiagnostic Builder(string refValue) => (Location) => new InvalidRefDiagnostic(refValue, Location);
}

public record CouldNotFindTargetNodeDiagnostic(Uri Uri, Location Location) : DiagnosticBase(Location)
{
	public override IReadOnlyList<string> GetTextArguments() => [Uri.OriginalString];
	internal static DiagnosticException.ToDiagnostic Builder(Uri uri) => (Location) => new CouldNotFindTargetNodeDiagnostic(uri, Location);
}

public record UnknownAnchorDiagnostic(Uri Uri, Location Location) : DiagnosticBase(Location)
{
	public override IReadOnlyList<string> GetTextArguments() => [Uri.OriginalString];
	internal static DiagnosticException.ToDiagnostic Builder(Uri uri) => (Location) => new UnknownAnchorDiagnostic(uri, Location);
}

public record ResolveDocumentDiagnostic(Uri Uri, Location Location) : DiagnosticBase(Location)
{
	public override IReadOnlyList<string> GetTextArguments() => [Uri.OriginalString];
	internal static DiagnosticException.ToDiagnostic Builder(Uri uri) => (Location) => new ResolveDocumentDiagnostic(uri, Location);
}
