using Json.More;
using Json.Pointer;
using PrincipleStudios.OpenApi.Transformations.Abstractions;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0;

#pragma warning disable CA1822 // Mark members as static

/// <summary>
/// See https://spec.openapis.org/oas/v3.0.3
/// </summary>
internal class OpenApi3_0DocumentFactory : IOpenApiDocumentFactory
{
	private static string[] validMethods = new[]
	{
		"get",
		"put",
		"post",
		"delete",
		"options",
		"head",
		"patch",
		"trace",
	};

	private record Key(Uri Id, JsonNode? Node, IDocumentReference? CurrentDocument)
	{
		public Key AppendPointerStep(string pointerStep)
		{
			var pointer = JsonPointer.Create(PointerSegment.Create(pointerStep));

			var resultUri = new UriBuilder(Id);
			resultUri.Fragment = resultUri.Fragment + pointer.ToString();
			return this with { Id = resultUri.Uri, Node = pointer.TryEvaluate(Node, out var resultNode) ? resultNode : null };
		}
	}

	public static readonly Uri jsonSchemaDialect = new Uri("https://spec.openapis.example.org/oas/3.0/dialect/base");
	private readonly DocumentRegistry documentRegistry;

	public OpenApi3_0DocumentFactory(DocumentRegistry documentRegistry)
	{
		this.documentRegistry = documentRegistry;
	}

	public OpenApiDocument ConstructDocument(IDocumentReference documentReference)
	{
		var key = new Key(documentReference.BaseUri, documentReference.RootNode, documentReference);
		return ConstructDocument(key);
	}

	private OpenApiDocument ConstructDocument(Key key)
	{
		if (key.Node is not JsonObject obj) throw new InvalidOperationException(Errors.InvalidOpenApiRootNode);
		return new OpenApiDocument(key.Id,
			OpenApiSpecVersion: new OpenApiSpecVersion("openapi", obj["openapi"]?.GetValue<string>() ?? "3.0.3"),
			Info: ConstructInfo(key.AppendPointerStep("info"))
				// TODO: report diagnostic for missing info
				?? new MissingRequiredFieldDefaults.PlaceholderInfo(key.Id),
			JsonSchemaDialect: jsonSchemaDialect,
			Paths: ConstructPaths(key.AppendPointerStep("paths"))
		);
	}

