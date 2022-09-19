using FluentAssertions.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc
{

    public class SerializationShould : IClassFixture<TempDirectory>
    {
        private readonly string workingDirectory;

        public SerializationShould(TempDirectory directory)
        {
            workingDirectory = directory.DirectoryPath;
        }

        [Fact]
        public Task SerializeABasicClass() => 
            SerializeAsync(
                "petstore.yaml",
                @"new PS.Controller.NewPet(Tag: null, Name: ""Fido"")",
                new { tag = (string?)null, name = "Fido" }
            );

        [Fact]
        public Task SerializeAnAllOfClass() =>
            SerializeAsync(
                "petstore.yaml", 
                @"new PS.Controller.Pet(Id: 1007L, Tag: null, Name: ""Fido"")", 
                new { id = 1007L, tag = (string?)null, name = "Fido" }
            );

        [Fact]
        public Task SerializeAnEnum() =>
            SerializeAsync(
                "enum.yaml",
                @"PS.Controller.Option.Rock",
                "rock"
            );

        [Fact]
        public Task DeserializeABasicClass() =>
            DeserializeAsync(
                "petstore.yaml",
                new { tag = (string?)null, name = "Fido" },
                "PS.Controller.NewPet"
            );

        [Fact]
        public Task DeserializeAnAllOfClass() =>
            DeserializeAsync(
                "petstore.yaml",
                new { id = 1007L, tag = (string?)null, name = "Fido" },
                "PS.Controller.Pet"
            );

        [Fact]
        public Task DeserializeAnEnum() =>
            DeserializeAsync(
                "enum.yaml",
                "rock",
                "PS.Controller.Option"
            );

        private async Task SerializeAsync(string documentName, string csharpInitialization, object comparisonObject)
        {
            var libBytes = DynamicCompilation.GetGeneratedLibrary(documentName);

            var fullPath = Path.Combine(workingDirectory, Path.GetRandomFileName());
            File.WriteAllBytes(fullPath, libBytes);

            var scriptOptions = ScriptOptions.Default
                .AddReferences(DynamicCompilation.NewtonsoftCompilationRefPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray())
                .AddReferences(MetadataReference.CreateFromFile(fullPath));

            var result = (string)await CSharpScript.EvaluateAsync($"Newtonsoft.Json.JsonConvert.SerializeObject({csharpInitialization})", scriptOptions);

            Newtonsoft.Json.Linq.JToken.Parse(result).Should().BeEquivalentTo(
                Newtonsoft.Json.Linq.JToken.FromObject(comparisonObject)
            );
        }
        
        private async Task DeserializeAsync(string documentName, object targetObject, string typeName)
        {
            var libBytes = DynamicCompilation.GetGeneratedLibrary(documentName);

            var fullPath = Path.Combine(workingDirectory, Path.GetRandomFileName());
            File.WriteAllBytes(fullPath, libBytes);

            var scriptOptions = ScriptOptions.Default
                .AddReferences(DynamicCompilation.NewtonsoftCompilationRefPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray())
                .AddReferences(MetadataReference.CreateFromFile(fullPath));

            var original = Newtonsoft.Json.Linq.JToken.FromObject(targetObject);
            var originalJson = original.ToString(Newtonsoft.Json.Formatting.Indented).Replace("\"", "\"\"");

            var script = @$"
                Newtonsoft.Json.JsonConvert.SerializeObject(
                    Newtonsoft.Json.JsonConvert.DeserializeObject<{typeName}>(
                        @""{originalJson}""
                    )
                )";

            var result = (string)await CSharpScript.EvaluateAsync(script, scriptOptions);

            Newtonsoft.Json.Linq.JToken.Parse(result).Should().BeEquivalentTo(
                Newtonsoft.Json.Linq.JToken.FromObject(targetObject)
            );
        }
    }
}
