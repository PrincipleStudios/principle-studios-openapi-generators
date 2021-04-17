using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.Transformations;

#nullable enable

namespace PrincipleStudios.OpenApi.NetCore.ServerInterfaces
{
    [Generator]
    public class ControllerGenerator : OpenApiGeneratorBase<ControllerGenerator.Options>
    {
        const string propEnabled = "OpenApiServerInterfaceEnabled";
        const string propNamespace = "OpenApiServerInterfaceNamespace";

        public ControllerGenerator() : base(propEnabled)
        {
        }

        public record Options(OpenApiDocument Document, string DocumentNamespace) : OpenApiGeneratorOptions(Document);


        protected override IEnumerable<SourceEntry> SourceFilesFromAdditionalFile(Options options)
        {
            var handlebars = HandlebarsTemplateProcess.CreateHandlebars();

            var schemaTransformer = new CSharpSchemaTransformer(options.Document, options.DocumentNamespace);
            var transformer = new CombineOpenApiSourceTransformer(
                new SchemaSourceTransformer(schemaTransformer)
            );

            return transformer.ToSourceEntries(options.Document);
        }

        protected override bool TryCreateOptions(AdditionalText file, OpenApiDocument document, AnalyzerConfigOptions opt, GeneratorExecutionContext context, [NotNullWhen(true)] out Options? result)
        {
            if (!opt.TryGetValue($"build_metadata.additionalfiles.{propNamespace}", out var documentNamespace))
            {
                // TODO - report missing namespace
                result = null;
                return false;
            }

            result = new Options(document, documentNamespace);
            return true;
        }

    }
}
