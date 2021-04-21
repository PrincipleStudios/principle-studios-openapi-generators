using CommandLine;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using PrincipleStudios.OpenApi.CSharp;
using PrincipleStudios.OpenApi.Transformations;
using System;
using System.Linq;
using System.Threading.Tasks;

var parserResult = new CommandLine.Parser().ParseArguments<Options>(args);
return await parserResult.MapResult(async options =>
{
    var outputPath = options.OutputPath ?? System.IO.Directory.GetCurrentDirectory();
    System.IO.Directory.CreateDirectory(outputPath);
    if (options.Clean)
    {
        foreach (var file in System.IO.Directory.GetFiles(outputPath, "*.cs", new System.IO.EnumerationOptions() { RecurseSubdirectories = false, AttributesToSkip = System.IO.FileAttributes.Directory }))
        {
            System.IO.File.Delete(file);
        }
    }

    var openApiDocument = await LoadOpenApiDocument(options.InputPath);
    if (openApiDocument == null)
        return 1;

    var schemaTransformer = new CSharpPathControllerTransformer(openApiDocument, options.RootNamespace);
    var transformer = schemaTransformer.ToOpenApiSourceTransformer();

    var diagnostic = new OpenApiTransformDiagnostic();
    var entries = transformer.ToSourceEntries(openApiDocument, diagnostic).ToArray();
    foreach (var error in diagnostic.Errors)
    {
        Console.Error.WriteLine($"{options.InputPath}(1): error PSOPENAPI000: {error.Context}: {error.Message}");
    }
    foreach (var entry in entries)
    {
        await System.IO.File.WriteAllTextAsync(System.IO.Path.Combine(outputPath, entry.Key), entry.SourceText);
    }
    return 0;
}, async errs =>
{
    Console.WriteLine(CommandLine.Text.HelpText.AutoBuild(parserResult).ToString());
    await Task.Yield();
    return 1;
});

async Task<OpenApiDocument?> LoadOpenApiDocument(string inputPath)
{
    try
    {
        var openapiTextContent = await System.IO.File.ReadAllTextAsync(inputPath);
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
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Unable to parse OpenApi spec ({inputPath}): {ex.Message}");
        
        return null;
    }
}


public class Options
{
    [Option('o', "output", Required = false, HelpText = "Output folder for files.")]
    public string? OutputPath { get; set; }
    [Option('i', "input", Required = true, HelpText = "Input path for OpenAPI document.")]
    public string InputPath { get; set; } = "";
    [Option('n', "namespace", Required = true, HelpText = "Default namespace.")]
    public string RootNamespace { get; set; } = "";
    [Option('c', "clean", Required = false, HelpText = "Clean the folder before generating files.")]
    public bool Clean { get; set; }
}
