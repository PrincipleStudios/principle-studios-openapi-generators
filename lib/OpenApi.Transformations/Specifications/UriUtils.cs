using Json.Pointer;
using System;
using System.Globalization;

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
		resultUri.Fragment += pointer.ToString();
		return resolution with { Id = resultUri.Uri };
	}

	public static NodeMetadata Navigate(this NodeMetadata resolution, int pointerStep) =>
		Navigate(resolution, pointerStep.ToString(CultureInfo.InvariantCulture));

	public static ResolvableNode Navigate(this ResolvableNode resolution, string pointerStep)
	{
		var pointer = JsonPointer.Create(PointerSegment.Create(pointerStep));
		return new ResolvableNode(
			metadata: resolution.Metadata.Navigate(pointerStep),
			registry: resolution.Registry,
			document: resolution.Document,
			node: pointer.TryEvaluate(resolution.Node, out var resultNode) ? resultNode : null
		);
	}

	public static ResolvableNode Navigate(this ResolvableNode resolution, int pointerStep) =>
		Navigate(resolution, pointerStep.ToString(CultureInfo.InvariantCulture));
}
