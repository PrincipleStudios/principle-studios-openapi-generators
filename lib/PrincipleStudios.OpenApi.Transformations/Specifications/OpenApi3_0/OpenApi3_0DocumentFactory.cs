﻿using PrincipleStudios.OpenApi.Transformations.Abstractions;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using PrincipleStudios.OpenApi.Transformations.Specifications.Keywords;
using PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Validation;
using PrincipleStudios.OpenApi.Transformations.Specifications.Vocabularies;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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


	public static readonly Uri jsonSchemaMeta = new Uri("https://spec.openapis.example.org/oas/3.0/meta/base");
	public static readonly Uri jsonSchemaDialect = new Uri("https://spec.openapis.example.org/oas/3.0/dialect/base");
	private readonly DocumentRegistry documentRegistry;
	private readonly List<DiagnosticBase> diagnostics;

	public ICollection<DiagnosticBase> Diagnostics => diagnostics;
	public static IJsonSchemaVocabulary Vocabulary { get; }
	public static IJsonSchemaDialect OpenApiDialect { get; }

	static OpenApi3_0DocumentFactory()
	{
		//Vocabularies.Core201909.Keywords
		Vocabulary = new JsonSchemaVocabulary(
			jsonSchemaMeta,
			[
				// OpenAPI 3.0 is not truly JsonSchema compliant, which is why
				// this has its own Uri with "example" in it "type" must also be
				// included. See
				// https://swagger.io/docs/specification/data-models/keywords/
				("type", TypeKeyword.Instance),

				// Most of `Vocabularies.Validation202012Id` works, but the exclusiveMinimum / exclusiveMaximum work differently
				("multipleOf", MultipleOfKeyword.Instance),
				("minimum", MinimumKeyword.Instance),
				("maximum", MaximumKeyword.Instance),
				("exclusiveMinimum", OpenApi3_0.ExclusiveMinimumKeyword.Instance),
				("exclusiveMaimum", OpenApi3_0.ExclusiveMaximumKeyword.Instance),
				("maxLength", MaxLengthKeyword.Instance),
				("minLength", MinLengthKeyword.Instance),
				("pattern", PatternKeyword.Instance),
				("maxItems", MaxItemsKeyword.Instance),
				("minItems", MinItemsKeyword.Instance),
				("uniqueItems", UniqueItemsKeyword.Instance),
				("maxProperties", MaxPropertiesKeyword.Instance),
				("minProperties", MinPropertiesKeyword.Instance),
				("required", RequiredKeyword.Instance),
				("enum", EnumKeyword.Instance),
			]
		);
		OpenApiDialect =
			new JsonSchemaDialect(
				jsonSchemaDialect,
				[
					StandardVocabularies.Core202012,
					StandardVocabularies.Applicator202012,
					StandardVocabularies.Unevaluated202012,
					StandardVocabularies.Metadata202012,
					StandardVocabularies.FormatAnnotation202012,
					StandardVocabularies.Content202012,
					Vocabulary,
					// should be all of "https://spec.openapis.org/oas/3.0/schema/2021-09-28"
				],
				UnknownKeyword.Instance
			);
	}

	public OpenApi3_0DocumentFactory(DocumentRegistry documentRegistry, IEnumerable<DiagnosticBase> initialDiagnostics)
	{
		this.documentRegistry = documentRegistry;
		this.diagnostics = initialDiagnostics.ToList();
	}


	public OpenApiDocument ConstructDocument(IDocumentReference documentReference)
	{
		documentReference.Dialect = OpenApiDialect;
		return ConstructDocument(NodeMetadata.FromRoot(documentReference));
	}

	private OpenApiDocument ConstructDocument(NodeMetadata key)
	{
		if (key.Node is not JsonObject obj) throw new InvalidOperationException(Errors.InvalidOpenApiRootNode);
		return new OpenApiDocument(key.Id,
			OpenApiSpecVersion: new OpenApiSpecVersion("openapi", obj["openapi"]?.GetValue<string>() ?? "3.0.3"),
			Info: ConstructInfo(key.Navigate("info")),
			JsonSchemaDialect: jsonSchemaDialect,
			Paths: ConstructPaths(key.Navigate("paths"))
		);
	}

	private OpenApiContact? ConstructContact(NodeMetadata key) =>
		CatchDiagnostic(AllowNull(InternalConstructContact), (_) => null)(key);
	private OpenApiContact InternalConstructContact(NodeMetadata key)
	{
		if (key.Node is not JsonObject obj) throw new DiagnosticException(InvalidNode.Builder(nameof(OpenApiContact)));
		return new OpenApiContact(key.Id,
			Name: obj["name"]?.GetValue<string>(),
			Url: obj["url"]?.GetValue<string>() is string url
				&& Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var result)
					? result
					: null,
			Email: obj["email"]?.GetValue<string>()
		);
	}

	private OpenApiInfo ConstructInfo(NodeMetadata key) =>
		CatchDiagnostic(InternalConstructInfo, MissingRequiredFieldDefaults.ConstructPlaceholderInfo)(key);
	private OpenApiInfo InternalConstructInfo(NodeMetadata key)
	{
		if (key.Node is not JsonObject obj) throw new DiagnosticException(InvalidNode.Builder(nameof(OpenApiInfo)));
		return new OpenApiInfo(key.Id,
			Title: obj["title"]?.GetValue<string>() ?? MissingRequiredFieldDefaults.InfoTitle,
			Summary: null,
			Description: obj["description"]?.GetValue<string>(),
			TermsOfService: obj["termsOfService"]?.GetValue<string>() is string tos
				&& Uri.TryCreate(tos, UriKind.RelativeOrAbsolute, out var result)
					? result
					: null,
			Contact: obj["contact"] is JsonObject
				? ConstructContact(key.Navigate("contact"))
				: null,
			License: ConstructLicense(key.Navigate("license")),
			Version: obj["version"]?.GetValue<string>() ?? MissingRequiredFieldDefaults.InfoVersion
		);
	}

	private OpenApiLicense? ConstructLicense(NodeMetadata key) =>
		CatchDiagnostic(AllowNull(InternalConstructLicense), (_) => null)(key);
	private OpenApiLicense InternalConstructLicense(NodeMetadata key)
	{
		if (key.Node is not JsonObject obj) throw new DiagnosticException(InvalidNode.Builder(nameof(OpenApiLicense)));
		return new OpenApiLicense(key.Id,
			Name: obj["name"]?.GetValue<string>() ?? MissingRequiredFieldDefaults.LicenseName,
			Url: obj["url"]?.GetValue<string>() is string url
					&& Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var result)
						? result
						: null,
			Identifier: null
		);
	}

	private OpenApiMediaTypeObject ConstructMediaTypeObject(NodeMetadata key) =>
		CatchDiagnostic(InternalConstructMediaTypeObject, MissingRequiredFieldDefaults.ConstructPlaceholderMediaTypeObject)(key);
	// https://spec.openapis.org/oas/v3.0.0#media-type-object
	private OpenApiMediaTypeObject InternalConstructMediaTypeObject(NodeMetadata key)
	{
		if (key.Node is not JsonObject obj) throw new DiagnosticException(InvalidNode.Builder(nameof(OpenApiMediaTypeObject)));
		return new OpenApiMediaTypeObject(key.Id,
			Schema: ConstructSchema(key.Navigate("schema"))
		);
	}

	private OpenApiOperation ConstructOperation(NodeMetadata key) =>
		CatchDiagnostic(InternalConstructOperation, MissingRequiredFieldDefaults.ConstructPlaceholderOperation)(key);
	// https://spec.openapis.org/oas/v3.0.0#operationObject
	private OpenApiOperation InternalConstructOperation(NodeMetadata key)
	{
		if (key.Node is not JsonObject obj) throw new DiagnosticException(InvalidNode.Builder(nameof(OpenApiOperation)));
		return new OpenApiOperation(key.Id,
			Tags: ReadArray(key.Navigate("tags"), k => k.Node?.GetValue<string>() ?? MissingRequiredFieldDefaults.OperationTag),
			Summary: obj["summary"]?.GetValue<string>(),
			Description: obj["description"]?.GetValue<string>(),
			OperationId: obj["operationId"]?.GetValue<string>(),
			Parameters: ReadArray(key.Navigate("parameters"), ConstructParameter),
			RequestBody: ConstructRequestBody(key.Navigate("requestBody")),
			Responses: ConstructResponses(key.Navigate("responses")),
			Deprecated: obj["deprecated"]?.GetValue<bool>() ?? false
		);
	}

	private OpenApiParameter ConstructParameter(NodeMetadata key) =>
		CatchDiagnostic(AllowReference(InternalConstructParameter), MissingRequiredFieldDefaults.ConstructPlaceholderParameter)(key);
	// https://spec.openapis.org/oas/v3.0.0#parameterObject
	private OpenApiParameter InternalConstructParameter(NodeMetadata key)
	{
		if (key.Node is not JsonObject obj) throw new DiagnosticException(InvalidNode.Builder(nameof(OpenApiParameter)));

		var location = ToParameterLocation(obj["in"]);
		return LoadParameter(key, obj, location);
	}

	private OpenApiParameter LoadParameter(NodeMetadata key, JsonObject obj, ParameterLocation location)
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
					Schema: ConstructSchema(key.Navigate("schema"))
				);
	}

	private OpenApiParameter ConstructHeaderParameter(NodeMetadata key, string name) =>
		CatchDiagnostic(AllowReference(InternalConstructHeaderParameter), MissingRequiredFieldDefaults.ConstructPlaceholderParameter)(key) with { Name = name };
	private OpenApiParameter InternalConstructHeaderParameter(NodeMetadata key)
	{
		if (key.Node is not JsonObject obj) throw new DiagnosticException(InvalidNode.Builder(nameof(OpenApiParameter)));

		return LoadParameter(key, obj, ParameterLocation.Header);
	}

	private JsonSchema? ConstructSchema(NodeMetadata key) =>
		CatchDiagnostic(AllowReference(AllowNull(InternalConstructSchema)), (_) => null)(key);
	private JsonSchema InternalConstructSchema(NodeMetadata key)
	{
		var resolved = documentRegistry.ResolveSchema(key);
		return resolved.JsonSchema is JsonSchema schema
			? schema
			: throw new MultipleDiagnosticException(resolved.Diagnostics);
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

	private IReadOnlyDictionary<string, OpenApiPath> ConstructPaths(NodeMetadata key) =>
		CatchDiagnostic(InternalConstructPaths, (_) => new Dictionary<string, OpenApiPath>())(key);
	private IReadOnlyDictionary<string, OpenApiPath> InternalConstructPaths(NodeMetadata key)
	{
		if (key.Node is not JsonObject obj) throw new DiagnosticException(InvalidNode.Builder(nameof(OpenApiPath)));

		return ReadDictionary(key, filter: _ => true, toKeyValuePair: (prop, key) => (prop, ConstructPath(key)));
	}

	private OpenApiPath ConstructPath(NodeMetadata key) =>
		CatchDiagnostic(InternalConstructPath, MissingRequiredFieldDefaults.ConstructPlaceholderPath)(key);
	private OpenApiPath InternalConstructPath(NodeMetadata key)
	{
		if (key.Node is not JsonObject obj) throw new DiagnosticException(InvalidNode.Builder(nameof(OpenApiPath)));
		return new OpenApiPath(key.Id,
			Summary: obj["summary"]?.GetValue<string>(),
			Description: obj["description"]?.GetValue<string>(),
			Operations: ReadDictionary(key, validMethods.Contains, toKeyValuePair: (method, key) => (Key: method, Value: ConstructOperation(key)))
		);
	}

	private OpenApiRequestBody? ConstructRequestBody(NodeMetadata key) =>
		CatchDiagnostic(AllowReference(AllowNull(InternalConstructRequestBody)), (_) => null)(key);
	// https://spec.openapis.org/oas/v3.0.0#request-body-object
	private OpenApiRequestBody InternalConstructRequestBody(NodeMetadata key)
	{
		if (key.Node is not JsonObject obj) throw new DiagnosticException(InvalidNode.Builder(nameof(OpenApiRequestBody)));
		return new OpenApiRequestBody(
			Id: key.Id,
			Description: obj["description"]?.GetValue<string>(),
			Content: ConstructMediaContentDictionary(key.Navigate("content")),
			Required: obj["required"]?.GetValue<bool>() ?? false
		);
	}

	private IReadOnlyDictionary<string, OpenApiMediaTypeObject>? ConstructMediaContentDictionary(NodeMetadata key) =>
		CatchDiagnostic(AllowNull(InternalConstructMediaContentDictionary), (_) => null)(key);
	private IReadOnlyDictionary<string, OpenApiMediaTypeObject> InternalConstructMediaContentDictionary(NodeMetadata key)
	{
		if (key.Node is not JsonObject obj) throw new DiagnosticException(InvalidNode.Builder("OpenApiMediaTypeContent"));
		return ReadDictionary(key,
			filter: media => media.Contains('/'),
			toKeyValuePair: (media, key) => (Key: media, Value: ConstructMediaTypeObject(key))
		);
	}

	// https://spec.openapis.org/oas/v3.0.0#response-object
	private OpenApiResponse ConstructResponse(NodeMetadata key) =>
		CatchDiagnostic(AllowReference(InternalConstructResponse), MissingRequiredFieldDefaults.ConstructPlaceholderResponse)(key);
	private OpenApiResponse InternalConstructResponse(NodeMetadata key)
	{
		if (key.Node is not JsonObject obj) throw new DiagnosticException(InvalidNode.Builder(nameof(OpenApiResponse)));
		return new OpenApiResponse(key.Id,
			Description: obj["description"]?.GetValue<string>() ?? MissingRequiredFieldDefaults.ResponseDescription,
			Headers: ConstructHeaders(key.Navigate("headers")),
			Content: ConstructMediaContentDictionary(key.Navigate("content"))
		);
	}

	private OpenApiParameter[] ConstructHeaders(NodeMetadata key) =>
		CatchDiagnostic(InternalConstructHeaders, _ => Array.Empty<OpenApiParameter>())(key);
	private OpenApiParameter[] InternalConstructHeaders(NodeMetadata key)
	{
		if (key.Node == null) return Array.Empty<OpenApiParameter>();
		return ReadDictionary(
				key,
				_ => true,
				(name, key) => (name, ConstructHeaderParameter(key, name))
			).Values.ToArray();
	}

	// https://spec.openapis.org/oas/v3.0.0#responses-object
	private OpenApiResponses? ConstructResponses(NodeMetadata key) =>
		CatchDiagnostic(AllowNull(InternalConstructResponses), (_) => null)(key);
	private OpenApiResponses InternalConstructResponses(NodeMetadata key)
	{
		if (key.Node is not JsonObject obj) throw new DiagnosticException(InvalidNode.Builder(nameof(OpenApiResponses)));
		return new OpenApiResponses(Id: key.Id,
			Default: AllowNull(ConstructResponse)(key.Navigate("default")),
			StatusCodeResponses: ReadDictionary(key,
				filter: statusCode => statusCode.Length == 3 && int.TryParse(statusCode, NumberStyles.Integer, CultureInfo.InvariantCulture, out _),
				toKeyValuePair: (statusCode, key) => (Key: int.Parse(statusCode, NumberStyles.Integer, CultureInfo.InvariantCulture), Value: ConstructResponse(key))
			)
		);
	}

	private Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(NodeMetadata key, Func<string, bool> filter, Func<string, NodeMetadata, (TKey Key, TValue Value)> toKeyValuePair, Action? ifNotObject = null)
	{
		if (key.Node is not JsonObject obj)
		{
			ifNotObject?.Invoke();
			return new Dictionary<TKey, TValue>();
		}
		return obj
			.Select(kvp => kvp.Key)
			.Where(filter)
			.Select(prop => toKeyValuePair(prop, key.Navigate(prop)))
			.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
	}

	private T[] ReadArray<T>(NodeMetadata key, Func<NodeMetadata, T> toItem, Action? ifNotArray = null)
	{
		if (key.Node is not JsonArray array)
		{
			ifNotArray?.Invoke();
			return Array.Empty<T>();
		}
		return array.Select((node, index) => key.Navigate(index.ToString())).Select(toItem).ToArray();
	}

	private Func<NodeMetadata, T?> AllowNull<T>(Func<NodeMetadata, T> toItem)
	{
		return (key) =>
		{
			if (key.Node == null) return default;
			return toItem(key);
		};
	}

	private Func<NodeMetadata, T> AllowReference<T>(Func<NodeMetadata, T> toItem)
	{
		return (key) =>
		{
			if (key.Node is not JsonObject obj) return toItem(key);
			if (obj["$ref"]?.GetValue<string>() is not string refName) return toItem(key);

			if (!Uri.TryCreate(refName, UriKind.RelativeOrAbsolute, out var uri))
				throw new DiagnosticException(InvalidRefDiagnostic.Builder());

			var newKey = documentRegistry.ResolveMetadata(uri, key.Document);
			return toItem(newKey);
		};
	}

	private Func<NodeMetadata, T> CatchDiagnostic<T>(Func<NodeMetadata, T> toItem, Func<Uri, T> constructDefault)
	{
		return (key) =>
		{
			try
			{
				return toItem(key);
			}
			catch (DocumentException ex)
			{
				diagnostics.Add(ex.ConstructDiagnostic(key.Document.RetrievalUri));
			}
			catch (DiagnosticException ex)
			{
				diagnostics.Add(ex.ConstructDiagnostic(documentRegistry.ResolveLocation(key)));
			}
			catch (MultipleDiagnosticException ex)
			{
				diagnostics.AddRange(ex.Diagnostics);
			}
#pragma warning disable CA1031 // Catching a general exception type here to turn it into a diagnostic for reporting
			catch (Exception ex)
			{
				diagnostics.Add(new UnhandledExceptionDiagnostic(ex, documentRegistry.ResolveLocation(key)));
			}
#pragma warning restore CA1031 // Do not catch general exception types
			return constructDefault(key.Id);
		};
	}
}


public record UnableToResolveSchema(Location Location) : DiagnosticBase(Location)
{
	public static DiagnosticException.ToDiagnostic Builder() => (Location) => new UnableToResolveSchema(Location);
}

public record InvalidNode(string NodeType, Location Location) : DiagnosticBase(Location)
{
	public static DiagnosticException.ToDiagnostic Builder(string nodeType) => (Location) => new InvalidNode(nodeType, Location);
}

public record UnhandledExceptionDiagnostic(Exception Exception, Location Location) : DiagnosticBase(Location)
{
}