using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using PrincipleStudios.OpenApi.Transformations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

#nullable enable

namespace PrincipleStudios.OpenApi.NetCore.ServerInterfaces
{
    public abstract class OpenApiGeneratorBase<TOptions> : ISourceGenerator
        where TOptions : OpenApiGeneratorOptions
    {
        private readonly string flagKey;

        public OpenApiGeneratorBase(string flagKey)
        {
            this.flagKey = flagKey;
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var options = GetLoadOptions(context);
            var nameCodeSequence = SourceFilesFromAdditionalFiles(options);
            foreach (var entry in nameCodeSequence)
                context.AddSource($"PrincipleStudios_NetCore_ServerInterfaces_{entry.Key}", SourceText.From(entry.SourceText, Encoding.UTF8));
        }


        public void Initialize(GeneratorInitializationContext context)
        {
        }

        private IEnumerable<TOptions> GetLoadOptions(GeneratorExecutionContext context)
        {
            foreach (AdditionalText file in context.AdditionalFiles)
            {
                var extension = Path.GetExtension(file.Path);
                if (!extension.Equals(".json", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase))
                    continue;

                var opt = context.AnalyzerConfigOptions.GetOptions(file);

                opt.TryGetValue($"build_metadata.additionalfiles.{flagKey}", out string? parseForThisGeneratorText);
                if (!bool.TryParse(parseForThisGeneratorText, out bool parseForThisGenerator) || !parseForThisGenerator)
                    continue;

                if (TryParseFile(file, out var document, out var diagnostic))
                {
                    if (TryCreateOptions(file, document, opt, context, out var fileOptions))
                        yield return fileOptions;
                }
                else if (diagnostic != null)
                {
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static bool TryParseFile(AdditionalText file, [NotNullWhen(true)] out OpenApiDocument? document, out Diagnostic? diagnostic)
        {
            document = null;
            diagnostic = null;
            try
            {
                var openapiTextContent = file.GetText()!.ToString();
                var reader = new OpenApiStringReader();
                document = reader.Read(openapiTextContent, out var openApiDiagnostic);
                if (openApiDiagnostic.Errors.Any())
                {
                    // TODO - report issues
                    // diagnostic = Diagnostic.Create();

                    return false;
                }

                return true;
            }
            catch
            {
                // TODO - report invalid files
                // diagnostic = Diagnostic.Create();
                return false;
            }
        }

        protected IEnumerable<SourceEntry> SourceFilesFromAdditionalFiles(IEnumerable<TOptions> options) =>
            options.SelectMany(opt => SourceFilesFromAdditionalFile(opt));

        protected abstract IEnumerable<SourceEntry> SourceFilesFromAdditionalFile(TOptions options);
        protected abstract bool TryCreateOptions(AdditionalText file, OpenApiDocument document, AnalyzerConfigOptions options, GeneratorExecutionContext context, [NotNullWhen(true)] out TOptions? result);
    }
}
