using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers.Interface;
using Microsoft.OpenApi.Readers;
using PrincipleStudios.OpenApi.Transformations;
using PrincipleStudios.OpenApiCodegen;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Linq;

namespace PrincipleStudios.OpenApi.CSharp;

public class ClientGenerator : IOpenApiCodeGenerator
{
    const string propNamespace = "Namespace";
    const string propConfig = "Configuration";
    const string propIdentity = "identity";
    const string propLink = "link";
    private readonly IEnumerable<string> metadataKeys = new[]
    {
        propNamespace,
        propConfig,
        propIdentity,
        propLink,
    };
    public IEnumerable<string> MetadataKeys => metadataKeys;

    public GenerationResult Generate(OpenApiDocumentConfiguration documentConfiguration)
    {
        if (!TryParseFile(documentConfiguration.DocumentContents, out var document, out var diagnostic))
        {
            if (diagnostic != null)
                return new GenerationResult(Array.Empty<OpenApiCodegen.SourceEntry>(), diagnostic.ToArray());
            // TODO - should never happen
            return new GenerationResult(Array.Empty<OpenApiCodegen.SourceEntry>(), Array.Empty<DiagnosticInfo>());
        }
        var sourceProvider = CreateSourceProvider(document, documentConfiguration.AdditionalTextMetadata);
        var openApiDiagnostic = new OpenApiTransformDiagnostic();

        var sources = (from entry in sourceProvider.GetSources(openApiDiagnostic)
                       select new OpenApiCodegen.SourceEntry(entry.Key, entry.SourceText)).ToArray();

        return new GenerationResult(
            sources,
            // TODO - do something with the errors in openApiDiagnostic.Errors!
            Array.Empty<DiagnosticInfo>()
        );
    }

    private static bool TryParseFile(string openapiTextContent, [NotNullWhen(true)] out OpenApiDocument? document, out IReadOnlyList<DiagnosticInfo> diagnostic)
    {
        document = null;
        diagnostic = Array.Empty<DiagnosticInfo>();
        try
        {
            var reader = new OpenApiStringReader();
            document = reader.Read(openapiTextContent, out var openApiDiagnostic);
            if (openApiDiagnostic.Errors.Any())
            {
                // TODO - report issues

                return false;
            }

            return true;
        }
        catch
        {
            // TODO - report invalid files
            return false;
        }
    }

    private static ISourceProvider CreateSourceProvider(OpenApiDocument document, IReadOnlyDictionary<string, string?> opt)
    {
        var options = LoadOptions(opt[propConfig]);
        var documentNamespace = opt[propNamespace];
        if (string.IsNullOrEmpty(documentNamespace))
            documentNamespace = GetStandardNamespace(opt, options);

        return document.BuildCSharpClientSourceProvider(GetVersionInfo(), documentNamespace, options);
    }

    private static CSharpSchemaOptions LoadOptions(string? optionsFiles)
    {
        using var defaultJsonStream = CSharpSchemaOptions.GetDefaultOptionsJson();
        var builder = new ConfigurationBuilder();
        builder.AddYamlStream(defaultJsonStream);
        if (optionsFiles is { Length: > 0 })
        {
            foreach (var file in optionsFiles.Split(';'))
            {
                if (System.IO.File.Exists(file))
                {
                    builder.AddYamlFile(file);
                }
            }
        }
        var result = builder.Build().Get<CSharpSchemaOptions>();
        // TODO - generate diagnostic instead of throwing exception
        if (result == null) throw new InvalidOperationException("Could not build schema options");
        return result;
    }

    private static string GetVersionInfo()
    {
        return $"{typeof(CSharpClientTransformer).FullName} v{typeof(CSharpClientTransformer).Assembly.GetName().Version}";
    }

    private static string? GetStandardNamespace(IReadOnlyDictionary<string, string?> opt, CSharpSchemaOptions options)
    {
        var identity = opt["identity"];
        var link = opt["link"];
        opt.TryGetValue("build_property.projectdir", out var projectDir);
        opt.TryGetValue("build_property.rootnamespace", out var rootNamespace);

        return CSharpNaming.ToNamespace(rootNamespace, projectDir, identity, link, options.ReservedIdentifiers());
    }
}
