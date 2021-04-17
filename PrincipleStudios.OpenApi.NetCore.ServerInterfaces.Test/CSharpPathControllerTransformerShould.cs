using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.Transformations;
using Snapshooter.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static PrincipleStudios.OpenApi.NetCore.ServerInterfaces.DocumentHelpers;

namespace PrincipleStudios.OpenApi.NetCore.ServerInterfaces
{
    public class CSharpPathControllerTransformerShould
    {
        [Theory]
        [InlineData("/pets")]
        //[InlineData("/pets/{id}")]
        public void TransformController(string path)
        {
            var document = GetPetStoreOpenApiDocument();

            var target = ConstructTarget(document);

            var result = target.TransformController(path, document.Paths[path]).ToArray();

            Assert.Collection(result, t => Snapshot.Match(t.SourceText));
        }

        private static IOpenApiPathControllerTransformer ConstructTarget(OpenApiDocument document, string baseNamespace = "PrincipleStudios.Test")
        {
            return new CSharpPathControllerTransformer(document, baseNamespace);
        }

    }
}
