using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using PrincipleStudios.OpenApi.CSharp;
using PrincipleStudios.OpenApi.Transformations;
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
		[InlineData(false, "petstore.yaml", "paths./pets.get.parameters[?(@.name=='tags')].schema")]
		[InlineData(false, "petstore.yaml", "paths./pets.get.parameters[?(@.name=='limit')].schema")]
		[InlineData(false, "petstore.yaml", "paths./pets.get.responses.200.content.application/json.schema")]
		[InlineData(true, "petstore.yaml", "paths./pets.get.responses.default.content.application/json.schema")]
		[InlineData(true, "petstore.yaml", "paths./pets.post.requestBody.content.application/json.schema")]
		[InlineData(true, "petstore.yaml", "paths./pets.post.responses.200.content.application/json.schema")]
		[InlineData(false, "petstore.yaml", "paths./pets/{id}.get.parameters[?(@.name=='id')].schema")]
		[InlineData(false, "petstore.yaml", "paths./pets/{id}.delete.parameters[?(@.name=='id')].schema")]
		[InlineData(true, "petstore.yaml", "components.schemas.Pet")]
		[InlineData(true, "petstore.yaml", "components.schemas.NewPet")]
		[InlineData(true, "petstore.yaml", "components.schemas.Error")]
		[InlineData(true, "no-refs.yaml", "paths./address.post.requestBody.content.application/json.schema")]
		[InlineData(true, "no-refs.yaml", "paths./address.post.requestBody.content.application/json.schema.properties.location")]
		public void DetermineWhenToGenerateSource(bool expectedInline, string documentName, string path)
		{
			var docContents = GetDocumentString(documentName);

			var (document, schema) = GetSchema(docContents, path);
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
				Entry("FindPetsByStatusStatusItem", "petstore3.json", doc =>
					doc.Paths["/pet/findByStatus"].Operations[OperationType.Get].Parameters.First(p => p.Name == "status").Schema.Items),
			};

			object[] Entry(string expectedName, string documentName, Func<OpenApiDocument, OpenApiSchema> locateSchema) =>
				new object[] { expectedName, documentName, locateSchema };
		}

		private (OpenApiDocument? document, OpenApiSchema? schema) GetSchema(string docContents, string path)
		{
			const string prefix = "components.schemas.";
			var openApiReader = new OpenApiStringReader();
			var document = openApiReader.Read(docContents, out var docDiagnostic);

			if (!path.StartsWith(prefix))
			{
				using var reader = new StringReader(docContents);
				var serializer = new SharpYaml.Serialization.Serializer();
				var deserialized = serializer.Deserialize(reader);
				Newtonsoft.Json.Linq.JToken documentJObject = deserialized == null
					? Newtonsoft.Json.Linq.JValue.CreateNull()
					: Newtonsoft.Json.Linq.JObject.FromObject(deserialized);
				var token = documentJObject.SelectToken(path);
				if (token == null)
				{
					return (document, null);
				}

				var schema = openApiReader.ReadFragment<OpenApiSchema>(token.ToString(), ToSpecVersion((documentJObject["openapi"] ?? documentJObject["swagger"])?.ToObject<string>()), out var openApiDiagnostic);
				if (schema.UnresolvedReference)
					schema = (OpenApiSchema)document.ResolveReference(schema.Reference);
				return (document, schema);
			}
			else
			{
				return (document, document.Components.Schemas[path.Substring(prefix.Length)]);
			}
		}

		private static CSharpSchemaSourceResolver ConstructTarget(OpenApiDocument document, CSharpSchemaOptions options, string baseNamespace = "PrincipleStudios.Test")
		{
			return new CSharpSchemaSourceResolver(baseNamespace, options, new HandlebarsFactory(HandlebarsTemplateProcess.CreateHandlebars), "");
		}

	}
}
