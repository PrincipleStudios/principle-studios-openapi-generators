using Json.Pointer;
using Json.Schema;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PrincipleStudios.OpenApi.Transformations.DocumentTypes;

public static class SubschemaLoader
{
	public static JsonSchema? FindSubschema(IDocumentReference documentReference, JsonPointer jsonPointer, Json.Schema.EvaluationOptions options)
	{
		if (!jsonPointer.TryEvaluate(documentReference.RootNode, out var subNode) || subNode == null) return null;

		var schema = JsonSerializer.Deserialize<JsonSchema>(subNode);
		if (schema == null) return null;

		schema.BaseUri = new UriBuilder(documentReference.BaseUri) { Fragment = documentReference.BaseUri.Fragment + jsonPointer.ToString() }.Uri;
		return schema;
	}
}
