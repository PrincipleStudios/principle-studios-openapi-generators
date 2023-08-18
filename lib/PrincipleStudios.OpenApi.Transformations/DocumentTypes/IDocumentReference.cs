using Json.Pointer;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations.DocumentTypes;

public interface IDocumentReference
{
	/// <summary>
	/// The Id of the document.
	/// </summary>
	Uri Id { get; }

	/// <summary>
	/// The root element of the document.
	/// </summary>
	System.Text.Json.JsonElement RootElement { get; }

	/// <summary>
	/// May be a file path on disk or may be a Uri.
	/// </summary>
	string OriginalPath { get; }

	FileLocation? GetLocation(JsonPointer path);
}
