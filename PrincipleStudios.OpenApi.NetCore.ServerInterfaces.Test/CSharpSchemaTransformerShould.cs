using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Snapshooter.Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static PrincipleStudios.OpenApi.NetCore.ServerInterfaces.DocumentHelpers;

namespace PrincipleStudios.OpenApi.NetCore.ServerInterfaces
{

    public class CSharpSchemaTransformerShould
    {
        [Fact]
        public void RecognizeInlinedValues()
        {
            var document = GetDocument(0);

            var target = ConstructTarget(document);

            Assert.True(target.UseInline(document.Paths["/pets"].Operations[OperationType.Get].Parameters.SingleOrDefault(p => p.Name == "tags").Schema, document.Components));
            Assert.True(target.UseInline(document.Paths["/pets"].Operations[OperationType.Get].Parameters.SingleOrDefault(p => p.Name == "limit").Schema, document.Components));
            Assert.False(target.UseInline(document.Paths["/pets"].Operations[OperationType.Get].Responses["200"].Content["application/json"].Schema, document.Components));
            Assert.False(target.UseInline(document.Paths["/pets"].Operations[OperationType.Get].Responses["default"].Content["application/json"].Schema, document.Components));
            Assert.False(target.UseInline(document.Paths["/pets"].Operations[OperationType.Post].RequestBody.Content["application/json"].Schema, document.Components));
            Assert.False(target.UseInline(document.Paths["/pets"].Operations[OperationType.Post].Responses["200"].Content["application/json"].Schema, document.Components));

            Assert.True(target.UseInline(document.Paths["/pets/{id}"].Operations[OperationType.Get].Parameters.SingleOrDefault(p => p.Name == "id").Schema, document.Components));
            Assert.True(target.UseInline(document.Paths["/pets/{id}"].Operations[OperationType.Delete].Parameters.SingleOrDefault(p => p.Name == "id").Schema, document.Components));

            Assert.False(target.UseInline(document.Components.Schemas["Pet"], document.Components));
            Assert.False(target.UseInline(document.Components.Schemas["NewPet"], document.Components));
            Assert.False(target.UseInline(document.Components.Schemas["Error"], document.Components));
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

            var result = target.TransformComponentSchema(model, document.Components.Schemas[model]);

            Snapshot.Match(result.SourceText, $"{nameof(CSharpSchemaTransformerShould)}.{nameof(TransformModel)}.{CSharpNaming.ToTitleCaseIdentifier(documentName)}.{CSharpNaming.ToTitleCaseIdentifier(model)}");
        }

        private static CSharpSchemaTransformer ConstructTarget(OpenApiDocument document, string baseNamespace = "PrincipleStudios.Test")
        {
            return new CSharpSchemaTransformer(document, baseNamespace);
        }

    }
}
