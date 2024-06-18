using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using PrincipleStudios.OpenApi.Transformations;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using PrincipleStudios.OpenApi.Transformations.Specifications;
using PrincipleStudios.OpenApi.TypeScript;
using System;
using System.IO;
using System.Linq;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript
{
	class Program
	{
		static void Main(string[] args)
		{
			var commandLineApplication = new CommandLineApplication(true);
			commandLineApplication.Description = GetVersionInfo();
			commandLineApplication.HelpOption("-? | -h | --help");
			commandLineApplication.Option("-o | --options", "Path to the options file", CommandOptionType.SingleValue);
			commandLineApplication.Option("-x | --exclude-gitignore", "Do not emit gitignore file", CommandOptionType.NoValue);
			commandLineApplication.Option("-c | --clean", "Clean output path before generating", CommandOptionType.NoValue);
			commandLineApplication.Argument("input-openapi-document", "Path to the Open API document to convert");
			commandLineApplication.Argument("output-path", "Path under which to generate the TypeScript files");
			commandLineApplication.OnExecute(() =>
			{
				if (commandLineApplication.Arguments.Any(arg => arg.Value == null))
				{
					commandLineApplication.ShowHelp();
					return 1;
				}

				var inputPath = commandLineApplication.Arguments.Single(arg => arg.Name == "input-openapi-document").Value;
				var outputPath = commandLineApplication.Arguments.Single(arg => arg.Name == "output-path").Value;
				var optionsPath = commandLineApplication.Options.Find(opt => opt.LongName == "options")?.Value();
				var excludeGitignore = commandLineApplication.Options.Find(opt => opt.LongName == "exclude-gitignore")?.HasValue() ?? false;
				var clean = commandLineApplication.Options.Find(opt => opt.LongName == "clean")?.HasValue() ?? false;

				var options = LoadOptions(optionsPath);

				var (baseDocument, registry) = LoadDocument(inputPath, options);
				var parseResult = CommonParsers.DefaultParsers.Parse(baseDocument, registry);
				if (parseResult == null)
					return 3;
				if (parseResult.Diagnostics.Count > 0)
				{
					foreach (var d in parseResult.Diagnostics)
						Console.Error.WriteLine(ToDiagnosticMessage(d));
					return 4;
				}

				var openApiDocument = LoadOpenApiDocument(inputPath);
				if (openApiDocument == null)
					return 2;

				var transformer = openApiDocument.BuildTypeScriptOperationSourceProvider(GetVersionInfo(), options);

				var diagnostic = new OpenApiTransformDiagnostic();
				var entries = transformer.GetSources(diagnostic).ToArray();
				foreach (var error in diagnostic.Errors)
				{
#pragma warning disable CA2241 // CommandLineApplication does not follow standard format string format
					commandLineApplication.Error.WriteLine(
						"{subcategory}{errorCode}: {helpKeyword} {file}({lineNumber},{columnNumber}-{endLineNumber},{endColumnNumber}) {message}",
						null, "PSOPENAPI000", null, inputPath, 0, 0, 0, 0, error.Message);
#pragma warning restore CA2241
				}
				if (clean && System.IO.Directory.Exists(outputPath))
				{
					foreach (var entry in System.IO.Directory.GetFiles(outputPath))
						System.IO.File.Delete(entry);
					foreach (var entry in System.IO.Directory.GetDirectories(outputPath))
						System.IO.Directory.Delete(entry, true);
				}
				foreach (var entry in entries)
				{
					var path = System.IO.Path.Combine(outputPath, entry.Key);
					if (System.IO.Path.GetDirectoryName(path) is string dir)
						System.IO.Directory.CreateDirectory(dir);
					System.IO.File.WriteAllText(path, entry.SourceText);
				}
				if (!excludeGitignore)
				{
					var path = System.IO.Path.Combine(outputPath, ".gitignore");
					System.IO.File.WriteAllText(path, "*");
				}

				return 0;
			});
			commandLineApplication.Execute(args);
		}

		private static string ToDiagnosticMessage(DiagnosticBase d)
		{
			var position = d.Location.Range is FileLocationRange { Start: var start }
				? $"({start.Line},{start.Column})"
				: "";
			// TODO: diagnostic message line
			return $"{d.Location.RetrievalUri.LocalPath}{position}: {d.GetType().FullName}";
		}

		private static Uri ToInternalUri(string documentPath) =>
			new Uri(new Uri(documentPath).AbsoluteUri);

		private static (IDocumentReference, DocumentRegistry) LoadDocument(string documentPath, TypeScriptSchemaOptions options)
		{
			return DocumentResolverFactory.FromInitialDocumentInMemory(
				ToInternalUri(Path.Combine(Directory.GetCurrentDirectory(), documentPath)),
				File.ReadAllText(documentPath),
				ToResolverOptions(options)
			);
		}

		private static DocumentRegistryOptions ToResolverOptions(TypeScriptSchemaOptions options) =>
			new DocumentRegistryOptions([
			// TODO: use the `options` to determine how to resolve additional documents
			]);

		private static string GetVersionInfo()
		{
			return $"{typeof(Program).Namespace} v{typeof(Program).Assembly.GetName().Version}";
		}

		private static TypeScriptSchemaOptions LoadOptions(string? optionsPath)
		{
			using var defaultJsonStream = TypeScriptSchemaOptions.GetDefaultOptionsJson();
			var builder = new ConfigurationBuilder();
			builder.AddYamlStream(defaultJsonStream);
			if (optionsPath is { Length: > 0 })
				builder.AddYamlFile(Path.Combine(Directory.GetCurrentDirectory(), optionsPath));
			var result = builder.Build().Get<TypeScriptSchemaOptions>()
				?? throw new InvalidOperationException("Could not construct options");
			return result;
		}

		private static OpenApiDocument? LoadOpenApiDocument(string inputPath)
		{
			try
			{
				var openapiTextContent = System.IO.File.ReadAllText(inputPath);
				var reader = new OpenApiStringReader();
				var document = reader.Read(openapiTextContent, out var openApiDiagnostic);
				if (openApiDiagnostic.Errors.Any())
				{
					Console.WriteLine($"Errors while parsing OpenApi spec ({inputPath}):");
					foreach (var error in openApiDiagnostic.Errors)
					{
						Console.Error.WriteLine($"{inputPath}(1): error PSOPENAPI000: {error.Pointer}: {error.Message}");
					}
				}

				return document;
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
			{
				Console.Error.WriteLine($"Unable to parse OpenApi spec ({inputPath}): {ex.Message}");

				return null;
			}
		}

	}
}
