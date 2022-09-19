using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using PrincipleStudios.OpenApi.CSharp;
using PrincipleStudios.OpenApi.Transformations;
using Snapshooter.Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static PrincipleStudios.OpenApiCodegen.Server.Mvc.OptionsHelpers;
using static PrincipleStudios.OpenApiCodegen.TestUtils.DocumentHelpers;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc
{
    public class ComprehensiveTransformsShould
    {
        [MemberData(nameof(ValidFileNames))]
        [Theory]
        public void Compile(string name)
        {
            var document = GetDocument(name);
            var options = LoadOptions();

            var transformer = document.BuildCSharpPathControllerSourceProvider("", "PS.Controller", options);
            OpenApiTransformDiagnostic diagnostic = new();

            var entries = transformer.GetSources(diagnostic).ToArray();

            Assert.Empty(diagnostic.Errors);

            var syntaxTrees = entries.Select(e => CSharpSyntaxTree.ParseText(e.SourceText, path: e.Key)).ToArray();

            string assemblyName = Path.GetRandomFileName();
            MetadataReference[] references = DynamicCompilation.NewtonsoftCompilationRefPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: syntaxTrees,
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary) { });

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            Assert.All(result.Diagnostics, diagnostic =>
            {
                Assert.False(diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
            });
            Assert.True(result.Success);
        }

        [MemberData(nameof(ValidFileNames))]
        [Theory]
        public void CoverFullFiles(string name)
        {
            var document = GetDocument(name);
            var options = LoadOptions();

            var transformer = document.BuildCSharpPathControllerSourceProvider("", "PS.Controller", options);
            OpenApiTransformDiagnostic diagnostic = new();

            var entries = transformer.GetSources(diagnostic).ToArray();

            Assert.All(entries, entry =>
            {
                Snapshot.Match(entry.SourceText, $"{nameof(ComprehensiveTransformsShould)}.{CSharpNaming.ToTitleCaseIdentifier(name, options.ReservedIdentifiers())}.{CSharpNaming.ToTitleCaseIdentifier(entry.Key.Split('.')[^2], options.ReservedIdentifiers())}");
            });
            Assert.Empty(diagnostic.Errors);
        }

        [MemberData(nameof(InvalidFileNames))]
        [Theory]
        public void ReportDiagnosticsForMissingReferences(string name)
        {
            var document = GetDocument(name);
            var options = LoadOptions();

            var transformer = document.BuildCSharpPathControllerSourceProvider("", "PS.Controller", options);
            OpenApiTransformDiagnostic diagnostic = new();

            var entries = transformer.GetSources(diagnostic).ToArray();

            Snapshot.Match(diagnostic.Errors.Select(err => new { Context = err.Context.ToOpenApiPathContextString(), Message = err.Message }).ToArray(), $"Diagnostics.{CSharpNaming.ToTitleCaseIdentifier(name, options.ReservedIdentifiers())}");
        }

        public static IEnumerable<object[]> ValidFileNames =>
            from fileIndex in GetValidDocumentIndices()
            select new object[] { GetDocumentName(fileIndex) };

        public static IEnumerable<object[]> InvalidFileNames =>
            from fileIndex in GetInvalidDocumentIndices()
            select new object[] { GetDocumentName(fileIndex) };

    }
}
