using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using PrincipleStudios.OpenApi.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc
{
    public static class DocumentHelpers
    {
        public static OpenApiDocument GetDocument(int index)
        {
            var documentStream = typeof(CSharpSchemaTransformerShould).Assembly.GetManifestResourceStream($"PrincipleStudios.OpenApiCodegen.Server.Mvc.{GetDocumentName(index)}");
            var reader = new OpenApiStreamReader();
            return reader.Read(documentStream, out var openApiDiagnostic);
        }

        public static string GetDocumentName(int index)
        {
            return index switch
            {
                0 => "petstore.yaml",
                1 => "petstore3.json",
                2 => "power-sample.json",
                3 => "no-refs.yaml",
                4 => "bad.yaml",
                5 => "oauth.yaml",
                _ => throw new ArgumentException(nameof(index)),
            };
        }


        public static CSharpSchemaOptions LoadOptions(Action<IConfigurationBuilder>? configureBuilder = null)
        {
            using var defaultJsonStream = CSharpSchemaOptions.GetDefaultOptionsJson();
            var builder = new ConfigurationBuilder();
            builder.AddYamlStream(defaultJsonStream);
            configureBuilder?.Invoke(builder);
            var result = builder.Build().Get<CSharpSchemaOptions>();
            return result;
        }

    }
}
