using HandlebarsDotNet;
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
    public class OpenApiMvcServerGenerator :
#if ROSLYN4_0_OR_GREATER
    IIncrementalGenerator
#else
    ISourceGenerator
#endif
    {
        const string sourceGroup = "OpenApiServerInterface";
        const string propNamespace = "Namespace";
        const string propConfig = "Configuration";

        private const string sourceItemGroupKey = "SourceItemGroup";
        private static readonly DiagnosticDescriptor IncludeDependentDll = new DiagnosticDescriptor(id: "PSAPICTRL001",
                                                                                                    title: "Include a reference to PrincipleStudios.OpenApiCodegen.Json.Extensions",
                                                                                                    messageFormat: "Include a reference to PrincipleStudios.OpenApiCodegen.Json.Extensions",
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

#if ROSLYN4_0_OR_GREATER
        public void Initialize(IncrementalGeneratorInitializationContext incremental)
        {
            var hasJsonExtensions = incremental.CompilationProvider.Select(static (compilation, _) => compilation.ReferencedAssemblyNames.Any(ai => ai.Name.Equals("PrincipleStudios.OpenApiCodegen.Json.Extensions", StringComparison.OrdinalIgnoreCase)));
            incremental.RegisterImplementationSourceOutput(hasJsonExtensions, static (context, hasJsonExtensions) =>
            {
                if (!hasJsonExtensions)
                    context.ReportDiagnostic(Diagnostic.Create(IncludeDependentDll, Location.None));
            });

            var additionalTexts = incremental.AdditionalTextsProvider.Combine(incremental.AnalyzerConfigOptionsProvider)
                .Select(static (tuple, _) => GetOptions(tuple.Left, tuple.Right))
                .Where(IsMvcServerFile);
            incremental.RegisterSourceOutput(additionalTexts, static (context, tuple) =>
            {
                var (file, opt) = tuple;
                GenerateSources(file, opt, context.AddSource, context.ReportDiagnostic);
            });
        }
#else
        public void Execute(GeneratorExecutionContext context)
        {
            // check that the users compilation references the expected library
            if (!context.Compilation.ReferencedAssemblyNames.Any(static ai => ai.Name.Equals("PrincipleStudios.OpenApiCodegen.Json.Extensions", StringComparison.OrdinalIgnoreCase)))
            {
                context.ReportDiagnostic(Diagnostic.Create(IncludeDependentDll, Location.None));
            }

            var additionalTexts = context.AdditionalFiles.Select(file => GetOptions(file, context.AnalyzerConfigOptions))
                .Where(IsMvcServerFile);
            foreach (var (file, opt) in additionalTexts)
            {
                GenerateSources(file, opt, context.AddSource, context.ReportDiagnostic);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
#endif

        private static void GenerateSources(AdditionalText file, AnalyzerConfigOptions opt, Action<string, SourceText> addSource, Action<Diagnostic> reportDiagnostic)
        {
            if (!TryParseFile(file, out var document, out var diagnostic))
            {
                if (diagnostic != null)
                    reportDiagnostic(diagnostic);
                return;
            }
            var sourceProvider = CreateSourceProvider(document, opt);
            var openApiDiagnostic = new OpenApiTransformDiagnostic();
            foreach (var entry in sourceProvider.GetSources(openApiDiagnostic))
            {
                addSource($"PS_{entry.Key}", SourceText.From(entry.SourceText, Encoding.UTF8));
            }
            foreach (var error in openApiDiagnostic.Errors)
            {
                // TODO - do something with these errors!
            }
        }

        private static (AdditionalText File, AnalyzerConfigOptions ConfigOptions) GetOptions(AdditionalText file, AnalyzerConfigOptionsProvider analyzerConfigOptions)
        {
            var opt = analyzerConfigOptions.GetOptions(file);
            return (file, opt);
        }

        private static bool IsMvcServerFile((AdditionalText File, AnalyzerConfigOptions ConfigOptions) tuple)
        {
            string? currentSourceGroup = tuple.ConfigOptions.GetAdditionalFilesMetadata(sourceItemGroupKey);
            if (currentSourceGroup != sourceGroup)
            {
                return false;
            }

            return true;
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

        protected static ISourceProvider CreateSourceProvider(OpenApiDocument document, AnalyzerConfigOptions opt)
        {
            var options = LoadOptions(opt.GetAdditionalFilesMetadata(propConfig));
            var documentNamespace = opt.GetAdditionalFilesMetadata(propNamespace);
            if (string.IsNullOrEmpty(documentNamespace))
                documentNamespace = GetStandardNamespace(opt, options);

            return document.BuildCSharpPathControllerSourceProvider(GetVersionInfo(), documentNamespace, options);
        }

        private static CSharpServerSchemaOptions LoadOptions(string? optionsFiles)
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
            var result = builder.Build().Get<CSharpServerSchemaOptions>();
            return result;
        }

        private static string GetVersionInfo()
        {
            return $"{typeof(CSharpControllerTransformer).FullName} v{typeof(CSharpControllerTransformer).Assembly.GetName().Version}";
        }

        private static string? GetStandardNamespace(AnalyzerConfigOptions opt, CSharpSchemaOptions options)
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
