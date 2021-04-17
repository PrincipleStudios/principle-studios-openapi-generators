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
        public static OpenApiDocument GetPetStoreOpenApiDocument()
        {
            var documentStream = typeof(CSharpSchemaTransformerShould).Assembly.GetManifestResourceStream("PrincipleStudios.OpenApi.NetCore.ServerInterfaces.petstore.yaml");
            var reader = new OpenApiStreamReader();
            return reader.Read(documentStream, out var openApiDiagnostic);
        }
    }
}
