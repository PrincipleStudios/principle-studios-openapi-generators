using Microsoft.OpenApi.Models;
using Xunit;
using PrincipleStudios.OpenApi.Transformations;
using System.Linq;
using PrincipleStudios.OpenApi.TypeScript;
using static PrincipleStudios.OpenApiCodegen.TestUtils.DocumentHelpers;
using System.IO;
using Microsoft.OpenApi.Readers;
using System.Collections.Generic;
using System;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using Json.Pointer;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript
{
	using static OptionsHelpers;

	public class TypeScriptSchemaTransformerShould
	{
		[Theory]
		[InlineData(false, "proj://embedded/petstore.yaml#/paths/~1pets/get/parameters/0/schema")]
		[InlineData(false, "proj://embedded/petstore.yaml#/paths/~1pets/get/parameters/1/schema")]
		[InlineData(false, "proj://embedded/petstore.yaml#/paths/~1pets/get/responses/200/content/application~1json/schema")]
		[InlineData(true, "proj://embedded/petstore.yaml#/paths/~1pets/get/responses/200/content/application~1json/schema/items")]
		[InlineData(true, "proj://embedded/petstore.yaml#/paths/~1pets/get/responses/default/content/application~1json/schema")]
		[InlineData(true, "proj://embedded/petstore.yaml#/paths/~1pets/post/requestBody/content/application~1json/schema")]
		[InlineData(true, "proj://embedded/petstore.yaml#/paths/~1pets/post/responses/200/content/application~1json/schema")]
		[InlineData(false, "proj://embedded/petstore.yaml#/paths/~1pets~1{id}/get/parameters/0/schema")]
		[InlineData(false, "proj://embedded/petstore.yaml#/paths/~1pets~1{id}/delete/parameters/0/schema")]
		[InlineData(true, "proj://embedded/petstore.yaml#/components/schemas/Pet")]
		[InlineData(true, "proj://embedded/petstore.yaml#/components/schemas/NewPet")]
		[InlineData(true, "proj://embedded/petstore.yaml#/components/schemas/Error")]
		[InlineData(false, "proj://embedded/no-refs.yaml#/paths/~1address/post/requestBody/content/application~1json/schema")]
		[InlineData(false, "proj://embedded/no-refs.yaml#/paths/~1address/post/requestBody/content/application~1json/schema/properties/location")]
		public void Know_when_to_generate_source(bool expectedInline, string schemaUriString)
		{
			var schemaUri = new Uri(schemaUriString);
			var docRef = GetDocumentByUri(schemaUri);

			// TODO - use json schema instead
			var (document, schema) = GetSchema(docRef, Uri.UnescapeDataString(schemaUri.Fragment.Substring(1)));
			Assert.NotNull(document);
			Assert.NotNull(schema);

			var target = ConstructTarget(document!, LoadOptions());
			var actual = target.ProduceSourceEntry(schema!);

			Assert.Equal(expectedInline, actual);
		}

		[Theory]
		[InlineData(false, "petstore.yaml", "/paths/~1pets/get/parameters/0/schema")]
		[InlineData(false, "petstore.yaml", "/paths/~1pets/get/parameters/1/schema")]
		[InlineData(false, "petstore.yaml", "/paths/~1pets/get/responses/200/content/application~1json/schema")]
		[InlineData(true, "petstore.yaml", "/paths/~1pets/get/responses/200/content/application~1json/schema/items")]
		[InlineData(true, "petstore.yaml", "/paths/~1pets/get/responses/default/content/application~1json/schema")]
		[InlineData(true, "petstore.yaml", "/paths/~1pets/post/requestBody/content/application~1json/schema")]
		[InlineData(true, "petstore.yaml", "/paths/~1pets/post/responses/200/content/application~1json/schema")]
		[InlineData(false, "petstore.yaml", "/paths/~1pets~1{id}/get/parameters/0/schema")]
		[InlineData(false, "petstore.yaml", "/paths/~1pets~1{id}/delete/parameters/0/schema")]
		[InlineData(true, "petstore.yaml", "/components/schemas/Pet")]
		[InlineData(true, "petstore.yaml", "/components/schemas/NewPet")]
		[InlineData(true, "petstore.yaml", "/components/schemas/Error")]
		[InlineData(false, "no-refs.yaml", "/paths/~1address/post/requestBody/content/application~1json/schema")]
		[InlineData(false, "no-refs.yaml", "/paths/~1address/post/requestBody/content/application~1json/schema/properties/location")]
		public void KnowWhenToGenerateSource(bool expectedInline, string documentName, string path)
		{
			var docRef = GetDocumentReference(documentName);

			var (document, schema) = GetSchema(docRef, path);
			Assert.NotNull(document);
			Assert.NotNull(schema);

			var target = ConstructTarget(document!, LoadOptions());
			var actual = target.ProduceSourceEntry(schema!);

			Assert.Equal(expectedInline, actual);
		}

		private static (OpenApiDocument? document, OpenApiSchema? schema) GetSchema(IDocumentReference docRef, string path)
		{
			const string prefix = "/components/schemas/";
			var openApiReader = new OpenApiStringReader();
			var document = openApiReader.Read(docRef.RootNode?.ToJsonString(), out var docDiagnostic);

			if (path.StartsWith(prefix))
			{
				return (document, document.Components.Schemas[path.Substring(prefix.Length)]);
			}

			if (!JsonPointer.Parse(path).TryEvaluate(docRef.RootNode, out var node) || node == null)
			{
				return (document, null);
			}

			var schema = openApiReader.ReadFragment<OpenApiSchema>(node.ToJsonString(), ToSpecVersion((docRef.RootNode!.AsObject().TryGetPropertyValue("openapi", out var n) ? n : null)?.GetValue<string>()), out var openApiDiagnostic);
			if (schema.UnresolvedReference)
				schema = (OpenApiSchema)document.ResolveReference(schema.Reference);
			return (document, schema);
		}

		[Theory]
		[MemberData(nameof(InlineAssertionData))]
		public void ConvertToInlineTypes(string documentName, Func<OpenApiDocument, OpenApiSchema> findSchema, string expectedInline)
		{
			var document = GetDocument(documentName);

			var schema = findSchema(document);

			var target = ConstructTarget(document, LoadOptions());
			target.EnsureSchemasRegistered(document, OpenApiContext.From(document), new());
			var inline = target.ToInlineDataType(schema)();

			Assert.Equal(expectedInline, inline.text);
		}

		public static IEnumerable<object[]> InlineAssertionData =>
			new List<(string documentName, Func<OpenApiDocument, OpenApiSchema> findSchema, string expectedInline)>
			{
				("petstore.yaml", (OpenApiDocument doc) => doc.Paths["/pets"].Operations[OperationType.Get].Parameters.First(p => p.Name == "tags").Schema, "Array<string>"),
				("petstore.yaml", (OpenApiDocument doc) => doc.Paths["/pets"].Operations[OperationType.Get].Parameters.First(p => p.Name == "limit").Schema, "number"),
				("petstore.yaml", (OpenApiDocument doc) => doc.Paths["/pets"].Operations[OperationType.Get].Responses["200"].Content["application/json"].Schema, "Array<Pet>"),
				("petstore.yaml", (OpenApiDocument doc) => doc.Paths["/pets"].Operations[OperationType.Get].Responses["200"].Content["application/json"].Schema.Items, "Pet"),
				("petstore.yaml", (OpenApiDocument doc) => doc.Paths["/pets"].Operations[OperationType.Get].Responses["default"].Content["application/json"].Schema, "_Error"),
				("petstore.yaml", (OpenApiDocument doc) => doc.Paths["/pets"].Operations[OperationType.Post].RequestBody.Content["application/json"].Schema, "NewPet"),
				("petstore.yaml", (OpenApiDocument doc) => doc.Paths["/pets"].Operations[OperationType.Post].Responses["200"].Content["application/json"].Schema, "Pet"),
				("petstore.yaml", (OpenApiDocument doc) => doc.Paths["/pets/{id}"].Operations[OperationType.Get].Parameters.First(p => p.Name == "id").Schema, "BigInt"),
				("petstore.yaml", (OpenApiDocument doc) => doc.Paths["/pets/{id}"].Operations[OperationType.Delete].Parameters.First(p => p.Name == "id").Schema, "BigInt"),
				("petstore.yaml", (OpenApiDocument doc) => doc.Components.Schemas["Pet"], "Pet"),
				("petstore.yaml", (OpenApiDocument doc) => doc.Components.Schemas["NewPet"], "NewPet"),
				("petstore.yaml", (OpenApiDocument doc) => doc.Components.Schemas["Error"], "_Error"),
				("no-refs.yaml", (OpenApiDocument doc) => doc.Paths["/address"].Operations[OperationType.Post].RequestBody.Content["application/json"].Schema, "{ \"formattedAddress\": string; \"location\": { \"latitude\": number; \"longitude\": number } }"),
				("no-refs.yaml", (OpenApiDocument doc) => doc.Paths["/address"].Operations[OperationType.Post].RequestBody.Content["application/json"].Schema.Properties["location"], "{ \"latitude\": number; \"longitude\": number }"),
				("enum.yaml", (OpenApiDocument doc) => doc.Paths["/rock-paper-scissors"].Operations[OperationType.Post].Responses["200"].Content["application/json"].Schema, "\"player1\" | \"player2\""),
			}.Select(t => new object[] { t.documentName, t.findSchema, t.expectedInline });

		private static TypeScriptSchemaSourceResolver ConstructTarget(OpenApiDocument document, TypeScriptSchemaOptions options)
		{
			return new TypeScriptSchemaSourceResolver(options, new HandlebarsFactory(HandlebarsTemplateProcess.CreateHandlebars), "");
		}
	}
}