	private OpenApiContact? ConstructContact(Key key)
	{
		if (key.Node is not JsonObject obj) return null;
		return new OpenApiContact(key.Id,
			Name: obj["name"]?.GetValue<string>(),
			Url: obj["url"]?.GetValue<string>() is string url
				&& Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var result)
					? result
					: null,
			Email: obj["email"]?.GetValue<string>()
		);
	}

	private OpenApiInfo? ConstructInfo(Key key)
	{
		if (key.Node is not JsonObject obj) return null;
		return new OpenApiInfo(key.Id,
			Title: obj["title"]?.GetValue<string>() ?? MissingRequiredFieldDefaults.InfoTitle,
			Summary: null,
			Description: obj["description"]?.GetValue<string>(),
			TermsOfService: obj["termsOfService"]?.GetValue<string>() is string tos
				&& Uri.TryCreate(tos, UriKind.RelativeOrAbsolute, out var result)
					? result
					: null,
			Contact: obj["contact"] is JsonObject
				? ConstructContact(key.AppendPointerStep("contact"))
				: null,
			License: obj["license"] is JsonObject
				? ConstructLicense(key.AppendPointerStep("license"))
				: null,
			Version: obj["version"]?.GetValue<string>() ?? MissingRequiredFieldDefaults.InfoVersion
		);
	}

	private OpenApiLicense? ConstructLicense(Key key)
	{
		if (key.Node is not JsonObject obj) return null;
		return new OpenApiLicense(key.Id,
			Name: obj["name"]?.GetValue<string>() ?? MissingRequiredFieldDefaults.LicenseName,
			Url: obj["url"]?.GetValue<string>() is string url
					&& Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var result)
						? result
						: null,
			Identifier: null
		);
	}

	// https://spec.openapis.org/oas/v3.0.0#media-type-object
	private OpenApiMediaTypeObject ConstructMediaTypeObject(Key key)
	{
		if (key.Node is not JsonObject obj) return new MissingRequiredFieldDefaults.PlaceholderMediaTypeObject(key.Id);
		return new OpenApiMediaTypeObject(key.Id,
			Schema: AllowReference(ConstructSchema)(key.AppendPointerStep("schema"))
		);
	}

	// https://spec.openapis.org/oas/v3.0.0#operationObject
	private OpenApiOperation ConstructOperation(Key key)
	{
		if (key.Node is not JsonObject obj) return new MissingRequiredFieldDefaults.PlaceholderOperation(key.Id);
		return new OpenApiOperation(key.Id,
			Tags: ReadArray(key.AppendPointerStep("tags"), k => k.Node?.GetValue<string>() ?? MissingRequiredFieldDefaults.OperationTag),
			Summary: obj["summary"]?.GetValue<string>(),
			Description: obj["description"]?.GetValue<string>(),
			OperationId: obj["operationId"]?.GetValue<string>(),
			Parameters: ReadArray(key.AppendPointerStep("parameters"), AllowReference(ConstructParameter)),
			RequestBody: AllowReference(ConstructRequestBody)(key.AppendPointerStep("requestBody")),
			Responses: ConstructResponses(key.AppendPointerStep("responses")),
			Deprecated: obj["deprecated"]?.GetValue<bool>() ?? false
		);
	}

	// https://spec.openapis.org/oas/v3.0.0#parameterObject
	private OpenApiParameter ConstructParameter(Key key)
	{
		if (key.Node is not JsonObject obj) return new MissingRequiredFieldDefaults.PlaceholderParameter(key.Id);

		var location = ToParameterLocation(obj["in"]);
		return LoadParameter(key, obj, location);
	}

	private OpenApiParameter LoadParameter(Key key, JsonObject obj, ParameterLocation location)
	{
		var style = obj["style"]?.GetValue<string>() ?? (location switch
		{
			ParameterLocation.Header => "simple",
			ParameterLocation.Query => "form",
			ParameterLocation.Cookie => "form",
			ParameterLocation.Path => "simple",
			_ => "form",
		});
		return new OpenApiParameter(
					Id: key.Id,
					Name: obj["name"]?.GetValue<string>() ?? MissingRequiredFieldDefaults.ParameterName,
					In: location,
					Description: obj["description"]?.GetValue<string>(),
					Required: location == ParameterLocation.Path ? true : obj["required"]?.GetValue<bool>() ?? false,
					Deprecated: obj["deprecated"]?.GetValue<bool>() ?? false,
					AllowEmptyValue: obj["allowEmptyValue"]?.GetValue<bool>() ?? false,
					Style: style,
					Explode: obj["allowEmptyValue"]?.GetValue<bool>() ?? (style == "form"),
					Schema: AllowReference(ConstructSchema)(key.AppendPointerStep("schema"))
				);
	}

	private OpenApiParameter ConstructHeaderParameter(Key key)
	{
		if (key.Node is not JsonObject obj) return new MissingRequiredFieldDefaults.PlaceholderHeaderParameter(key.Id);

		return LoadParameter(key, obj, ParameterLocation.Header);
	}

	private Json.Schema.JsonSchema? ConstructSchema(Key key)
	{
		return documentRegistry.ResolveSchema(key.Id, key.CurrentDocument);
	}

	private ParameterLocation ToParameterLocation(JsonNode? jsonNode)
	{
		switch (jsonNode?.GetValue<string>())
		{
			case "path": return ParameterLocation.Path;
			case "query": return ParameterLocation.Query;
			case "header": return ParameterLocation.Header;
			case "cookie": return ParameterLocation.Cookie;
			case null:
			default:
				// TODO: probably log diagnostics
				return ParameterLocation.Query;
		}
	}

	private IReadOnlyDictionary<string, OpenApiPath> ConstructPaths(Key key)
	{
		if (key.Node is not JsonObject obj) return new Dictionary<string, OpenApiPath>();

		return ReadDictionary(key, filter: _ => true, toKeyValuePair: (prop, key) => (prop, ConstructPath(key)));
	}

	private OpenApiPath ConstructPath(Key key)
	{
		if (key.Node is not JsonObject obj) return new MissingRequiredFieldDefaults.EmptyPath(key.Id);
		return new OpenApiPath(key.Id,
			Summary: obj["summary"]?.GetValue<string>(),
			Description: obj["description"]?.GetValue<string>(),
			Operations: ReadDictionary(key, validMethods.Contains, toKeyValuePair: (method, key) => (Key: method, Value: ConstructOperation(key.AppendPointerStep(method))))
		);
	}

	// https://spec.openapis.org/oas/v3.0.0#request-body-object
	private OpenApiRequestBody? ConstructRequestBody(Key key)
	{
		if (key.Node is not JsonObject obj) return null;
		return new OpenApiRequestBody(
			Id: key.Id,
			Description: obj["description"]?.GetValue<string>(),
			Content: ConstructMediaContentDictionary(key.AppendPointerStep("content")),
			Required: obj["required"]?.GetValue<bool>() ?? false
		);
	}

	private IReadOnlyDictionary<string, OpenApiMediaTypeObject> ConstructMediaContentDictionary(Key key)
	{
		if (key.Node is not JsonObject obj) return new Dictionary<string, OpenApiMediaTypeObject>();
		return ReadDictionary(key,
			filter: media => media.Contains('/'),
			toKeyValuePair: (media, key) => (Key: media, Value: ConstructMediaTypeObject(key))
		);
	}

	// https://spec.openapis.org/oas/v3.0.0#response-object
	private OpenApiResponse ConstructResponse(Key key)
	{
		if (key.Node is not JsonObject obj) return new MissingRequiredFieldDefaults.PlaceholderResponse(key.Id);
		return new OpenApiResponse(key.Id,
			Description: obj["description"]?.GetValue<string>() ?? MissingRequiredFieldDefaults.ResponseDescription,
			Headers: ReadDictionary(
				key,
				_ => true,
				(name, key) => (name, AllowReference((key) => ConstructHeaderParameter(key))(key) with { Name = name })
			).Values.ToArray(),
			Content: ConstructMediaContentDictionary(key.AppendPointerStep("content"))
		);
	}

	// https://spec.openapis.org/oas/v3.0.0#responses-object
	private OpenApiResponses? ConstructResponses(Key key)
	{
		if (key.Node is not JsonObject obj) return null;
		return new OpenApiResponses(Id: key.Id,
			Default: AllowReference(ConstructResponse)(key.AppendPointerStep("default")),
			StatusCodeResponses: ReadDictionary(key,
				filter: statusCode => statusCode.Length == 3 && int.TryParse(statusCode, NumberStyles.Integer, CultureInfo.InvariantCulture, out _),
				toKeyValuePair: (statusCode, key) => (Key: int.Parse(statusCode, NumberStyles.Integer, CultureInfo.InvariantCulture), Value: AllowReference(ConstructResponse)(key))
			)
		);
	}

	private IReadOnlyDictionary<TKey, TValue> ReadDictionary<TKey, TValue>(Key key, Func<string, bool> filter, Func<string, Key, (TKey Key, TValue Value)> toKeyValuePair, Action? ifNotObject = null)
	{
		if (key.Node is not JsonObject obj)
		{
			ifNotObject?.Invoke();
			return new Dictionary<TKey, TValue>();
		}
		return obj
			.Select(kvp => kvp.Key)
			.Where(filter)
			.Select(prop => toKeyValuePair(prop, key.AppendPointerStep(prop)))
			.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
	}

	private IReadOnlyList<T> ReadArray<T>(Key key, Func<Key, T> toItem, Action? ifNotArray = null)
	{
		if (key.Node is not JsonArray array)
		{
			ifNotArray?.Invoke();
			return Array.Empty<T>();
		}
		return array.Select((node, index) => key.AppendPointerStep(index.ToString())).Select(toItem).ToArray();
	}

	private Func<Key, T> AllowReference<T>(Func<Key, T> toItem)
	{
		return (key) =>
		{
			if (key.Node is not JsonObject obj) return toItem(key);
			if (obj["$ref"]?.GetValue<string>() is not string refName) return toItem(key);

			// TODO - load ref
			throw new NotImplementedException();
		};
	}
}
