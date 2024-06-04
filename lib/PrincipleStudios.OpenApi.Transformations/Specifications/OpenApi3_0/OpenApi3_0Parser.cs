using PrincipleStudios.OpenApi.Transformations.Abstractions;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0;

internal class OpenApi3_0Parser : SchemaValidatingParser<OpenApiDocument>
{
	public OpenApi3_0Parser(DocumentRegistry registry) : base(LoadOpenApi3_0Schema(registry))
	{
	}

	private static JsonSchema LoadOpenApi3_0Schema(DocumentRegistry registry)
	{
		using var schemaStream = typeof(OpenApi3_0Parser).Assembly.GetManifestResourceStream($"{typeof(OpenApi3_0Parser).Namespace}.Schemas.schema.yaml");
		var yamlDocument = new YamlDocumentLoader().LoadDocument(new Uri("https://spec.openapis.org/oas/3.0/schema/2021-09-28"), schemaStream);
		var metadata = new NodeMetadata(yamlDocument.BaseUri, yamlDocument.RootNode, yamlDocument);

		var result = JsonSchemaParser.Deserialize(metadata, new JsonSchemaParserOptions(registry, OpenApi3_0DocumentFactory.OpenApiDialect));
		return result is DiagnosableResult<JsonSchema>.Success { Value: var schema }
			? schema
			: throw new InvalidOperationException(Errors.FailedToParseEmbeddedSchema);
	}


	public override bool CanParse(IDocumentReference documentReference)
	{
		if (documentReference.RootNode is not JsonObject jObject) return false;
		if (!jObject.TryGetPropertyValue("openapi", out var versionNode)) return false;
		if (versionNode is not JsonValue jValue) return false;
		if (!jValue.TryGetValue<string>(out var version)) return false;
		if (!version.StartsWith("3.0.")) return false;
		return true;
	}

	protected override ParseResult<OpenApiDocument> Construct(IDocumentReference documentReference, IEnumerable<DiagnosticBase> diagnostics, DocumentRegistry documentRegistry)
	{
		var factory = new OpenApi3_0DocumentFactory(documentRegistry, diagnostics);
		var result = factory.ConstructDocument(documentReference);
		return new ParseResult<OpenApiDocument>(
			result,
			factory.Diagnostics.ToArray()
		);
	}
}
