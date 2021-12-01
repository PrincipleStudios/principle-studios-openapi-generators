using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApiCodegen.TestUtils
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
            "empty.yaml",
            "controller-extension.yaml",
            "dictionary-ref.yaml",
            "enum.yaml",
            "array.yaml",
            "any.yaml",
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
            using var documentStream = typeof(DocumentHelpers).Assembly.GetManifestResourceStream($"PrincipleStudios.OpenApiCodegen.TestUtils.schemas.{name}");
            var reader = new OpenApiStreamReader();
            return reader.Read(documentStream, out var openApiDiagnostic);
        }

        public static string GetDocumentString(string name)
        {
            using var documentStream = typeof(DocumentHelpers).Assembly.GetManifestResourceStream($"PrincipleStudios.OpenApiCodegen.TestUtils.schemas.{name}");
            using var reader = new System.IO.StreamReader(documentStream!);
            return reader.ReadToEnd();
        }

        public static Microsoft.OpenApi.OpenApiSpecVersion ToSpecVersion(string? inputVersion)
        {
            switch (inputVersion)
            {
                case string version when version == "2.0":
                    return Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0;

                case string version when version.StartsWith("3.0"):
                    return Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;

                default:
                    throw new NotSupportedException(inputVersion);
            }
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



    }
}
