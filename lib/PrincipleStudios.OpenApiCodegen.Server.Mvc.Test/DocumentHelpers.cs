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
        private static readonly string[] ValidDocumentNames = new[]
        {
            "petstore.yaml",
            "petstore3.json",
            "power-sample.json",
            "no-refs.yaml",
            "oauth.yaml",
            "headers.yaml",
        };

        private static readonly string[] InvalidDocumentNames = new[]
        {
            "bad.yaml",
        };

        public static IEnumerable<int> GetValidDocumentIndices() => Enumerable.Range(0, ValidDocumentNames.Length);
        public static IEnumerable<int> GetInvalidDocumentIndices() => Enumerable.Range(ValidDocumentNames.Length, InvalidDocumentNames.Length);

        public static OpenApiDocument GetDocument(int index)
        {
            return GetDocument(GetDocumentName(index));
        }

        public static OpenApiDocument GetDocument(string name)
        {
            var documentStream = typeof(CSharpSchemaTransformerShould).Assembly.GetManifestResourceStream($"PrincipleStudios.OpenApiCodegen.Server.Mvc.{name}");
            var reader = new OpenApiStreamReader();
            return reader.Read(documentStream, out var openApiDiagnostic);
        }

        public static string GetDocumentName(int index)
        {
            return index switch
            {
                >= 0 when index < ValidDocumentNames.Length => ValidDocumentNames[index],
                >= 0 when index < ValidDocumentNames.Length + InvalidDocumentNames.Length => InvalidDocumentNames[index - ValidDocumentNames.Length],
                _ => throw new ArgumentException(nameof(index))
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
