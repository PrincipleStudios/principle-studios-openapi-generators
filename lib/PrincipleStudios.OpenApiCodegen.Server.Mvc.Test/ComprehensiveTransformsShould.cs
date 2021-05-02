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
        [MemberData(nameof(ValidFileNames))]
        [Theory]
        public void CoverFullFiles(string name)
        {
            var document = GetDocument(name);
            var options = LoadOptions();

            var schemaTransformer = new CSharpPathControllerTransformer(document, "PS.Controller", options, "");
            var transformer = schemaTransformer.ToOpenApiSourceTransformer();
            OpenApiTransformDiagnostic diagnostic = new();

            var entries = transformer.ToSourceEntries(document, diagnostic).ToArray();

            foreach (var entry in entries)
            {
                Snapshot.Match(entry.SourceText, $"{nameof(ComprehensiveTransformsShould)}.{CSharpNaming.ToTitleCaseIdentifier(name, options.ReservedIdentifiers)}.{CSharpNaming.ToTitleCaseIdentifier(entry.Key.Split('.')[^2], options.ReservedIdentifiers)}");
            }
            Assert.Empty(diagnostic.Errors);
        }

        [MemberData(nameof(InvalidFileNames))]
        [Theory]
        public void ReportDiagnosticsForMissingReferences(string name)
        {
            var document = GetDocument(name);
            var options = LoadOptions();

            var schemaTransformer = new CSharpPathControllerTransformer(document, "PS.Controller", LoadOptions(), "");
            var transformer = schemaTransformer.ToOpenApiSourceTransformer();
            OpenApiTransformDiagnostic diagnostic = new();

            var entries = transformer.ToSourceEntries(document, diagnostic).ToArray();

            Snapshot.Match(diagnostic.Errors, $"Diagnostics.{CSharpNaming.ToTitleCaseIdentifier(name, options.ReservedIdentifiers)}");
        }

        public static IEnumerable<object[]> ValidFileNames =>
            from fileIndex in GetValidDocumentIndices()
            select new object[] { GetDocumentName(fileIndex) };

        public static IEnumerable<object[]> InvalidFileNames =>
            from fileIndex in GetInvalidDocumentIndices()
            select new object[] { GetDocumentName(fileIndex) };

    }
}
