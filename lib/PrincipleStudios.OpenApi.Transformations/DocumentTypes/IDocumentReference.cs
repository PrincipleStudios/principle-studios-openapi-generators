using Json.Pointer;
using Json.Schema;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations.DocumentTypes;

public interface IDocumentReference : IBaseDocument
{
	/// <summary>
	/// The RetrievalUri of the document.
	/// </summary>
	Uri RetrievalUri { get; }

	/// <summary>
	/// The root element of the document.
	/// </summary>
	System.Text.Json.Nodes.JsonNode? RootNode { get; }

	/// <summary>
	/// May be a file path on disk or may be a Uri.
	/// </summary>
	string OriginalPath { get; }

	FileLocationRange? GetLocation(JsonPointer path);
}
