using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using PrincipleStudios.OpenApi.CSharp;
using PrincipleStudios.OpenApi.Transformations;
using static PrincipleStudios.OpenApiCodegen.Client.CSharp.OptionsHelpers;
using static PrincipleStudios.OpenApiCodegen.TestUtils.DocumentHelpers;

namespace PrincipleStudios.OpenApiCodegen.Client.CSharp
{
    internal class DynamicCompilation
    {
        public static readonly string[] SystemTextCompilationRefPaths = {
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

        public static byte[] GetGeneratedLibrary(string documentName, Action<CSharpSchemaOptions>? configureOptions = null)
        {
            var document = GetDocument(documentName);
            var options = LoadOptions();
            configureOptions?.Invoke(options);

            var transformer = document.BuildCSharpClientSourceProvider("", "PS.Controller", options);
            OpenApiTransformDiagnostic diagnostic = new();

            var entries = transformer.GetSources(diagnostic).ToArray();

            Assert.Empty(diagnostic.Errors);

            var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp9);
            var syntaxTrees = entries.Select(e => CSharpSyntaxTree.ParseText(e.SourceText, options: parseOptions, path: e.Key)).ToArray();

            string assemblyName = Path.GetRandomFileName();
            MetadataReference[] references = SystemTextCompilationRefPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();

            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName)
                .WithReferences(references)
                .AddSyntaxTrees(syntaxTrees)
                .WithOptions(new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default
                )
            );

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            Assert.All(result.Diagnostics, diagnostic =>
            {
                Assert.False(diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
            });
            Assert.True(result.Success);
            return ms.ToArray();
        }
    }
}
