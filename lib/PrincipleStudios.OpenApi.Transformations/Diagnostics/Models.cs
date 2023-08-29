using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations.Diagnostics;
[DebuggerDisplay("{Line},{Column}")]
public record FileLocationMark(int Line, int Column);
[DebuggerDisplay("{Start},{End}")]
public record FileLocationRange(FileLocationMark Start, FileLocationMark End);

[DebuggerDisplay("{RetrievalUri}({Range})")]
public record PreciseLocation(Uri RetrievalUri, FileLocationRange Range);
public record DocumentUriLocation(Uri DocumentLocation);

[DebuggerDisplay("{Value}")]
public record Location(Either<DocumentUriLocation, PreciseLocation> Value)
{
	public Location(Uri documentUri) : this(new Either<DocumentUriLocation, PreciseLocation>.Left(new(documentUri))) { }

	public Location(Uri retrievalUri, FileLocationRange range)
		 : this(new Either<DocumentUriLocation, PreciseLocation>.Right(new(retrievalUri, range))) { }

	public Location(Uri retrievalUri, FileLocationMark start, FileLocationMark end)
		 : this(retrievalUri, new FileLocationRange(start, end)) { }
}


public abstract record DiagnosticBase(Location Location);
