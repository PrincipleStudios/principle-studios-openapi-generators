using Json.Schema;
using PrincipleStudios.OpenApi.Transformations.Abstractions;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Yaml2JsonNode;
using YamlDotNet.RepresentationModel;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_0;

internal class OpenApi3_0Parser : SchemaValidatingParser<IOpenApiDocument, OpenApi3_0Document>
{
	public OpenApi3_0Parser() : base(LoadOpenApi3_0Schema())
	{

	}

	private static JsonSchema LoadOpenApi3_0Schema()
	{
		using var schemaStream = typeof(OpenApi3_0Parser).Assembly.GetManifestResourceStream($"{typeof(OpenApi3_0Parser).Namespace}.Schemas.schema.yaml");
		var yamlStream = new YamlStream();
		using var sr = new StreamReader(schemaStream);
		yamlStream.Load(sr);

		return JsonSerializer.Deserialize<JsonSchema>(yamlStream.Documents[0].ToJsonNode())
			?? throw new InvalidOperationException("Unable to parse OpenApi 3.0 embedded schema");
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

	protected override IOpenApiDocument? Construct(IDocumentReference documentReference, EvaluationResults evaluationResults)
	{
		// TODO
		return new OpenApi3_0Document(documentReference.RootNode);
	}
}
