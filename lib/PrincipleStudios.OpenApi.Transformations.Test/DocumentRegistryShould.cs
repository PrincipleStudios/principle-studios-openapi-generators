using Json.More;
using Moq;
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
	[Fact]
	public void Throws_if_a_document_cannot_be_located_without_a_fetch_function()
	{
		var target = new DocumentRegistry();
		var documentId = new Uri(new Bogus.DataSets.Internet().UrlWithPath());

		var ex = Assert.Throws<ResolveDocumentException>(() => target.ResolveNode(documentId));
		Assert.Equal(documentId, ex.Uri);
	}

	[Fact]
	public void Throws_if_a_document_cannot_be_located_with_a_fetch_function()
	{
		var target = new DocumentRegistry();
		var mockFetch = new Mock<DocumentResolver>();
		mockFetch.Setup(a => a(It.IsAny<Uri>(), It.IsAny<IDocumentReference?>())).Returns((IDocumentReference?)null);
		target.Fetch = mockFetch.Object;

		var documentId = new Uri(new Bogus.DataSets.Internet().UrlWithPath());

		var ex = Assert.Throws<ResolveDocumentException>(() => target.ResolveNode(documentId));
		Assert.Equal(documentId, ex.Uri);
	}

	[Fact]
	public void Allow_documents_to_be_added()
	{
		var target = new DocumentRegistry();
		var rootJson = "foo".AsJsonElement();
		CreateDocument(rootJson, out var documentMock, out var documentId);
		target.AddDocument(documentMock.Object);

		Assert.Contains(documentId, target.RegisteredDocumentIds);
	}

	[Fact]
	public void Allow_documents_to_be_added_with_a_different_base_uri()
	{
		var target = new DocumentRegistry();
		var documentId = new Uri(new Bogus.DataSets.Internet().UrlWithPath());
		var rootJson = new Dictionary<string, object> { ["$id"] = documentId }.ToJsonDocument().RootElement;
		CreateDocument(rootJson, out var documentMock, out _);
		documentMock.Setup(d => d.BaseUri).Returns(documentId);
		target.AddDocument(documentMock.Object);

		Assert.Contains(documentId, target.RegisteredDocumentIds);
	}

	[Fact]
	public void Allow_documents_to_be_retrieved()
	{
		var target = new DocumentRegistry();
		var rootJson = "foo".AsJsonElement();
		CreateDocument(rootJson, out var documentMock, out var documentId);
		target.AddDocument(documentMock.Object);

		var actual = target.ResolveNode(documentId);

		Assert.True(actual.IsEquivalentTo(rootJson.AsNode()));
	}

	[Fact]
	public void Finds_a_document_via_fetch()
	{
		var target = new DocumentRegistry();
		var rootJson = "foo".AsJsonElement();
		CreateDocument(rootJson, out var documentMock, out var documentId);
		target.Fetch = (uri, relative) => uri == documentId ? documentMock.Object : null;

		var actual = target.ResolveNode(documentId);

		Assert.True(actual.IsEquivalentTo(rootJson.AsNode()));
	}

	[Fact]
	public void Finds_a_relative_document_via_fetch()
	{
		var target = new DocumentRegistry();
		var rootJson = "foo".AsJsonElement();
		CreateDocument(rootJson, out var documentMock, out var documentId);

		var relativePath = new Uri("/relative/path", UriKind.Relative);
		var document2Id = new Uri(documentId, relativePath);
		var rootJson2 = "foo2".AsJsonElement();
		CreateDocumentWithRetrievalId(rootJson2, document2Id, out var document2Mock);

		target.Fetch = (uri, relative) => uri == document2Id ? document2Mock.Object : null;

		var actual = target.ResolveNode(documentMock.Object, relativePath);

		Assert.True(actual.IsEquivalentTo(rootJson2.AsNode()));
	}

	[Fact]
	public void Throws_if_an_added_document_already_exists()
	{
		var target = new DocumentRegistry();
		var rootJson = "foo".AsJsonElement();
		CreateDocument(rootJson, out var documentMock, out var documentId);
		CreateDocumentWithRetrievalId("baz".AsJsonElement(), documentId, out var documentMock2);
		target.AddDocument(documentMock.Object);

		Assert.Throws<ArgumentException>(() => target.AddDocument(documentMock2.Object));
	}

	[Fact]
	public void Allow_fragments_to_be_retrieved()
	{
		var target = new DocumentRegistry();
		var rootJson = new { foo = new { bar = "baz" } }.ToJsonDocument().RootElement;
		CreateDocument(rootJson, out var documentMock, out var documentId);
		target.AddDocument(documentMock.Object);
		var fragmentId = new UriBuilder(documentId) { Fragment = "/foo/bar" }.Uri;

		var actual = target.ResolveNode(fragmentId);

		Assert.Equal("baz", actual?.GetValue<string>());
	}

	[Fact]
	public void Throws_if_a_fragment_cannot_be_located()
	{
		var target = new DocumentRegistry();
		var rootJson = new { foo = new { bar = "baz" } }.ToJsonDocument().RootElement;
		CreateDocument(rootJson, out var documentMock, out var documentId);
		target.AddDocument(documentMock.Object);
		var fragmentId = new UriBuilder(documentId) { Fragment = "/foo/bad/fragment" }.Uri;

		var ex = Assert.Throws<ResolveNodeException>(() => target.ResolveNode(fragmentId));
		Assert.Equal(fragmentId, ex.Uri);
	}

	[Fact]
	public void Finds_a_fragment_via_fetch()
	{
		var target = new DocumentRegistry();
		var rootJson = new { foo = new { bar = "baz" } }.ToJsonDocument().RootElement;
		CreateDocument(rootJson, out var documentMock, out var documentId);
		target.Fetch = (uri, relative) => uri == documentId ? documentMock.Object : null;
		var fragmentId = new UriBuilder(documentId) { Fragment = "/foo/bar" }.Uri;

		var actual = target.ResolveNode(fragmentId);

		Assert.Equal("baz", actual?.GetValue<string>());
	}

	[Fact]
	public void Finds_a_fragment_via_relative_fetch()
	{

		var target = new DocumentRegistry();
		var rootJson = "foo".AsJsonElement();
		CreateDocument(rootJson, out var documentMock, out var documentId);

		var relativePath = new Uri("/relative/path", UriKind.Relative);
		var document2Id = new Uri(documentId, relativePath);
		var rootJson2 = new { foo = new { bar = "baz" } }.ToJsonDocument().RootElement;
		CreateDocumentWithRetrievalId(rootJson2, document2Id, out var document2Mock);
		var fragmentId = new Uri("/relative/path#/foo/bar", UriKind.Relative);

		target.Fetch = (uri, relative) => uri == document2Id ? document2Mock.Object : null;

		var actual = target.ResolveNode(documentMock.Object, relativePath);

		Assert.True(actual.IsEquivalentTo(rootJson2.AsNode()));
	}

	[Fact]
	public void Finds_a_fragment_via_anchor()
	{
		var target = new DocumentRegistry();
		var rootJson = new { foo = new Dictionary<string, object> { ["$anchor"] = "my_anchor", ["bar"] = "baz" } }.ToJsonDocument().RootElement;
		CreateDocument(rootJson, out var documentMock, out var documentId);
		target.AddDocument(documentMock.Object);
		var fragmentId = new UriBuilder(documentId) { Fragment = "my_anchor" }.Uri;

		var actual = target.ResolveNode(fragmentId);

		Assert.Equal("baz", actual?["bar"]?.GetValue<string>());
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
		documentId = new Uri(new Bogus.DataSets.Internet().UrlWithPath());
		CreateDocumentWithRetrievalId(rootJson, documentId, out documentMock);
	}

	private static void CreateDocumentWithRetrievalId(JsonElement rootJson, Uri documentId, out Moq.Mock<IDocumentReference> documentMock)
	{
		documentMock = new Moq.Mock<IDocumentReference>();
		documentMock.SetupGet(m => m.RootNode).Returns(rootJson.AsNode());
		documentMock.SetupGet(m => m.RetrievalUri).Returns(documentId);
		documentMock.SetupGet(m => m.BaseUri).Returns(documentId);
	}

}
