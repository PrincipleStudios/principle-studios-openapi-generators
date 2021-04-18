using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApi.NetCore.ServerInterfaces
{
    public static class DocumentHelpers
    {
        public static OpenApiDocument GetDocument(int index)
        {
            var documentStream = typeof(CSharpSchemaTransformerShould).Assembly.GetManifestResourceStream($"PrincipleStudios.OpenApi.NetCore.ServerInterfaces.{GetDocumentName(index)}");
            var reader = new OpenApiStreamReader();
            return reader.Read(documentStream, out var openApiDiagnostic);
        }

        public static string GetDocumentName(int index)
        {
            return index switch
            {
                0 => "petstore.yaml",
                1 => "petstore3.json",
                _ => throw new ArgumentException(nameof(index)),
            };
        }

    }
}
