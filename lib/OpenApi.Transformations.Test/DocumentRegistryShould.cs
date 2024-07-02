using Json.More;
using Moq;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApi.Transformations;

public class DocumentRegistryShould
{
	private static Bogus.DataSets.Internet internetDataSet = new Bogus.DataSets.Internet();
	private DocumentRegistryOptions DefaultOptions() => new([]);

	[Fact]
	public void Throws_if_a_document_cannot_be_located_without_a_fetch_function()
	{
		var target = new DocumentRegistry(DefaultOptions());
		var documentId = new Uri(internetDataSet.UrlWithPath());

		var diag = AssertThrowsDiagnostic<ResolveDocumentDiagnostic>(() => target.ResolveMetadataNode(documentId));
		Assert.Equal(documentId, diag.Uri);
	}


	[Fact]
	public void Throws_if_a_document_cannot_be_located_with_a_fetch_function()
	{
		var mockFetch = new Mock<DocumentResolver>();
		mockFetch.Setup(a => a(It.IsAny<Uri>(), It.IsAny<IDocumentReference?>())).Returns((IDocumentReference?)null);
		var target = new DocumentRegistry(DefaultOptions() with { Resolvers = [mockFetch.Object] });

		var documentId = new Uri(internetDataSet.UrlWithPath());

		var diag = AssertThrowsDiagnostic<ResolveDocumentDiagnostic>(() => target.ResolveMetadataNode(documentId));
		Assert.Equal(documentId, diag.Uri);
	}

	[Fact]
	public void Allow_documents_to_be_added()
	{
		var target = new DocumentRegistry(DefaultOptions());
		var rootJson = "foo".AsJsonElement();
		CreateDocument(rootJson, out var documentMock, out var documentId);
		target.AddDocument(documentMock.Object);

		Assert.True(target.HasDocument(documentId));
	}

	[Fact]
	public void Allow_documents_to_be_added_with_a_different_base_uri()
	{
		var target = new DocumentRegistry(DefaultOptions());
		var documentId = new Uri(internetDataSet.UrlWithPath());
		var rootJson = new Dictionary<string, object> { ["$id"] = documentId }.ToJsonDocument().RootElement;
		CreateDocument(rootJson, out var documentMock, out _);
		documentMock.Setup(d => d.BaseUri).Returns(documentId);
		target.AddDocument(documentMock.Object);

		Assert.True(target.HasDocument(documentId));
	}

	[Fact]
	public void Allow_documents_to_be_retrieved()
	{
		var target = new DocumentRegistry(DefaultOptions());
		var rootJson = "foo".AsJsonElement();
		CreateDocument(rootJson, out var documentMock, out var documentId);
		target.AddDocument(documentMock.Object);

		var actual = target.ResolveMetadataNode(documentId);

		Assert.True(actual.Node.IsEquivalentTo(rootJson.AsNode()));
	}

	[Fact]
	public void Finds_a_document_via_fetch()
	{
		var rootJson = "foo".AsJsonElement();
		CreateDocument(rootJson, out var documentMock, out var documentId);
		var target = new DocumentRegistry(DefaultOptions() with
		{
			Resolvers = [
				(uri, relative) => uri == documentId ? documentMock.Object : null
			]
		});

		var actual = target.ResolveMetadataNode(documentId);

		Assert.True(actual.Node.IsEquivalentTo(rootJson.AsNode()));
	}

	[Fact]
	public void Throws_if_an_added_document_already_exists()
	{
		var target = new DocumentRegistry(DefaultOptions());
		var rootJson = "foo".AsJsonElement();
		CreateDocument(rootJson, out var documentMock, out var documentId);
		CreateDocumentWithRetrievalId("baz".AsJsonElement(), documentId, out var documentMock2);
		target.AddDocument(documentMock.Object);

		Assert.Throws<ArgumentException>(() => target.AddDocument(documentMock2.Object));
	}

	[Fact]
	public void Allow_fragment_metadata_to_be_retrieved_via_absolute_fragment()
	{
		var target = new DocumentRegistry(DefaultOptions());
		var rootJson = new { foo = new { bar = "baz" } }.ToJsonDocument().RootElement;
		CreateDocument(rootJson, out var documentMock, out var documentId);
		target.AddDocument(documentMock.Object);
		var fragmentId = new UriBuilder(documentId) { Fragment = "/foo/bar" }.Uri;

		var actual = target.ResolveMetadataNode(fragmentId);

		Assert.Equal(fragmentId, actual.Id);
		Assert.Equal("baz", actual.Node?.GetValue<string>());
	}

