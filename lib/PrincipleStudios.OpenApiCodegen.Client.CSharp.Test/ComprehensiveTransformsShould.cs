using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CSharp;
using PrincipleStudios.OpenApi.CSharp;
using PrincipleStudios.OpenApi.Transformations;
using Snapshooter.Xunit;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static PrincipleStudios.OpenApiCodegen.Client.CSharp.OptionsHelpers;
using static PrincipleStudios.OpenApiCodegen.TestUtils.DocumentHelpers;

namespace PrincipleStudios.OpenApiCodegen.Client.CSharp
{
    public class ComprehensiveTransformsShould
    {
        [MemberData(nameof(ValidFileNames))]
        [Theory]
        public void Compile(string name)
        {
            var document = GetDocument(name);
            var options = LoadOptions();

            var transformer = document.BuildCSharpClientSourceProvider("", "PS.Controller", options);
            OpenApiTransformDiagnostic diagnostic = new();

            var entries = transformer.GetSources(diagnostic).ToArray();

            Assert.Empty(diagnostic.Errors);

            var syntaxTrees = entries.Select(e => CSharpSyntaxTree.ParseText(e.SourceText, path: e.Key)).ToArray();

            string assemblyName = Path.GetRandomFileName();
            var refPaths = new[] {
                Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location)!, "netstandard.dll"),
                Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location)!, "System.Runtime.dll"),
                typeof(System.AttributeUsageAttribute).Assembly.Location,
                typeof(System.Linq.Enumerable).Assembly.Location,
                typeof(System.ComponentModel.TypeConverter).Assembly.Location,
                typeof(System.ComponentModel.TypeConverterAttribute).Assembly.Location,
                typeof(System.ComponentModel.DataAnnotations.RequiredAttribute).Assembly.Location,
                typeof(System.Text.Json.JsonSerializer).Assembly.Location,
                typeof(System.Net.Http.Json.JsonContent).Assembly.Location,

                Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location)!, "System.Net.Http.dll"),
                Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location)!, "System.Net.Primitives.dll"),
                typeof(Uri).Assembly.Location,
                typeof(System.Web.HttpUtility).Assembly.Location,
                typeof(System.Collections.Specialized.NameValueCollection).Assembly.Location,
                typeof(PrincipleStudios.OpenApiCodegen.Json.Extensions.JsonStringEnumPropertyNameConverter).Assembly.Location,
            };
            MetadataReference[] references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: syntaxTrees,
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary) {  });


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

            var transformer = document.BuildCSharpClientSourceProvider("", "PS.Controller", options);
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

            var transformer = document.BuildCSharpClientSourceProvider("", "PS.Controller", options);
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
