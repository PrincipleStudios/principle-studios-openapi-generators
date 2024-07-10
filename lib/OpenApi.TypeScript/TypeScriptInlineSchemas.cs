using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.Pointer;
using PrincipleStudios.OpenApi.Transformations;
using PrincipleStudios.OpenApi.Transformations.Abstractions;
using PrincipleStudios.OpenApi.Transformations.Specifications;
using PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0;

namespace PrincipleStudios.OpenApi.TypeScript;

public record TypeScriptImportReference(JsonSchema Schema, string Member, string File);

public record TypeScriptInlineDefinition(string Text, IReadOnlyList<TypeScriptImportReference> Imports, bool Nullable = false, bool IsEnumerable = false)
{
	public TypeScriptInlineDefinition MakeNullable() =>
		Nullable ? this : this with { Text = Text + " | null", Nullable = true };
}

public class TypeScriptInlineSchemas(TypeScriptSchemaOptions options, DocumentRegistry documentRegistry)
{
	public static readonly TypeScriptInlineDefinition AnyObject = new("any", [], Nullable: true);

	internal bool ProduceSourceEntry(JsonSchema schema)
	{
		var nodes = documentRegistry.GetNodesTo(schema.Metadata.Id);
		if (nodes.LastOrDefault() is (["allOf", _], JsonSchema))
			return false;

		// We're going to inline much less than TS allows because it makes things easier for developers
		return TypeScriptTypeInfo.From(schema) switch
		{
			{ Type: "object", Properties.Count: 0, AdditionalProperties: JsonSchema } => true,
			{ AllOf.Count: > 0 } => true,
			{ AnyOf.Count: > 0 } => true,
			{ OneOf.Count: > 0 } => true,
			{ Type: "string", Enum.Count: > 0 } => true,
			{ Type: "array" } or { Items: JsonSchema _ } => false,
			{ Type: string type, Format: var format, Properties.Count: 0, Enum.Count: 0 } => options.Find(type, format) == "object",
			{ Type: "object", Format: null } => true,
			{ Properties.Count: > 0 } => true,
			{ Type: "string" or "number" or "integer" or "boolean" } => false,
			{ } => false,
			_ => throw new NotSupportedException("Unknown schema"),
		};
	}

	internal TypeScriptInlineDefinition ToInlineDataType(JsonSchema? schema)
	{
		if (schema == null) return new TypeScriptInlineDefinition("never", [], true, false);

		var typeInfo = TypeScriptTypeInfo.From(schema);
		TypeScriptInlineDefinition result = typeInfo switch
		{
			_ when ProduceSourceEntry(schema) =>
				new(UseReferenceName(schema), [ToImportReference(schema)]),
			{ Type: string type, Format: var format } =>
				new(options.Find(type, format), []),
			_ => throw new NotSupportedException("Unknown schema"),
		};
		return schema?.TryGetAnnotation<NullableKeyword>() is { IsNullable: true }
			? result.MakeNullable()
			: result;
	}

	public string ToSourceEntryKey(JsonSchema schema)
	{
		var className = UseReferenceName(schema);
		return $"models/{className}.ts";
	}

	public TypeScriptImportReference ToImportReference(JsonSchema schema)
	{
		return new TypeScriptImportReference(schema, UseReferenceName(schema), ToSourceEntryKey(schema));
	}

	private string UseReferenceName(JsonSchema schema)
	{
		return TypeScriptNaming.ToClassName(UriToClassIdentifier(schema.Metadata.Id), options.ReservedIdentifiers());
	}

	private static readonly Regex HttpSuccessRegex = new Regex("2[0-9]{2}");
	private static bool Is2xx(int statusCode) => statusCode is >= 200 and < 300;

