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
        [InlineData(0, "/pets")]
        [InlineData(0, "/pets/{id}")]
        public void TransformController(int documentId, string path)
        {
            var documentName = GetDocumentName(documentId);
            var document = GetDocument(documentId);

            var target = ConstructTarget(document);

            var result = target.TransformController(path, document.Paths[path]);

            Snapshot.Match(result.SourceText, $"{nameof(CSharpPathControllerTransformerShould)}.{nameof(TransformController)}.{CSharpNaming.ToTitleCaseIdentifier(documentName)}.{CSharpNaming.ToTitleCaseIdentifier(path)}");
        }

        private static IOpenApiPathControllerTransformer ConstructTarget(OpenApiDocument document, string baseNamespace = "PrincipleStudios.Test")
        {
            return new CSharpPathControllerTransformer(document, baseNamespace);
        }

    }
}
