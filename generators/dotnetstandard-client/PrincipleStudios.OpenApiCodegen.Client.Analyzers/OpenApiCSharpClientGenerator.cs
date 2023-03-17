using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
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
    [Generator]
    public sealed class OpenApiCSharpClientGenerator : BaseGenerator
    {
        private const string sourceItemGroupKey = "SourceItemGroup";
        const string sourceGroup = "OpenApiClientInterface";
        private static readonly DiagnosticDescriptor IncludeDependentDll = new DiagnosticDescriptor(id: "PSAPICLNT001",
                                                                                                  title: "Include a reference to PrincipleStudios.OpenApiCodegen.Json.Extensions",
                                                                                                  messageFormat: "Include a reference to PrincipleStudios.OpenApiCodegen.Json.Extensions",
                                                                                                  category: "PrincipleStudios.OpenApiCodegen.Client",
                                                                                                  DiagnosticSeverity.Warning,
                                                                                                  isEnabledByDefault: true);

        public OpenApiCSharpClientGenerator() : base("PrincipleStudios.OpenApi.CSharp.ClientGenerator")
        {
        }

        protected override void ReportCompilationDiagnostics(Compilation compilation, CompilerApis apis)
        {
            // check that the users compilation references the expected library
            if (!compilation.ReferencedAssemblyNames.Any(static ai => ai.Name.Equals("PrincipleStudios.OpenApiCodegen.Json.Extensions", StringComparison.OrdinalIgnoreCase)))
            {
                apis.ReportDiagnostic(Diagnostic.Create(IncludeDependentDll, Location.None));
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
    }
}
