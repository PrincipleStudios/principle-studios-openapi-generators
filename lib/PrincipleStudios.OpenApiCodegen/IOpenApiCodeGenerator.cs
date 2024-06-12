using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PrincipleStudios.OpenApiCodegen;

public record SourceEntry(string Key, string SourceText);


[DebuggerDisplay("{Line},{Column}")]
public record DiagnosticLocationMark(int Line, int Column);
[DebuggerDisplay("{Start},{End}")]
public record DiagnosticLocationRange(DiagnosticLocationMark Start, DiagnosticLocationMark End)
{
}

public record DiagnosticLocation(string FilePath, DiagnosticLocationRange? Range);

// TODO: metadata
public record DiagnosticInfo(string Id, DiagnosticLocation Location);

public record GenerationResult(IReadOnlyList<SourceEntry> Sources, IReadOnlyList<DiagnosticInfo> Diagnostics);

// Note: This interface is not used directly, but is used by the `BaseGenerator` via reflection/compiled lambdas
public interface IOpenApiCodeGenerator
{
	IEnumerable<string> MetadataKeys { get; }

	GenerationResult Generate(string documentPath, string documentContents, IReadOnlyDictionary<string, string?> additionalTextMetadata);
}