	[Fact]
	public void Allow_fragments_to_be_retrieved()
	{
		var target = new DocumentRegistry(DefaultOptions());
		var rootJson = new { foo = new { bar = "baz" } }.ToJsonDocument().RootElement;
		CreateDocument(rootJson, out var documentMock, out var documentId);
		target.AddDocument(documentMock.Object);
		var fragmentId = new UriBuilder(documentId) { Fragment = "/foo/bar" }.Uri;

		var actual = target.ResolveMetadataNode(fragmentId);

		Assert.Equal("baz", actual.Node?.GetValue<string>());
	}

	[Fact]
	public void Throws_if_a_fragment_cannot_be_located()
	{
		var target = new DocumentRegistry(DefaultOptions());
		var rootJson = new { foo = new { bar = "baz" } }.ToJsonDocument().RootElement;
		CreateDocument(rootJson, out var documentMock, out var documentId);
		target.AddDocument(documentMock.Object);
		var fragmentId = new UriBuilder(documentId) { Fragment = "/foo/bad/fragment" }.Uri;

		var diag = AssertThrowsDiagnostic<CouldNotFindTargetNodeDiagnostic>(() => target.ResolveMetadataNode(fragmentId));
		Assert.Equal(fragmentId, diag.Uri);
	}

	[Fact]
	public void Finds_a_fragment_via_fetch()
	{
		var rootJson = new { foo = new { bar = "baz" } }.ToJsonDocument().RootElement;
		CreateDocument(rootJson, out var documentMock, out var documentId);
		var target = new DocumentRegistry(DefaultOptions() with
		{
			Resolvers = [(uri, relative) => uri == documentId ? documentMock.Object : null]
		});
		var fragmentId = new UriBuilder(documentId) { Fragment = "/foo/bar" }.Uri;

		var actual = target.ResolveMetadataNode(fragmentId);

		Assert.Equal("baz", actual.Node?.GetValue<string>());
	}

	[Fact]
	public void Finds_a_fragment_via_anchor()
	{
		var target = new DocumentRegistry(DefaultOptions());
		var rootJson = new { foo = new Dictionary<string, object> { ["$anchor"] = "my_anchor", ["bar"] = "baz" } }.ToJsonDocument().RootElement;
		CreateDocument(rootJson, out var documentMock, out var documentId);
		target.AddDocument(documentMock.Object);
		var fragmentId = new UriBuilder(documentId) { Fragment = "my_anchor" }.Uri;

		var actual = target.ResolveMetadataNode(fragmentId);

		Assert.Equal("baz", actual.Node?["bar"]?.GetValue<string>());
	}

	[Fact(Skip = "TODO")]
	public void Automatically_locates_bundled_documents() { }


	// Invalidation is for incremental builds to ensure local files get updated
	[Fact(Skip = "TODO")]
	public void Can_invalidate_documents() { }

	// Invalidation is for incremental builds to ensure local files get updated
	[Fact(Skip = "TODO")]
	public void Can_invalidate_bundled_documents() { }



	private static void CreateDocument(JsonElement rootJson, out Moq.Mock<IDocumentReference> documentMock, out Uri documentId)
	{
		documentId = new Uri(internetDataSet.UrlWithPath());
		CreateDocumentWithRetrievalId(rootJson, documentId, out documentMock);
	}

	private static void CreateDocumentWithRetrievalId(JsonElement rootJson, Uri documentId, out Moq.Mock<IDocumentReference> documentMock)
	{
		documentMock = new Moq.Mock<IDocumentReference>();
		documentMock.SetupGet(m => m.RootNode).Returns(rootJson.AsNode());
		documentMock.SetupGet(m => m.RetrievalUri).Returns(documentId);
		documentMock.SetupGet(m => m.BaseUri).Returns(documentId);
	}

	private static T AssertThrowsDiagnostic<T>(Action testCode)
		where T : DiagnosticBase
	{
		return UnwrapDiagnostic<T>(Assert.Throws<DiagnosticException>(testCode));
	}

	private static T UnwrapDiagnostic<T>(DiagnosticException ex)
		where T : DiagnosticBase
	{
		var location = new Location(new Uri(internetDataSet.UrlWithPath()));
		var diagnostic = Assert.IsType<T>(ex.ConstructDiagnostic(location));
		Assert.Equal(location, diagnostic.Location);
		return diagnostic;
	}

}
