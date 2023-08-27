using Json.Pointer;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using PrincipleStudios.OpenApi.CSharp;
using PrincipleStudios.OpenApi.Transformations;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using static PrincipleStudios.OpenApiCodegen.TestUtils.DocumentHelpers;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc
{
	using static OptionsHelpers;

	public class CSharpSchemaTransformerShould
	{
		public delegate OpenApiSchema SchemaAccessor(OpenApiDocument document);
		[Theory]
		[InlineData(false, "petstore.yaml", "/paths/~1pets/get/parameters/0/schema")]
		[InlineData(false, "petstore.yaml", "/paths/~1pets/get/parameters/1/schema")]
		[InlineData(false, "petstore.yaml", "/paths/~1pets/get/responses/200/content/application~1json/schema")]
		[InlineData(true, "petstore.yaml", "/paths/~1pets/get/responses/default/content/application~1json/schema")]
		[InlineData(true, "petstore.yaml", "/paths/~1pets/post/requestBody/content/application~1json/schema")]
		[InlineData(true, "petstore.yaml", "/paths/~1pets/post/responses/200/content/application~1json/schema")]
		[InlineData(false, "petstore.yaml", "/paths/~1pets~1{id}/get/parameters/0/schema")]
		[InlineData(false, "petstore.yaml", "/paths/~1pets~1{id}/delete/parameters/0/schema")]
		[InlineData(true, "petstore.yaml", "/components/schemas/Pet")]
		[InlineData(true, "petstore.yaml", "/components/schemas/NewPet")]
		[InlineData(true, "petstore.yaml", "/components/schemas/Error")]
		[InlineData(true, "no-refs.yaml", "/paths/~1address/post/requestBody/content/application~1json/schema")]
		[InlineData(true, "no-refs.yaml", "/paths/~1address/post/requestBody/content/application~1json/schema/properties/location")]
		public void DetermineWhenToGenerateSource(bool expectedInline, string documentName, string path)
		{
			var docRef = GetDocumentReference(documentName);

			var (document, schema) = GetSchema(docRef, path);
			Assert.NotNull(document);
			Assert.NotNull(schema);

			var target = ConstructTarget(document!, LoadOptions());
			target.EnsureSchemasRegistered(document!, OpenApiContext.From(document!), new());

			var actual = target.ProduceSourceEntry(schema!);

			Assert.Equal(expectedInline, actual);
		}

		[Theory]
		[MemberData(nameof(DetermineSchemaNameList))]
		public void Determine_a_name_for_schema(string expectedName, string documentName, Func<OpenApiDocument, OpenApiSchema> locateSchema)
		{
			var document = GetDocument(documentName);
			var schema = locateSchema(document);

			Assert.NotNull(document);
			Assert.NotNull(schema);

			var target = ConstructTarget(document!, LoadOptions());
			target.EnsureSchemasRegistered(document!, OpenApiContext.From(document!), new());

			var actual = target.ToInlineDataType(schema!)();

			Assert.Equal(expectedName, actual?.text);
		}

		public static IEnumerable<object[]> DetermineSchemaNameList()
		{
			return new[]
			{
				// TODO: swap to uri to get schema
				Entry("FindPetsByStatusStatusItem", "petstore3.json", doc =>
					doc.Paths["/pet/findByStatus"].Operations[OperationType.Get].Parameters.First(p => p.Name == "status").Schema.Items),
			};

			object[] Entry(string expectedName, string documentName, Func<OpenApiDocument, OpenApiSchema> locateSchema) =>
				new object[] { expectedName, documentName, locateSchema };
		}

		private static (OpenApiDocument? document, OpenApiSchema? schema) GetSchema(IDocumentReference docRef, string path)
		{
			var openApiReader = new OpenApiStringReader();
			var document = openApiReader.Read(docRef.RootNode?.ToJsonString(), out var docDiagnostic);

			if (!JsonPointer.Parse(path).TryEvaluate(docRef.RootNode, out var node) || node == null)
			{
				return (document, null);
			}

			var schema = openApiReader.ReadFragment<OpenApiSchema>(node.ToJsonString(), ToSpecVersion((docRef.RootNode!.AsObject().TryGetPropertyValue("openapi", out var n) ? n : null)?.GetValue<string>()), out var openApiDiagnostic);
			if (schema.UnresolvedReference)
				schema = (OpenApiSchema)document.ResolveReference(schema.Reference);
			return (document, schema);
		}

		private static CSharpSchemaSourceResolver ConstructTarget(OpenApiDocument document, CSharpSchemaOptions options, string baseNamespace = "PrincipleStudios.Test")
		{
			return new CSharpSchemaSourceResolver(baseNamespace, options, new HandlebarsFactory(HandlebarsTemplateProcess.CreateHandlebars), "");
		}

	}
}
