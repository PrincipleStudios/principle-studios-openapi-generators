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
		var mockFetch = new Mock<Func<Uri, IDocumentReference?>>();
		mockFetch.Setup(a => a(It.IsAny<Uri>())).Returns((IDocumentReference?)null);
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
		target.AddDocument(documentId, documentMock.Object);

		Assert.Contains(documentId, target.RegisteredDocumentIds);
	}

	[Fact]
	public void Allow_documents_to_be_retrieved()
	{
		var target = new DocumentRegistry();
		var rootJson = "foo".AsJsonElement();
		CreateDocument(rootJson, out var documentMock, out var documentId);
		target.AddDocument(documentId, documentMock.Object);

		var actual = target.ResolveNode(documentId);

		Assert.Equal(rootJson, actual);
	}

	[Fact]
	public void Finds_a_document_via_fetch()
	{
		var target = new DocumentRegistry();
		var rootJson = "foo".AsJsonElement();
		CreateDocument(rootJson, out var documentMock, out var documentId);
		target.Fetch = (uri) => uri == documentId ? documentMock.Object : null;

		var actual = target.ResolveNode(documentId);

		Assert.Equal(rootJson, actual);
	}

	[Fact]
	public void Throws_if_an_added_document_already_exists()
	{
		var target = new DocumentRegistry();
		var rootJson = "foo".AsJsonElement();
		CreateDocument(rootJson, out var documentMock, out var documentId);
		CreateDocumentWithoutId("baz".AsJsonElement(), out var documentMock2);
		target.AddDocument(documentId, documentMock.Object);

		Assert.Throws<ArgumentException>(() => target.AddDocument(documentId, documentMock2.Object));
	}

	[Fact]
	public void Allow_fragments_to_be_retrieved()
	{
		var target = new DocumentRegistry();
		var rootJson = new { foo = new { bar = "baz" } }.ToJsonDocument().RootElement;
		CreateDocument(rootJson, out var documentMock, out var documentId);
		target.AddDocument(documentId, documentMock.Object);
		var fragmentId = new UriBuilder(documentId) { Fragment = "/foo/bar" }.Uri;

		var actual = target.ResolveNode(fragmentId);

		Assert.Equal("\"baz\"", actual.ToJsonString());
	}

	[Fact]
	public void Throws_if_a_fragment_cannot_be_located()
	{
		var target = new DocumentRegistry();
		var rootJson = new { foo = new { bar = "baz" } }.ToJsonDocument().RootElement;
		CreateDocument(rootJson, out var documentMock, out var documentId);
		target.AddDocument(documentId, documentMock.Object);
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
		target.Fetch = (uri) => uri == documentId ? documentMock.Object : null;
		var fragmentId = new UriBuilder(documentId) { Fragment = "/foo/bar" }.Uri;

		var actual = target.ResolveNode(fragmentId);

		Assert.Equal("\"baz\"", actual.ToJsonString());
	}

	[Fact(Skip = "TODO")]
	public void Automatically_locates_nested_document_references() { }

	[Fact(Skip = "TODO")]
	public void Can_invalidate_documents() { }

	[Fact(Skip = "TODO")]
	public void Can_invalidate_nested_documents() { }



	private static void CreateDocument(JsonElement rootJson, out Moq.Mock<IDocumentReference> documentMock, out Uri documentId)
	{
		documentId = new Uri(new Bogus.DataSets.Internet().UrlWithPath());
		CreateDocumentWithoutId(rootJson, out documentMock);
		documentMock.SetupGet(m => m.Id).Returns(documentId);
	}

	private static void CreateDocumentWithoutId(JsonElement rootJson, out Moq.Mock<IDocumentReference> documentMock)
	{
		documentMock = new Moq.Mock<IDocumentReference>();
		documentMock.SetupGet(m => m.RootElement).Returns(rootJson);
	}

}
