using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using PrincipleStudios.OpenApi.CSharp;
using PrincipleStudios.OpenApi.Transformations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

#if NETSTANDARD2_0
#nullable disable warnings
#endif

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc
{
    [Generator]
    public class OpenApiMvcServerGeneratorBase : ISourceGenerator
    {
        const string sourceGroup = "OpenApiServerInterface";
        const string propNamespace = "Namespace";
        const string propConfig = "Configuration";

        private const string sourceItemGroupKey = "SourceItemGroup";
        private static readonly DiagnosticDescriptor IncludeNewtonsoftJson = new DiagnosticDescriptor(id: "PSAPICTRL001",
                                                                                                  title: "Include a reference to Newtonsoft.Json",
                                                                                                  messageFormat: "Include a reference to Newtonsoft.Json",
                                                                                                  category: "PrincipleStudios.OpenApiCodegen.Server.Mvc",
                                                                                                  DiagnosticSeverity.Warning,
                                                                                                  isEnabledByDefault: true);
        private static readonly DiagnosticDescriptor GeneratedNamespace = new DiagnosticDescriptor(id: "PSAPICTRLINFO001",
                                                                                                  title: "Generated Namespace",
                                                                                                  messageFormat: "Generated Namespace: {0}",
                                                                                                  category: "PrincipleStudios.OpenApiCodegen.Server.Mvc",
                                                                                                  DiagnosticSeverity.Info,
                                                                                                  isEnabledByDefault: true);
        private static readonly DiagnosticDescriptor NoFilesGenerated = new DiagnosticDescriptor(id: "PSAPICTRL002",
                                                                                          title: "No files found enabled",
                                                                                          messageFormat: "No files were found; ensure you have added an item for 'OpenApiSchemaMvcServer'",
                                                                                          category: "PrincipleStudios.OpenApiCodegen",
                                                                                          DiagnosticSeverity.Warning,
                                                                                          isEnabledByDefault: true);
        protected static readonly DiagnosticDescriptor FileGenerated = new DiagnosticDescriptor(id: "PSAPICTRL003",
                                                                                          title: "File generated",
                                                                                          messageFormat: "Generated file '{0}'",
                                                                                          category: "PrincipleStudios.OpenApiCodegen",
                                                                                          DiagnosticSeverity.Info,
                                                                                          isEnabledByDefault: true);
        protected static readonly DiagnosticDescriptor NoSourceGroup = new DiagnosticDescriptor(id: "PSAPICTRL004",
                                                                                          title: "No source group",
                                                                                          messageFormat: "No source group for '{0}'",
                                                                                          category: "PrincipleStudios.OpenApiCodegen",
                                                                                          DiagnosticSeverity.Info,
                                                                                          isEnabledByDefault: true);

        public virtual void Execute(GeneratorExecutionContext context)
        {
            // check that the users compilation references the expected library 
            if (!context.Compilation.ReferencedAssemblyNames.Any(ai => ai.Name.Equals("Newtonsoft.Json", StringComparison.OrdinalIgnoreCase)))
            {
                context.ReportDiagnostic(Diagnostic.Create(IncludeNewtonsoftJson, Location.None));
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
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
            stopwatch.Stop();
            context.AddSource($"PS_timing.txt", SourceText.From($"Complete: {stopwatch.Elapsed}", Encoding.UTF8));
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

        protected bool TryCreateSourceProvider(AdditionalText file, OpenApiDocument document, AnalyzerConfigOptions opt, GeneratorExecutionContext context, [NotNullWhen(true)] out ISourceProvider? result)
        {
            var options = LoadOptions(opt.GetAdditionalFilesMetadata(propConfig));
            var documentNamespace = opt.GetAdditionalFilesMetadata(propNamespace);
            if (string.IsNullOrEmpty(documentNamespace))
                documentNamespace = GetStandardNamespace(opt, options);

            context.ReportDiagnostic(Diagnostic.Create(GeneratedNamespace, Location.None, documentNamespace));

            result = document.BuildCSharpPathControllerSourceProvider(GetVersionInfo(), documentNamespace, options);

            return true;
        }

        private CSharpSchemaOptions LoadOptions(string? optionsFiles)
        {
            using var defaultJsonStream = CSharpSchemaOptions.GetDefaultOptionsJson();
            var builder = new ConfigurationBuilder();
            builder.AddYamlStream(defaultJsonStream);
            if (optionsFiles is { Length: > 0 })
            {
                foreach (var file in optionsFiles.Split(';'))
                {
                    if (System.IO.File.Exists(file))
                    {
                        builder.AddYamlFile(file);
                    }
                }
            }
            var result = builder.Build().Get<CSharpSchemaOptions>();
            return result;
        }


        public record Options(OpenApiDocument Document, string DocumentNamespace);

        private static string GetVersionInfo()
        {
            return $"{typeof(CSharpControllerTransformer).FullName} v{typeof(CSharpControllerTransformer).Assembly.GetName().Version}";
        }

        private string? GetStandardNamespace(AnalyzerConfigOptions opt, CSharpSchemaOptions options)
        {
            var identity = opt.GetAdditionalFilesMetadata("identity");
            var link = opt.GetAdditionalFilesMetadata("link");
            opt.TryGetValue("build_property.projectdir", out var projectDir);
            opt.TryGetValue("build_property.rootnamespace", out var rootNamespace);

            return CSharpNaming.ToNamespace(rootNamespace, projectDir, identity, link, options.ReservedIdentifiers());
        }
    }

#if NETSTANDARD2_0
    [System.AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    sealed class NotNullWhenAttribute : Attribute
    {
        // This is a positional argument
        public NotNullWhenAttribute(bool result)
        {
        }
    }
#endif
}
