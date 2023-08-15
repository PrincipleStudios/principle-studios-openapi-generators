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
				@"new PS.Controller.NewPet(Tag: PrincipleStudios.OpenApiCodegen.Json.Extensions.Optional.Create(""dog""), Name: ""Fido"")",
				new { tag = "dog", name = "Fido" }
			);

		[Fact]
		public Task SerializeABasicClassWithOptionalValueOmitted() =>
			SerializeAsync(
				"petstore.yaml",
				@"new PS.Controller.NewPet(Tag: PrincipleStudios.OpenApiCodegen.Json.Extensions.Optional<string>.None, Name: ""Fido"")",
				new { name = "Fido" }
			);

		[Fact]
		public Task SerializeAnAllOfClass() =>
			SerializeAsync(
				"petstore.yaml",
				@"new PS.Controller.Pet(Id: 1007L, Tag: PrincipleStudios.OpenApiCodegen.Json.Extensions.Optional.Create(""dog""), Name: ""Fido"")",
				new { id = 1007L, tag = "dog", name = "Fido" }
			);

		[Fact]
		public Task SerializeAnAllOfClassWithOptionalValueOmitted() =>
			SerializeAsync(
				"petstore.yaml",
				@"new PS.Controller.Pet(Id: 1007L, Tag: PrincipleStudios.OpenApiCodegen.Json.Extensions.Optional<string>.None, Name: ""Fido"")",
				new { id = 1007L, name = "Fido" }
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

		[Theory]
		[InlineData("new PS.Controller.Pet(Dog: new PS.Controller.Dog(Bark: true, Breed: \"Shiba Inu\"))", "{ \"bark\": true, \"breed\": \"Shiba Inu\" }")]
		[InlineData("new PS.Controller.Pet(Cat: new PS.Controller.Cat(Hunts: false, Age: 12))", "{ \"hunts\": false, \"age\": 12 }")]
		[InlineData("new PS.Controller.SpecifiedPet(Dog: new PS.Controller.Dog(Bark: true, Breed: \"Shiba Inu\"))", "{ \"petType\": \"dog\", \"bark\": true, \"breed\": \"Shiba Inu\" }")]
		[InlineData("new PS.Controller.SpecifiedPet(Cat: new PS.Controller.Cat(Hunts: false, Age: 12))", "{ \"petType\": \"cat\", \"hunts\": false, \"age\": 12 }")]
		public Task SerializeAOneOfObject(string csharpScript, string json) =>
			SerializeAsync(
				"one-of.yaml",
				csharpScript,
				Newtonsoft.Json.Linq.JToken.Parse(json)
			);

		[Theory]
		[InlineData("PS.Controller.Pet", "{ \"bark\": true, \"breed\": \"Shiba Inu\" }")]
		[InlineData("PS.Controller.Pet", "{ \"hunts\": false, \"age\": 12 }")]
		[InlineData("PS.Controller.SpecifiedPet", "{ \"petType\": \"dog\", \"bark\": true, \"breed\": \"Shiba Inu\" }")]
		[InlineData("PS.Controller.SpecifiedPet", "{ \"petType\": \"cat\", \"hunts\": false, \"age\": 12 }")]
		public Task DeserializeAOneOfObject(string csharpType, string json) =>
			DeserializeAsync(
				"one-of.yaml",
				Newtonsoft.Json.Linq.JToken.Parse(json),
				csharpType
			);

		private async Task SerializeAsync(string documentName, string csharpInitialization, object comparisonObject)
		{
			var libBytes = DynamicCompilation.GetGeneratedLibrary(documentName);

			var fullPath = Path.Combine(workingDirectory, Path.GetRandomFileName());
			File.WriteAllBytes(fullPath, libBytes);

			var scriptOptions = ScriptOptions.Default
				.AddReferences(DynamicCompilation.SystemTextCompilationRefPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray())
				.AddReferences(MetadataReference.CreateFromFile(fullPath));

			var result = (string)await CSharpScript.EvaluateAsync($"System.Text.Json.JsonSerializer.Serialize({csharpInitialization})", scriptOptions);

			Newtonsoft.Json.Linq.JToken.Parse(result).Should().BeEquivalentTo(
				comparisonObject is Newtonsoft.Json.Linq.JToken token
					? token
					: Newtonsoft.Json.Linq.JToken.FromObject(comparisonObject)
			);
		}

		private async Task DeserializeAsync(string documentName, object targetObject, string typeName)
		{
			var libBytes = DynamicCompilation.GetGeneratedLibrary(documentName);

			var fullPath = Path.Combine(workingDirectory, Path.GetRandomFileName());
			File.WriteAllBytes(fullPath, libBytes);

			var scriptOptions = ScriptOptions.Default
				.AddReferences(DynamicCompilation.SystemTextCompilationRefPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray())
				.AddReferences(MetadataReference.CreateFromFile(fullPath));

			var original = Newtonsoft.Json.Linq.JToken.FromObject(targetObject);
			var originalJson = original.ToString(Newtonsoft.Json.Formatting.Indented).Replace("\"", "\"\"");

			var script = @$"
                System.Text.Json.JsonSerializer.Serialize(
                    System.Text.Json.JsonSerializer.Deserialize<{typeName}>(
                        @""{originalJson}""
                    )
                )";

			var result = (string)await CSharpScript.EvaluateAsync(script, scriptOptions);

			Newtonsoft.Json.Linq.JToken.Parse(result).Should().BeEquivalentTo(
				targetObject is Newtonsoft.Json.Linq.JToken token
					? token
					: Newtonsoft.Json.Linq.JToken.FromObject(targetObject)
			);
		}
	}
}
