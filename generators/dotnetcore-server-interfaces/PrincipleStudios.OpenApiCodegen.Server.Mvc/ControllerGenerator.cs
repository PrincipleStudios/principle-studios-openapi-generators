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

#nullable enable

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc
{
    [Generator]
    public class ControllerGenerator : OpenApiGeneratorBase<ControllerGenerator.Options>
    {
        private static readonly DiagnosticDescriptor IncludeNewtonsoftJson = new DiagnosticDescriptor(id: "PSAPICTRL001",
                                                                                                  title: "Include a reference to Newtonsoft.Json",
                                                                                                  messageFormat: "Include a reference to Newtonsoft.Json",
                                                                                                  category: "PrincipleStudios.OpenApiCodegen.Server.Mvc",
                                                                                                  DiagnosticSeverity.Warning,
                                                                                                  isEnabledByDefault: true);
        private readonly CSharpSchemaOptions options;
        const string sourceGroup = "OpenApiServerInterface";
        const string propNamespace = "OpenApiServerInterfaceNamespace";

        public ControllerGenerator() : base(sourceGroup)
        {
            this.options = LoadOptions();
        }

        private CSharpSchemaOptions LoadOptions()
        {
            using var defaultJsonStream = CSharpSchemaOptions.GetDefaultOptionsJson();
            var builder = new ConfigurationBuilder();
            builder.AddYamlStream(defaultJsonStream);
            // TODO - allow config overrides
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

        protected override IEnumerable<SourceEntry> SourceFilesFromAdditionalFile(Options options, OpenApiTransformDiagnostic diagnostic)
        {
            var schemaTransformer = new CSharpPathControllerTransformer(options.Document, options.DocumentNamespace, this.options, GetVersionInfo());
            var transformer = schemaTransformer.ToOpenApiSourceTransformer();

            return transformer.ToSourceEntries(options.Document, diagnostic);
        }

        private static string GetVersionInfo()
        {
            return $"{typeof(CSharpPathControllerTransformer).FullName} v{typeof(CSharpPathControllerTransformer).Assembly.GetName().Version}";
        }

        protected override bool TryCreateOptions(AdditionalText file, OpenApiDocument document, AnalyzerConfigOptions opt, GeneratorExecutionContext context, [NotNullWhen(true)] out Options? result)
        {
            var documentNamespace = opt.GetAdditionalFilesMetadata(propNamespace);
            if (string.IsNullOrEmpty(documentNamespace))
                documentNamespace = GetStandardNamespace(opt);

            result = new Options(document, documentNamespace ?? "");
            return true;
        }

        private string? GetStandardNamespace(AnalyzerConfigOptions opt)
        {
            var identity = opt.GetAdditionalFilesMetadata("identity");
            var link = opt.GetAdditionalFilesMetadata("link");
            opt.TryGetValue("build_property.projectdir", out var projectDir);
            opt.TryGetValue("build_property.rootnamespace", out var rootNamespace);

            return CSharpNaming.ToNamespace(rootNamespace, projectDir, identity, link, options.ReservedIdentifiers);
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
