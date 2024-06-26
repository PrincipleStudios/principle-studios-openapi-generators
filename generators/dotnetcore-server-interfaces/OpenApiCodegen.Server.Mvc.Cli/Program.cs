// This is a tiny hacky CLI for use when testing - not intended for reuse or publishing

// Parameters:
//  PrincipleStudios.OpenApiCodegen.Server.Mvc.Cli <openapi-path> <c-sharp-namespace> <output-folder>

using Microsoft.Extensions.Configuration;
using PrincipleStudios.OpenApi.CSharp;
using PrincipleStudios.OpenApi.Transformations;
using PrincipleStudios.OpenApi.Transformations.Specifications;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
using PrincipleStudios.OpenApiCodegen;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;

var openApiFilePath = args[0];
var csharpNamespace = args[1];
var outputFolder = args[2];
var pathPrefix = args.Length > 3 ? args[3] : "";

Console.WriteLine($"Generating with the following settings:");
Console.WriteLine($"  OpenAPI: {openApiFilePath}");
Console.WriteLine($"  C# Namespace: {csharpNamespace}");
Console.WriteLine($"  Output folder: {outputFolder}");
Console.WriteLine($"  Path prefix: {pathPrefix}");

// empty the directory and then recreate it
if (Directory.Exists(outputFolder))
	Directory.Delete(outputFolder, recursive: true);
Directory.CreateDirectory(outputFolder);

var options = LoadOptions();
var (baseDocument, registry) = LoadDocument(openApiFilePath, File.ReadAllText(openApiFilePath), options);
var parseResult = CommonParsers.DefaultParsers.Parse(baseDocument, registry);
var parsedDiagnostics = parseResult.Diagnostics.Select(DiagnosticsConversion.ToDiagnosticInfo).ToArray();
if (!parseResult.HasDocument)
{
	LogDiagnostics(parsedDiagnostics);
	return 1;
}

var document = GetDocument(openApiFilePath);
options.PathPrefix = pathPrefix;

var transformer = document.BuildCSharpPathControllerSourceProvider("", csharpNamespace, options);
OpenApiTransformDiagnostic diagnostic = new();

var entries = transformer.GetSources(diagnostic).ToArray();

foreach (var entry in entries)
	File.WriteAllText(Path.Combine(outputFolder, entry.Key), entry.SourceText);
return 0;

Microsoft.OpenApi.Models.OpenApiDocument GetDocument(string path)
{
	using var documentStream = File.OpenRead(path);
	var reader = new Microsoft.OpenApi.Readers.OpenApiStreamReader();
	return reader.Read(documentStream, out var openApiDiagnostic);
}
CSharpServerSchemaOptions LoadOptions(Action<IConfigurationBuilder>? configureBuilder = null)
{
	using var defaultJsonStream = CSharpSchemaOptions.GetDefaultOptionsJson();
	var builder = new ConfigurationBuilder();
	builder.AddYamlStream(defaultJsonStream);
	configureBuilder?.Invoke(builder);
	var result = builder.Build().Get<CSharpServerSchemaOptions>()
		?? throw new InvalidOperationException("Could not construct options");
	return result;
}

static Uri ToInternalUri(string documentPath) =>
		new Uri(new Uri(documentPath).AbsoluteUri);
static (IDocumentReference, DocumentRegistry) LoadDocument(string documentPath, string documentContents, CSharpServerSchemaOptions options)
{
	return DocumentResolverFactory.FromInitialDocumentInMemory(
		ToInternalUri(documentPath),
		documentContents,
		ToResolverOptions(options)
	);
}

static DocumentRegistryOptions ToResolverOptions(CSharpServerSchemaOptions options) =>
	new DocumentRegistryOptions([
	// TODO: use the `options` to determine how to resolve additional documents
	]);

void LogDiagnostics(DiagnosticInfo[] parsedDiagnostics)
{
	foreach (var d in parsedDiagnostics)
	{
		var position = d.Location.Range is not { Start: var s } ? "" : $":{s.Line}:{s.Column}";
		Console.Error.WriteLine($"{d.Location.FilePath}{position}: {d.Id} {string.Join(" ", d.Metadata)}");
	}
}