using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using PrincipleStudios.OpenApi.Transformations;

namespace PrincipleStudios.OpenApi.CSharp
{
    public class MvcServerCodegenTask : Task
    {
#nullable disable warnings
        [Required]
        public string OutputPath { get; set; }

        [Required]
        public string InputPath { get; set; }

        [Required]
        public string Namespace { get; set; }

        public bool Clean { get; set; } = true;
#nullable enable warnings

        public override bool Execute()
        {
            var outputPath = OutputPath ?? System.IO.Directory.GetCurrentDirectory();
            System.IO.Directory.CreateDirectory(outputPath);
            if (Clean)
            {
                foreach (var file in System.IO.Directory.GetFiles(outputPath, "*.cs", System.IO.SearchOption.TopDirectoryOnly))
                {
                    System.IO.File.Delete(file);
                }
            }

            var openApiDocument = LoadOpenApiDocument(InputPath);
            if (openApiDocument == null)
                return false;

            var schemaTransformer = new CSharpPathControllerTransformer(openApiDocument, Namespace);
            var transformer = schemaTransformer.ToOpenApiSourceTransformer();

            var diagnostic = new OpenApiTransformDiagnostic();
            var entries = transformer.ToSourceEntries(openApiDocument, diagnostic).ToArray();
            foreach (var error in diagnostic.Errors)
            {
                Log.LogError(subcategory: null, errorCode: "PSOPENAPI000", helpKeyword: null, file: InputPath, lineNumber: 0, columnNumber: 0, endLineNumber: 0, endColumnNumber: 0, error.Message);
            }
            foreach (var entry in entries)
            {
                System.IO.File.WriteAllText(System.IO.Path.Combine(outputPath, entry.Key), entry.SourceText);
            }
            return true;
        }


        OpenApiDocument? LoadOpenApiDocument(string inputPath)
        {
            try
            {
                var openapiTextContent = System.IO.File.ReadAllText(inputPath);
                var reader = new OpenApiStringReader();
                var document = reader.Read(openapiTextContent, out var openApiDiagnostic);
                if (openApiDiagnostic.Errors.Any())
                {
                    Console.WriteLine($"Errors while parsing OpenApi spec ({inputPath}):");
                    foreach (var error in openApiDiagnostic.Errors)
                    {
                        Console.Error.WriteLine($"{inputPath}(1): error PSOPENAPI000: {error.Pointer}: {error.Message}");
                    }
                }

                return document;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unable to parse OpenApi spec ({inputPath}): {ex.Message}");

                return null;
            }
        }

    }
}
