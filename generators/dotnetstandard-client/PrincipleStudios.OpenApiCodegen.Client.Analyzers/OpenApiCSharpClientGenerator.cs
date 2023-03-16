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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

#if NETSTANDARD2_0
#nullable disable warnings
#endif

namespace PrincipleStudios.OpenApiCodegen.Client
{
    [Obsolete("TODO: use IOpenApiCodeGenerator constructor")]
    [Generator]
    public sealed class OpenApiCSharpClientGenerator : BaseGenerator
    {
        private const string sourceItemGroupKey = "SourceItemGroup";
        const string sourceGroup = "OpenApiClientInterface";
        const string propNamespace = "Namespace";
        const string propConfig = "Configuration";
        private static readonly DiagnosticDescriptor IncludeDependentDll = new DiagnosticDescriptor(id: "PSAPICLNT001",
                                                                                                  title: "Include a reference to PrincipleStudios.OpenApiCodegen.Json.Extensions",
                                                                                                  messageFormat: "Include a reference to PrincipleStudios.OpenApiCodegen.Json.Extensions",
                                                                                                  category: "PrincipleStudios.OpenApiCodegen.Client",
                                                                                                  DiagnosticSeverity.Warning,
                                                                                                  isEnabledByDefault: true);

        protected override void ReportCompilationDiagnostics(Compilation compilation, CompilerApis apis)
        {
            // check that the users compilation references the expected library
            if (!compilation.ReferencedAssemblyNames.Any(static ai => ai.Name.Equals("PrincipleStudios.OpenApiCodegen.Json.Extensions", StringComparison.OrdinalIgnoreCase)))
            {
                apis.ReportDiagnostic(Diagnostic.Create(IncludeDependentDll, Location.None));
            }
        }

        protected override void GenerateSources(AdditionalTextWithOptions additionalText, CompilerApis apis)
        {
            if (!TryParseFile(additionalText.TextContents, out var document, out var diagnostic))
            {
                if (diagnostic != null)
                    apis.ReportDiagnostic(diagnostic);
                return;
            }
            var sourceProvider = CreateSourceProvider(document, additionalText.ConfigOptions);
            var openApiDiagnostic = new OpenApiTransformDiagnostic();
            foreach (var entry in sourceProvider.GetSources(openApiDiagnostic))
            {
                apis.AddSource($"PS_{entry.Key}", SourceText.From(entry.SourceText, Encoding.UTF8));
            }
            foreach (var error in openApiDiagnostic.Errors)
            {
                // TODO - do something with these errors!
            }
        }

        protected override bool IsRelevantFile(AdditionalTextWithOptions additionalText)
        {
            string? currentSourceGroup = additionalText.ConfigOptions.GetAdditionalFilesMetadata(sourceItemGroupKey);
            if (currentSourceGroup != sourceGroup)
            {
                return false;
            }

            return true;
        }

        private static bool TryParseFile(string openapiTextContent, [NotNullWhen(true)] out OpenApiDocument? document, out Diagnostic? diagnostic)
        {
            document = null;
            diagnostic = null;
            try
            {
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

        private static ISourceProvider CreateSourceProvider(OpenApiDocument document, AnalyzerConfigOptions opt)
        {
            var options = LoadOptions(opt.GetAdditionalFilesMetadata(propConfig));
            var documentNamespace = opt.GetAdditionalFilesMetadata(propNamespace);
            if (string.IsNullOrEmpty(documentNamespace))
                documentNamespace = GetStandardNamespace(opt, options);

            return document.BuildCSharpClientSourceProvider(GetVersionInfo(), documentNamespace, options);
        }

        private static CSharpSchemaOptions LoadOptions(string? optionsFiles)
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

        private static string GetVersionInfo()
        {
            return $"{typeof(CSharpClientTransformer).FullName} v{typeof(CSharpClientTransformer).Assembly.GetName().Version}";
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
}
