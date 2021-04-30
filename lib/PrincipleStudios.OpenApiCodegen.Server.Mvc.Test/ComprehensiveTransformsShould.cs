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
        [MemberData(nameof(ValidFileIndices))]
        [Theory]
        public void CoverFullFiles(int index)
        {
            var name = GetDocumentName(index);
            var document = GetDocument(index);
            var options = LoadOptions();

            var schemaTransformer = new CSharpPathControllerTransformer(document, "PS.Controller", options);
            var transformer = schemaTransformer.ToOpenApiSourceTransformer();
            OpenApiTransformDiagnostic diagnostic = new();

            var entries = transformer.ToSourceEntries(document, diagnostic).ToArray();

            foreach (var entry in entries)
            {
                Snapshot.Match(entry.SourceText, $"{nameof(ComprehensiveTransformsShould)}.{CSharpNaming.ToTitleCaseIdentifier(name, options.ReservedIdentifiers)}.{CSharpNaming.ToTitleCaseIdentifier(entry.Key.Split('.')[^2], options.ReservedIdentifiers)}");
            }
            Assert.Empty(diagnostic.Errors);
        }

        [MemberData(nameof(InvalidFileIndices))]
        [Theory]
        public void ReportDiagnosticsForMissingReferences(int index)
        {
            var name = GetDocumentName(index);
            var document = GetDocument(index);
            var options = LoadOptions();

            var schemaTransformer = new CSharpPathControllerTransformer(document, "PS.Controller", LoadOptions());
            var transformer = schemaTransformer.ToOpenApiSourceTransformer();
            OpenApiTransformDiagnostic diagnostic = new();

            var entries = transformer.ToSourceEntries(document, diagnostic).ToArray();

            Snapshot.Match(diagnostic.Errors, $"Diagnostics.{CSharpNaming.ToTitleCaseIdentifier(name, options.ReservedIdentifiers)}");
        }

        public static IEnumerable<object[]> ValidFileIndices =>
            from fileIndex in GetValidDocumentIndices()
            select new object[] { fileIndex };

        public static IEnumerable<object[]> InvalidFileIndices =>
            from fileIndex in GetInvalidDocumentIndices()
            select new object[] { fileIndex };

    }
}
