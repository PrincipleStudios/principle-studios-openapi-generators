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

#if NETSTANDARD2_0
#nullable disable warnings
#endif

namespace PrincipleStudios.OpenApiCodegen.Client
{
    public abstract class OpenApiGeneratorBase : ISourceGenerator
    {
        private const string sourceItemGroupKey = "SourceItemGroup";
        private static readonly DiagnosticDescriptor NoFilesGenerated = new DiagnosticDescriptor(id: "PSAPIGEN001",
                                                                                          title: "No files found enabled",
                                                                                          messageFormat: "No additional files were found with SourceItemGroup '{0}'",
                                                                                          category: "PrincipleStudios.OpenApiCodegen",
                                                                                          DiagnosticSeverity.Warning,
                                                                                          isEnabledByDefault: true);
        protected static readonly DiagnosticDescriptor FileGenerated = new DiagnosticDescriptor(id: "PSAPIGEN002",
                                                                                          title: "File generated",
                                                                                          messageFormat: "Generated file '{0}'",
                                                                                          category: "PrincipleStudios.OpenApiCodegen",
                                                                                          DiagnosticSeverity.Info,
                                                                                          isEnabledByDefault: true);
        protected static readonly DiagnosticDescriptor NoSourceGroup = new DiagnosticDescriptor(id: "PSAPIGEN003",
                                                                                          title: "No source group",
                                                                                          messageFormat: "No source group for '{0}'",
                                                                                          category: "PrincipleStudios.OpenApiCodegen",
                                                                                          DiagnosticSeverity.Info,
                                                                                          isEnabledByDefault: true);

        private readonly string sourceGroup;

        public OpenApiGeneratorBase(string sourceGroup)
        {
            this.sourceGroup = sourceGroup;
        }

        public virtual void Execute(GeneratorExecutionContext context)
        {
            var options = GetLoadOptions(context).ToArray();
            if (!options.Any())
                context.ReportDiagnostic(Diagnostic.Create(NoFilesGenerated, Location.None, sourceGroup));
            var diagnostic = new OpenApiTransformDiagnostic();
            var nameCodeSequence = SourceFilesFromAdditionalFiles(options, diagnostic).ToArray();
            foreach (var entry in nameCodeSequence)
            {
                context.AddSource($"PS_{entry.Key}", SourceText.From(entry.SourceText, Encoding.UTF8));
                context.ReportDiagnostic(Diagnostic.Create(FileGenerated, Location.None, $"PS_{entry.Key}"));
            }
        }


        public virtual void Initialize(GeneratorInitializationContext context)
        {
        }

        private IEnumerable<ISourceProvider> GetLoadOptions(GeneratorExecutionContext context)
        {
            foreach (AdditionalText file in context.AdditionalFiles)
            {
                var opt = context.AnalyzerConfigOptions.GetOptions(file);

                string? currentSourceGroup = opt.GetAdditionalFilesMetadata(sourceItemGroupKey);
                if (currentSourceGroup != sourceGroup)
                {
                    if (string.IsNullOrEmpty(currentSourceGroup))
                        context.ReportDiagnostic(Diagnostic.Create(NoSourceGroup, Location.None, file.Path));
                    continue;
                }

                if (TryParseFile(file, out var document, out var diagnostic))
                {
                    if (TryCreateSourceProvider(file, document!, opt, context, out var sourceProvider))
                        yield return sourceProvider;
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

        private IEnumerable<SourceEntry> SourceFilesFromAdditionalFiles(IEnumerable<ISourceProvider> options, OpenApiTransformDiagnostic diagnostic) =>
            options.SelectMany(opt => SourceFilesFromAdditionalFile(opt, diagnostic));

        protected virtual IEnumerable<SourceEntry> SourceFilesFromAdditionalFile(ISourceProvider options, OpenApiTransformDiagnostic diagnostic) =>
            options.GetSources(diagnostic);
        protected abstract bool TryCreateSourceProvider(AdditionalText file, OpenApiDocument document, AnalyzerConfigOptions options, GeneratorExecutionContext context, [NotNullWhen(true)] out ISourceProvider? result);
    }
}
