// This is a tiny hacky CLI for use when testing - not intended for reuse or publishing

// Parameters:
//  PrincipleStudios.OpenApiCodegen.Server.Mvc.Cli <openapi-path> <c-sharp-namespace> <output-folder>

using Microsoft.Extensions.Configuration;
using PrincipleStudios.OpenApi.CSharp;
using PrincipleStudios.OpenApi.Transformations;
using System.IO;

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

var document = GetDocument(openApiFilePath);
var options = LoadOptions();
options.PathPrefix = pathPrefix;

var transformer = document.BuildCSharpPathControllerSourceProvider("", csharpNamespace, options);
OpenApiTransformDiagnostic diagnostic = new();

var entries = transformer.GetSources(diagnostic).ToArray();

foreach (var entry in entries)
    File.WriteAllText(Path.Combine(outputFolder, entry.Key), entry.SourceText);


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
