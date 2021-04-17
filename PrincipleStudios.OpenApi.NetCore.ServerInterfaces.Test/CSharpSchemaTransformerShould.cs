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

namespace PrincipleStudios.OpenApi.NetCore.ServerInterfaces
{
    public class CSharpSchemaTransformerShould
    {
        [Fact]
        public void RecognizeInlinedValues()
        {
            var document = GetPetStoreOpenApiDocument();

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

        [Fact]
        public void TransformThePetModel()
        {
            var document = GetPetStoreOpenApiDocument();

            var target = ConstructTarget(document);

            var result = target.TransformComponentSchema("Pet", document.Components.Schemas["Pet"]);

            Snapshot.Match(result.SourceText);
        }

        [Fact]
        public void TransformTheNewPetModel()
        {
            var document = GetPetStoreOpenApiDocument();

            var target = ConstructTarget(document);

            var result = target.TransformComponentSchema("NewPet", document.Components.Schemas["NewPet"]);

            Snapshot.Match(result.SourceText);
        }

        [Fact]
        public void TransformTheErrorModel()
        {
            var document = GetPetStoreOpenApiDocument();

            var target = ConstructTarget(document);

            var result = target.TransformComponentSchema("Error", document.Components.Schemas["Error"]);

            Snapshot.Match(result.SourceText);
        }

        private static CSharpSchemaTransformer ConstructTarget(OpenApiDocument document, string baseNamespace = "PrincipleStudios.Test")
        {
            return new CSharpSchemaTransformer(document, baseNamespace);
        }

        private static OpenApiDocument GetPetStoreOpenApiDocument()
        {
            var documentStream = typeof(CSharpSchemaTransformerShould).Assembly.GetManifestResourceStream("PrincipleStudios.OpenApi.NetCore.ServerInterfaces.petstore.yaml");
            var reader = new OpenApiStreamReader();
            return reader.Read(documentStream, out var openApiDiagnostic);
        }
    }
}
