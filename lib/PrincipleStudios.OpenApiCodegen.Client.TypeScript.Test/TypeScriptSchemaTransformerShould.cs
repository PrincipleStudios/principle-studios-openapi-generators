using Microsoft.OpenApi.Models;
using Xunit;
using PrincipleStudios.OpenApi.Transformations;
using System.Linq;
using Snapshooter.Xunit;
using PrincipleStudios.OpenApi.TypeScript;
using static PrincipleStudios.OpenApiCodegen.TestUtils.DocumentHelpers;
using System.IO;
using Microsoft.OpenApi.Readers;
using System.Collections.Generic;
using System;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript
{
    using static OptionsHelpers;

    public class TypeScriptSchemaTransformerShould
    {
        [Theory]
        [InlineData(false, "petstore.yaml", "paths./pets.get.parameters[?(@.name=='tags')].schema")]
        [InlineData(false, "petstore.yaml", "paths./pets.get.parameters[?(@.name=='limit')].schema")]
        [InlineData(false, "petstore.yaml", "paths./pets.get.responses.200.content.application/json.schema")]
        [InlineData(true, "petstore.yaml", "paths./pets.get.responses.200.content.application/json.schema.items")]
        [InlineData(true, "petstore.yaml", "paths./pets.get.responses.default.content.application/json.schema")]
        [InlineData(true, "petstore.yaml", "paths./pets.post.requestBody.content.application/json.schema")]
        [InlineData(true, "petstore.yaml", "paths./pets.post.responses.200.content.application/json.schema")]
        [InlineData(false, "petstore.yaml", "paths./pets/{id}.get.parameters[?(@.name=='id')].schema")]
        [InlineData(false, "petstore.yaml", "paths./pets/{id}.delete.parameters[?(@.name=='id')].schema")]
        [InlineData(true, "petstore.yaml", "components.schemas.Pet")]
        [InlineData(true, "petstore.yaml", "components.schemas.NewPet")]
        [InlineData(true, "petstore.yaml", "components.schemas.Error")]
        [InlineData(false, "no-refs.yaml", "paths./address.post.requestBody.content.application/json.schema")]
        [InlineData(false, "no-refs.yaml", "paths./address.post.requestBody.content.application/json.schema.properties.location")]
        public void KnowWhenToGenerateSource(bool expectedInline, string documentName, string path)
        {
            var docContents = GetDocumentString(documentName);

            var (document, schema) = GetSchema(docContents, path);
            Assert.NotNull(document);
            Assert.NotNull(schema);

            var target = ConstructTarget(document!, LoadOptions());
            var actual = target.ProduceSourceEntry(schema!);

            Assert.Equal(expectedInline, actual);
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
                var documentJObject = Newtonsoft.Json.Linq.JObject.FromObject(serializer.Deserialize(reader));
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

        [Theory]
        [MemberData(nameof(InlineAssertionData))]
        public void ConvertToInlineTypes(string documentName, Func<OpenApiDocument, OpenApiSchema> findSchema, string expectedInline)
        {
            var docContents = GetDocumentString(documentName);
            var openApiReader = new OpenApiStringReader();
            var document = openApiReader.Read(docContents, out var docDiagnostic);

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
            }.Select(t => new object[] { t.documentName, t.findSchema, t.expectedInline });

        [Theory]
        [InlineData("petstore.yaml", "Pet")]
        [InlineData("petstore.yaml", "NewPet")]
        [InlineData("petstore.yaml", "Error")]
        [InlineData("petstore3.json", "Order")]
        [InlineData("petstore3.json", "Category")]
        [InlineData("petstore3.json", "User")]
        [InlineData("petstore3.json", "Tag")]
        [InlineData("petstore3.json", "Pet")]
        [InlineData("petstore3.json", "ApiResponse")]
        public void TransformModel(string documentName, string model)
        {
            var document = GetDocument(documentName);
            var options = LoadOptions();

            var target = ConstructTarget(document, options);
            OpenApiTransformDiagnostic diagnostic = new();
            target.EnsureSchemasRegistered(document, OpenApiContext.From(document), diagnostic);

            var context = OpenApiContext.From(document).Append(nameof(document.Components), null, document.Components).Append(nameof(document.Components.Schemas), model, document.Components.Schemas[model]);

            var result = target.TransformSchema(document.Components.Schemas[model], context, diagnostic);

            Snapshot.Match(result?.SourceText, $"Full-{nameof(TransformModel)}.{TypeScriptNaming.ToTitleCaseIdentifier(documentName, options.ReservedIdentifiers())}.{TypeScriptNaming.ToTitleCaseIdentifier(model, options.ReservedIdentifiers())}");
        }

        private static TypeScriptSchemaSourceResolver ConstructTarget(OpenApiDocument document, TypeScriptSchemaOptions options)
        {
            return new TypeScriptSchemaSourceResolver(options, new HandlebarsFactory(HandlebarsTemplateProcess.CreateHandlebars), "");
        }

    }
}
