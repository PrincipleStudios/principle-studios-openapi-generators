using PrincipleStudios.OpenApi.Transformations;
using PrincipleStudios.OpenApi.TypeScript;
using Snapshooter.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static PrincipleStudios.OpenApiCodegen.TestUtils.DocumentHelpers;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript
{
    using static OptionsHelpers;
    public class ComprehensiveTransformsShould
    {
        [Trait("Category", "Integration")]
        [MemberData(nameof(ValidFileNames))]
        [Theory]
        public void CoverFullFiles(string name)
        {
            var document = GetDocument(name);
            var options = LoadOptions();

            var transformer = document.BuildTypeScriptOperationSourceProvider("", options);
            OpenApiTransformDiagnostic diagnostic = new();

            var entries = transformer.GetSources(diagnostic).ToArray();

            var sb = new StringBuilder();
            foreach (var entry in entries)
            {
                sb.AppendLine($"exports[`{entry.Key}`] = `{entry.SourceText}`;");
                sb.AppendLine();
            }
            Snapshot.Match(sb.ToString(), $"{nameof(ComprehensiveTransformsShould)}.{TypeScriptNaming.ToTitleCaseIdentifier(name, options.ReservedIdentifiers())}");
            Assert.Empty(diagnostic.Errors);
        }

        [Trait("Category", "Integration")]
        [MemberData(nameof(InvalidFileNames))]
        [Theory]
        public void ReportDiagnosticsForMissingReferences(string name)
        {
            var document = GetDocument(name);
            var options = LoadOptions();

            var transformer = document.BuildTypeScriptOperationSourceProvider("", options);
            OpenApiTransformDiagnostic diagnostic = new();

            var entries = transformer.GetSources(diagnostic).ToArray();

            Snapshot.Match(diagnostic.Errors.Select(err => new { Context = err.Context.ToOpenApiPathContextString(), Message = err.Message }).ToArray(), $"Diagnostics.{TypeScriptNaming.ToTitleCaseIdentifier(name, options.ReservedIdentifiers())}");
        }

        public static IEnumerable<object[]> ValidFileNames =>
            from fileIndex in GetValidDocumentIndices()
            select new object[] { GetDocumentName(fileIndex) };

        public static IEnumerable<object[]> InvalidFileNames =>
            from fileIndex in GetInvalidDocumentIndices()
            select new object[] { GetDocumentName(fileIndex) };

    }
}
