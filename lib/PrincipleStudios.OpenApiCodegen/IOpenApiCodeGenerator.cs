using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApiCodegen;

public record SourceEntry(string Key, string SourceText);

// TODO: add location info
public record DiagnosticInfo(string Id);

public record GenerationResult(IReadOnlyList<SourceEntry> Sources, IReadOnlyList<DiagnosticInfo> Diagnostics);

// TODO: options contents? external ref files contents?
public record OpenApiDocumentConfiguration(string DocumentContents, IReadOnlyDictionary<string, string?> AdditionalTextMetadata);

public interface IOpenApiCodeGenerator
{
    IEnumerable<string> MetadataKeys { get; }

    GenerationResult Generate(OpenApiDocumentConfiguration document);
}
