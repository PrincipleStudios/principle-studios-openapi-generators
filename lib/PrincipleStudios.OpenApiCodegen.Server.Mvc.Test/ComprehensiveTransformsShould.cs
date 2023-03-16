using PrincipleStudios.OpenApi.CSharp;
using PrincipleStudios.OpenApi.Transformations;
using System.Linq;
using Xunit;
using static PrincipleStudios.OpenApiCodegen.Server.Mvc.OptionsHelpers;
using static PrincipleStudios.OpenApiCodegen.TestUtils.DocumentHelpers;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc
{
    public class ComprehensiveTransformsShould
    {
        /// <summary>
        /// These tests should match the same set of yaml that is in the TestApp. If the TestApp
        /// builds, these should, too. However, this contributes to code coverage.
        /// </summary>
        [Trait("Category", "RepeatMsBuild")]
        [InlineData("all-of.yaml")]
        [InlineData("enum.yaml")]
        [InlineData("controller-extension.yaml")]
        [InlineData("regex-escape.yaml")]
        [InlineData("validation-min-max.yaml")]
        [InlineData("headers.yaml")]
        [InlineData("oauth.yaml")]
        [InlineData("form.yaml")]
        [InlineData("one-of.yaml")]
        [InlineData("nullable-vs-optional.yaml")]
        [InlineData("nullable-vs-optional-legacy.yaml")]
        [Theory]
        public void Compile_api_documents_included_in_the_TestApp(string name)
        {
            DynamicCompilation.GetGeneratedLibrary(name);
        }

        private static OpenApiTransformDiagnostic GetDocumentDiagnostics(string name)
        {
            var document = GetDocument(name);
            var options = LoadOptions();

            var transformer = document.BuildCSharpPathControllerSourceProvider("", "PS.Controller", options);
            OpenApiTransformDiagnostic diagnostic = new();

            transformer.GetSources(diagnostic).ToArray(); // force all sources to load to get diagnostics
            return diagnostic;
        }

        [Fact]
        public void ReportDiagnosticsForMissingReferences()
        {
            OpenApiTransformDiagnostic diagnostic = GetDocumentDiagnostics("bad.yaml");

            Assert.Collection(diagnostic.Errors, new[]
            {
                (OpenApiTransformError error) => Assert.Contains("Unresolved external reference", error.Message)
            });
        }

    }
}
