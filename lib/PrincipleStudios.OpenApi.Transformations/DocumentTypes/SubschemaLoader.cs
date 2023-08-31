using Json.Pointer;
using Json.Schema;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
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

		try
		{
			var schema = JsonSerializer.Deserialize<JsonSchema>(subNode);
			if (schema == null) return null;

			schema.BaseUri = new UriBuilder(documentReference.BaseUri) { Fragment = documentReference.BaseUri.Fragment + jsonPointer.ToString() }.Uri;
			return schema;
		}
		catch (JsonException ex)
		{
			throw new DiagnosticException(UnableToParseSchema.Builder(ex));
		}
	}
}

public record UnableToParseSchema(JsonException JsonException, Location Location) : DiagnosticBase(Location)
{
	public static DiagnosticException.ToDiagnostic Builder(JsonException JsonException) => (Location) => new UnableToParseSchema(JsonException, Location);
}
