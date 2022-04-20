using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.CSharp;
using PrincipleStudios.OpenApi.Transformations;

namespace PrincipleStudios.OpenApiCodegen.Client
{
    [Generator]
    public class ClientGenerator : OpenApiGeneratorBase
    {
        private static readonly DiagnosticDescriptor IncludeNewtonsoftJson = new DiagnosticDescriptor(id: "PSAPICLNT001",
                                                                                                  title: "Include a reference to Newtonsoft.Json",
                                                                                                  messageFormat: "Include a reference to Newtonsoft.Json",
                                                                                                  category: "PrincipleStudios.OpenApiCodegen.Client",
                                                                                                  DiagnosticSeverity.Warning,
                                                                                                  isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor GeneratedNamespace = new DiagnosticDescriptor(id: "PSAPICLNTINFO001",
                                                                                                  title: "Generated Namespace",
                                                                                                  messageFormat: "Generated Namespace: {0}",
                                                                                                  category: "PrincipleStudios.OpenApiCodegen.Client",
                                                                                                  DiagnosticSeverity.Info,
                                                                                                  isEnabledByDefault: true);
        const string sourceGroup = "OpenApiClientInterface";
        const string propNamespace = "Namespace";
        const string propConfig = "Configuration";

        public ClientGenerator() : base(sourceGroup)
        {
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


        public record Options(OpenApiDocument Document, string DocumentNamespace) : OpenApiGeneratorOptions(Document);

        public override void Execute(GeneratorExecutionContext context)
        {
            // check that the users compilation references the expected library 
            if (!context.Compilation.ReferencedAssemblyNames.Any(ai => ai.Name.Equals("Newtonsoft.Json", StringComparison.OrdinalIgnoreCase)))
            {
                context.ReportDiagnostic(Diagnostic.Create(IncludeNewtonsoftJson, Location.None));
            }
            base.Execute(context);
        }

        private static string GetVersionInfo()
        {   
            return $"{typeof(CSharpClientTransformer).FullName} v{typeof(CSharpClientTransformer).Assembly.GetName().Version}";
        }

        protected override bool TryCreateSourceProvider(AdditionalText file, OpenApiDocument document, AnalyzerConfigOptions opt, GeneratorExecutionContext context, [NotNullWhen(true)] out ISourceProvider? result)
        {
            var options = LoadOptions(opt.GetAdditionalFilesMetadata(propConfig));
            var documentNamespace = opt.GetAdditionalFilesMetadata(propNamespace);
            if (string.IsNullOrEmpty(documentNamespace))
                documentNamespace = GetStandardNamespace(opt, options);

            context.ReportDiagnostic(Diagnostic.Create(GeneratedNamespace, Location.None, documentNamespace));

            result = document.BuildCSharpClientSourceProvider(GetVersionInfo(), documentNamespace, options);

            return true;
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
