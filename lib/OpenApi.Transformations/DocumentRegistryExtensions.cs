using System;
using System.Collections.Generic;
using System.Linq;
using Json.Pointer;
using PrincipleStudios.OpenApi.Transformations.Specifications;

namespace PrincipleStudios.OpenApi.Transformations;

public record JsonDocumentNodeContext(IReadOnlyList<string> Steps, IJsonDocumentNode Element);

public static class DocumentRegistryExtensions
{

	public static JsonDocumentNodeContext[] GetNodesTo(this DocumentRegistry documentRegistry, Uri uri)
	{
		if (!documentRegistry.TryGetAllNodes(uri, out var nodes))
			// Document was not fully registered with the registry
			return Array.Empty<JsonDocumentNodeContext>();

		var fragment = Normalize(uri.Fragment);
		var relevantNodes = (from n in nodes
							 let id = Id(n)
							 where id == fragment || fragment.StartsWith(id + "/")
							 group n by id into similar
							 orderby similar.Key.Length
							 select similar.First()).ToArray();
		var ids = relevantNodes.Select(Id).ToArray();
		var steps = ids
			.Select((id, index) => index == 0 ? id : id.Substring(ids[index - 1].Length))
			.ToArray();

		return relevantNodes
			.Zip(steps, (node, step) => new JsonDocumentNodeContext(JsonPointer.Parse(step).Segments.Select(s => s.Value).ToArray(), node))
			.ToArray();

		static string Id(IJsonDocumentNode node) => Normalize(node.Metadata.Id.Fragment);
		static string Normalize(string fragment) => Uri.UnescapeDataString(fragment) switch
		{
			{ Length: 0 } => "",
			"#/" => "",
			var s when s[s.Length - 1] == '/' => s.Substring(1, s.Length - 1),
			var s => s.Substring(1),
		};
	}
}