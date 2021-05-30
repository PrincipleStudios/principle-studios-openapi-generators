using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using PrincipleStudios.OpenApi.CSharp;
using PrincipleStudios.OpenApi.Transformations;
using Snapshooter.Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static PrincipleStudios.OpenApiCodegen.Server.Mvc.DocumentHelpers;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc
{

    public class CSharpSchemaTransformerShould
    {
        [Fact]
        public void RecognizeInlinedValues()
        {
            var document = GetDocument("petstore.yaml");

            var target = ConstructTarget(document, LoadOptions());

            Assert.True(target.UseInline(document.Paths["/pets"].Operations[OperationType.Get].Parameters.Single(p => p.Name == "tags").Schema));
            Assert.True(target.UseInline(document.Paths["/pets"].Operations[OperationType.Get].Parameters.Single(p => p.Name == "limit").Schema));
            Assert.False(target.UseInline(document.Paths["/pets"].Operations[OperationType.Get].Responses["200"].Content["application/json"].Schema));
            Assert.False(target.UseInline(document.Paths["/pets"].Operations[OperationType.Get].Responses["default"].Content["application/json"].Schema));
            Assert.False(target.UseInline(document.Paths["/pets"].Operations[OperationType.Post].RequestBody.Content["application/json"].Schema));
            Assert.False(target.UseInline(document.Paths["/pets"].Operations[OperationType.Post].Responses["200"].Content["application/json"].Schema));

            Assert.True(target.UseInline(document.Paths["/pets/{id}"].Operations[OperationType.Get].Parameters.Single(p => p.Name == "id").Schema));
            Assert.True(target.UseInline(document.Paths["/pets/{id}"].Operations[OperationType.Delete].Parameters.Single(p => p.Name == "id").Schema));

            Assert.False(target.UseInline(document.Components.Schemas["Pet"]));
            Assert.False(target.UseInline(document.Components.Schemas["NewPet"]));
            Assert.False(target.UseInline(document.Components.Schemas["Error"]));
        }

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

            var context = OpenApiContext.From(document).Append(nameof(document.Components), null, document.Components).Append(nameof(document.Components.Schemas), model, document.Components.Schemas[model]);

            var result = target.TransformSchema(document.Components.Schemas[model], context, diagnostic);

            Snapshot.Match(result?.SourceText, $"Full-{nameof(TransformModel)}.{CSharpNaming.ToTitleCaseIdentifier(documentName, options.ReservedIdentifiers())}.{CSharpNaming.ToTitleCaseIdentifier(model, options.ReservedIdentifiers())}");
        }

        private static CSharpSchemaSourceResolver ConstructTarget(OpenApiDocument document, CSharpSchemaOptions options, string baseNamespace = "PrincipleStudios.Test")
        {
            return new CSharpSchemaSourceResolver(baseNamespace, options, new HandlebarsFactory(HandlebarsTemplateProcess.CreateHandlebars), "");
        }

    }
}
