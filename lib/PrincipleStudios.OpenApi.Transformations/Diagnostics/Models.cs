using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations.Diagnostics;
[DebuggerDisplay("{Line},{Column}")]
public record FileLocationMark(int Line, int Column);
[DebuggerDisplay("{Start},{End}")]
public record FileLocationRange(FileLocationMark Start, FileLocationMark End)
{
}

[DebuggerDisplay("{RetrievalUri}({Range})")]
public record Location(Uri RetrievalUri, FileLocationRange? Range = null)
{
}


public abstract record DiagnosticBase(Location Location);
