using Json.Schema;
using PrincipleStudios.OpenApi.Transformations.Abstractions;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using System;
using System.Text.Json.Nodes;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_1;

internal class OpenApi3_1Parser : SchemaValidatingParser<IOpenApiDocument, OpenApi3_1Document>
{
	public OpenApi3_1Parser() : base(Json.Schema.OpenApi.MetaSchemas.DocumentSchema)
	{

	}

	public override bool CanParse(IDocumentReference documentReference)
	{
		if (documentReference.RootNode is not JsonObject jObject) return false;
		if (!jObject.TryGetPropertyValue("openapi", out var versionNode)) return false;
		if (versionNode is not JsonValue jValue) return false;
		if (!jValue.TryGetValue<string>(out var version)) return false;
		if (!version.StartsWith("3.1.")) return false;
		return true;
	}

	protected override IOpenApiDocument? Construct(IDocumentReference documentReference, EvaluationResults evaluationResults)
	{
		return new OpenApi3_1Document(documentReference.BaseUri, documentReference.RootNode ?? throw new InvalidOperationException(Errors.InvalidOpenApiRootNode));
	}
}