	internal string UriToClassIdentifier(Uri uri)
	{
		IReadOnlyList<JsonDocumentNodeContext> remaining = documentRegistry.GetNodesTo(uri);
		if (remaining.Count == 0)
			return string.Join(" ", JsonPointer.Parse(uri.Fragment).Segments.Select(s => s.Value));

		IEnumerable<string> parts = Enumerable.Empty<string>();
		while (remaining.Count > 0)
		{
			(var newParts, remaining) = Simplify(remaining);
			parts = parts.Concat(newParts).ToArray();
		}

		return string.Join(" ", parts);

		(IEnumerable<string> parts, IReadOnlyList<JsonDocumentNodeContext> remaining) Simplify(IReadOnlyList<JsonDocumentNodeContext> context)
		{
			switch (context[0])
			{
				case (["paths", var path], OpenApiPath) when context.Count >= 2:
					switch (context[1])
					{
						case ([var method], OpenApiOperation { OperationId: null }):
							return ([$"{method} ${path}"], context.Skip(2).ToArray());
						case (_, OpenApiOperation { OperationId: string opId }):
							return ([opId], context.Skip(2).ToArray());
						default:
							throw new NotImplementedException();
					}
				case ([], OpenApiDocument) when context.Count >= 2 && context[1] is (["components", _, string componentName], JsonSchema):
					return ([componentName], context.Skip(2).ToArray());
				case (_, OpenApiDocument or OpenApiPath):
					return (Enumerable.Empty<string>(), context.Skip(1).ToArray());
				case (["responses"], OpenApiResponses responses) when context.Count >= 4:
					{
						if (context[1] is not ([var statusCode], OpenApiResponse response)) throw new NotImplementedException();
						if (context[3] is not (["schema"], _)) throw new NotImplementedException();

						var responseName = statusCode switch
						{
							"default" when responses.StatusCodeResponses.Count == 0 => "",
							"default" => "other",
							_ when responses.StatusCodeResponses.Count == 1 && responses.Default == null
								=> "",
							_ when HttpSuccessRegex.IsMatch(statusCode) && responses.StatusCodeResponses.Keys.Count(Is2xx) == 1
								=> "",
							_ when int.TryParse(statusCode, out var numeric) && HttpStatusCodes.StatusCodeNames.TryGetValue(numeric, out var statusCodeName)
								=> statusCodeName,
							_ => statusCode,
						};
						var (qualifierName, typeName) = context[2] switch
						{
							(["content", _], _) when response.Content!.Count == 1 => ("", "response"),
							(["content", var mimeType], _) => (mimeType, "response"),
							(["headers", var headerNam], _) => (headerNam, "header"),
							_ => throw new NotImplementedException()
						};
						return ([responseName, qualifierName, typeName], context.Skip(4).ToArray());
					}
				case (["components", "responses", var responseName], OpenApiResponse response) when context.Count >= 3:
					{
						if (context[2] is not (["schema"], _)) throw new NotImplementedException();

						var (qualifierName, typeName) = context[1] switch
						{
							(["content", _], _) when response.Content!.Count == 1 => ("", "response"),
							(["content", var mimeType], _) => (mimeType, "response"),
							(["headers", var headerNam], _) => (headerNam, "header"),
							_ => throw new NotImplementedException()
						};
						return ([responseName, qualifierName, typeName], context.Skip(3).ToArray());
					}
				case (["requestBody"], OpenApiRequestBody requestBody) when context.Count >= 3:
					{
						if (context[1] is not (["content", var mimeType], _)) throw new NotImplementedException();
						if (context[2] is not (["schema"], _)) throw new NotImplementedException();

						return ([requestBody.Content!.Count == 1 ? "" : mimeType, "request"], context.Skip(3).ToArray());
					}
				case (["parameters", _], OpenApiParameter { Name: string paramName }) when context.Count >= 1:
					if (context[1] is not (["schema"], _)) throw new NotImplementedException();
					return ([paramName], context.Skip(2).ToArray());
				case (["items"], JsonSchema):
					return (["Item"], context.Skip(1).ToArray());
				case (["properties", var propName], JsonSchema):
					return ([propName], context.Skip(1).ToArray());
				case (["additionalProperties"], JsonSchema):
					return (["AdditionalProperty"], context.Skip(1).ToArray());
				case (var parts, JsonSchema):
					return (parts, context.Skip(1).ToArray());
				case (var parts, var t):
					throw new NotImplementedException($"{string.Join(", ", parts)} {t.GetType().FullName}");
				default:
					throw new NotImplementedException();
			};
		}
	}
}
