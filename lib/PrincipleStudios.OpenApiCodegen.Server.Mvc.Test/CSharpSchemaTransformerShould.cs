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
            var document = GetDocument(0);

            var target = ConstructTarget(document);

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
        [InlineData(0, "Pet")]
        [InlineData(0, "NewPet")]
        [InlineData(0, "Error")]
        [InlineData(1, "Order")]
        [InlineData(1, "Category")]
        [InlineData(1, "User")]
        [InlineData(1, "Tag")]
        [InlineData(1, "Pet")]
        [InlineData(1, "ApiResponse")]
        public void TransformModel(int documentId, string model)
        {
            var documentName = GetDocumentName(documentId);
            var document = GetDocument(documentId);

            var target = ConstructTarget(document);
            OpenApiTransformDiagnostic diagnostic = new();

            var result = target.TransformSchema(document.Components.Schemas[model], diagnostic);

            Snapshot.Match(result.SourceText, $"Full-{nameof(TransformModel)}.{CSharpNaming.ToTitleCaseIdentifier(documentName)}.{CSharpNaming.ToTitleCaseIdentifier(model)}");
        }

        private static CSharpSchemaTransformer ConstructTarget(OpenApiDocument document, string baseNamespace = "PrincipleStudios.Test")
        {
            return new CSharpSchemaTransformer(document, baseNamespace, HandlebarsTemplateProcess.CreateHandlebars);
        }

    }
}
