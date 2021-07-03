using Microsoft.OpenApi.Models;
using Xunit;
using PrincipleStudios.OpenApi.Transformations;
using System.Linq;
using Snapshooter.Xunit;
using PrincipleStudios.OpenApi.TypeScript;
using static PrincipleStudios.OpenApiCodegen.TestUtils.DocumentHelpers;
using System.IO;
using Microsoft.OpenApi.Readers;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScriptRxJs
{
    using static OptionsHelpers;

    public class TypeScriptSchemaTransformerShould
    {
        [Theory]
        [InlineData(true, "petstore.yaml", "paths./pets.get.parameters[?(@.name=='tags')].schema")]
        [InlineData(true, "petstore.yaml", "paths./pets.get.parameters[?(@.name=='limit')].schema")]
        [InlineData(true, "petstore.yaml", "paths./pets.get.responses.200.content.application/json.schema")]
        [InlineData(false, "petstore.yaml", "paths./pets.get.responses.200.content.application/json.schema.items")]
        [InlineData(false, "petstore.yaml", "paths./pets.get.responses.default.content.application/json.schema")]
        [InlineData(false, "petstore.yaml", "paths./pets.post.requestBody.content.application/json.schema")]
        [InlineData(false, "petstore.yaml", "paths./pets.post.responses.200.content.application/json.schema")]
        [InlineData(true, "petstore.yaml", "paths./pets/{id}.get.parameters[?(@.name=='id')].schema")]
        [InlineData(true, "petstore.yaml", "paths./pets/{id}.delete.parameters[?(@.name=='id')].schema")]
        [InlineData(false, "petstore.yaml", "components.schemas.Pet")]
        [InlineData(false, "petstore.yaml", "components.schemas.NewPet")]
        [InlineData(false, "petstore.yaml", "components.schemas.Error")]
        [InlineData(true, "no-refs.yaml", "paths./address.post.requestBody.content.application/json.schema")]
        [InlineData(true, "no-refs.yaml", "paths./address.post.requestBody.content.application/json.schema.properties.location")]
        public void RecognizeInlinedValues(bool expectedInline, string documentName, string path)
        {
            var docContents = GetDocumentString(documentName);

            var (document, schema) = GetSchema(docContents, path);
            Assert.NotNull(document);
            Assert.NotNull(schema);

            var target = ConstructTarget(document!, LoadOptions());
            var actual = target.UseInline(schema!, document!);

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
                return (document, schema);
            }
            else
            {
                return (document, document.Components.Schemas[path.Substring(prefix.Length)]);
            }
        }

        //[Theory]
        //[InlineData("petstore.yaml", "Pet")]
        //[InlineData("petstore.yaml", "NewPet")]
        //[InlineData("petstore.yaml", "Error")]
        //[InlineData("petstore3.json", "Order")]
        //[InlineData("petstore3.json", "Category")]
        //[InlineData("petstore3.json", "User")]
        //[InlineData("petstore3.json", "Tag")]
        //[InlineData("petstore3.json", "Pet")]
        //[InlineData("petstore3.json", "ApiResponse")]
        //public void TransformModel(string documentName, string model)
        //{
        //    var document = GetDocument(documentName);
        //    var options = LoadOptions();

        //    var target = ConstructTarget(document, options);
        //    OpenApiTransformDiagnostic diagnostic = new();

        //    var context = OpenApiContext.From(document).Append(nameof(document.Components), null, document.Components).Append(nameof(document.Components.Schemas), model, document.Components.Schemas[model]);

        //    var result = target.TransformSchema(document.Components.Schemas[model], context, diagnostic);

        //    Snapshot.Match(result?.SourceText, $"Full-{nameof(TransformModel)}.{TypeScriptNaming.ToTitleCaseIdentifier(documentName, options.ReservedIdentifiers())}.{TypeScriptNaming.ToTitleCaseIdentifier(model, options.ReservedIdentifiers())}");
        //}

        private static TypeScriptSchemaSourceResolver ConstructTarget(OpenApiDocument document, TypeScriptSchemaOptions options, string baseNamespace = "PrincipleStudios.Test")
        {
            return new TypeScriptSchemaSourceResolver(baseNamespace, options, new HandlebarsFactory(HandlebarsTemplateProcess.CreateHandlebars), "");
        }

    }
}
