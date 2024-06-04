using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApiCodegen;

public record SourceEntry(string Key, string SourceText);

// TODO: add location info
public record DiagnosticInfo(string Id);

public record GenerationResult(IReadOnlyList<SourceEntry> Sources, IReadOnlyList<DiagnosticInfo> Diagnostics);

// Note: This interface is not used directly, but is used by the `BaseGenerator` via reflection/compiled lambdas
public interface IOpenApiCodeGenerator
{
	IEnumerable<string> MetadataKeys { get; }

	GenerationResult Generate(string documentPath, string documentContents, IReadOnlyDictionary<string, string?> additionalTextMetadata);
}
