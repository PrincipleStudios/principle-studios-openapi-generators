using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
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
    public sealed class OpenApiMvcServerGenerator : BaseGenerator
    {
        const string sourceGroup = "OpenApiServerInterface";

        private const string sourceItemGroupKey = "SourceItemGroup";
        private static readonly DiagnosticDescriptor IncludeDependentDll = new DiagnosticDescriptor(id: "PSAPICTRL001",
                                                                                                    title: "Include a reference to PrincipleStudios.OpenApiCodegen.Json.Extensions",
                                                                                                    messageFormat: "Include a reference to PrincipleStudios.OpenApiCodegen.Json.Extensions",
                                                                                                    category: "PrincipleStudios.OpenApiCodegen.Server.Mvc",
                                                                                                    DiagnosticSeverity.Warning,
                                                                                                    isEnabledByDefault: true);

        public OpenApiMvcServerGenerator() : base("PrincipleStudios.OpenApi.CSharp.MvcServerGenerator, PrincipleStudios.OpenApiCodegen.Server.Mvc.Base")
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
