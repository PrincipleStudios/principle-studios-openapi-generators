using Json.Pointer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace PrincipleStudios.OpenApi.Transformations.Specifications;

public static class UriUtils
{
	public static Uri AppendPointer(this Uri uri, string pointerParts)
	{
		var result = new UriBuilder(uri);
		result.Fragment = result.Fragment + "/" + pointerParts;
		return result.Uri;
	}

	public static NodeMetadata Navigate(this NodeMetadata resolution, string pointerStep)
	{
		var pointer = JsonPointer.Create(PointerSegment.Create(pointerStep));

		var resultUri = new UriBuilder(resolution.Id);
		resultUri.Fragment = resultUri.Fragment + pointer.ToString();
		return resolution with { Id = resultUri.Uri, Node = pointer.TryEvaluate(resolution.Node, out var resultNode) ? resultNode : null };
	}
}
