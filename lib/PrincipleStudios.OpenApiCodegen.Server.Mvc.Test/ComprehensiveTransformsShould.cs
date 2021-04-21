using PrincipleStudios.OpenApi.CSharp;
using PrincipleStudios.OpenApi.Transformations;
using Snapshooter.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static PrincipleStudios.OpenApiCodegen.Server.Mvc.DocumentHelpers;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc
{
    public class ComprehensiveTransformsShould
    {
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [Theory]
        public void CoverFullFiles(int index)
        {
            var name = GetDocumentName(index);
            var document = GetDocument(index);

            var schemaTransformer = new CSharpPathControllerTransformer(document, "PS.Controller");
            var transformer = schemaTransformer.ToOpenApiSourceTransformer();
            OpenApiTransformDiagnostic diagnostic = new();

            var entries = transformer.ToSourceEntries(document, diagnostic);

            Assert.Empty(diagnostic.Errors);
            foreach (var entry in entries)
            {
                Snapshot.Match(entry.SourceText, $"{nameof(ComprehensiveTransformsShould)}.{CSharpNaming.ToTitleCaseIdentifier(name)}.{CSharpNaming.ToTitleCaseIdentifier(entry.Key.Split('.')[^2])}");
            }
        }

        [InlineData(4)]
        [Theory]
        public void ReportDiagnosticsForMissingReferences(int index)
        {
            var name = GetDocumentName(index);
            var document = GetDocument(index);

            var schemaTransformer = new CSharpPathControllerTransformer(document, "PS.Controller");
            var transformer = schemaTransformer.ToOpenApiSourceTransformer();
            OpenApiTransformDiagnostic diagnostic = new();

            var entries = transformer.ToSourceEntries(document, diagnostic).ToArray();

            Snapshot.Match(diagnostic.Errors, $"Diagnostics.{CSharpNaming.ToTitleCaseIdentifier(name)}");
        }
    }
}
