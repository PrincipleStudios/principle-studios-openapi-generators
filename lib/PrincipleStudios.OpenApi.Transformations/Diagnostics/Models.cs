using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations.Diagnostics;
public record FileLocationMark(int Line, int Column);
public record FileLocationRange(FileLocationMark Start, FileLocationMark End);

public record PreciseLocation(Uri RetrievalUri, FileLocationRange Range);
public record DocumentUriLocation(Uri DocumentLocation);

public record Location(Either<DocumentUriLocation, PreciseLocation> Value)
{
	public Location(Uri documentUri) : this(new Either<DocumentUriLocation, PreciseLocation>.Left(new(documentUri))) { }
	public Location(Uri retrievalUri, FileLocationMark start, FileLocationMark end)
		 : this(new Either<DocumentUriLocation, PreciseLocation>.Right(new(retrievalUri, new FileLocationRange(start, end)))) { }
}


public abstract record DiagnosticBase(Location Location);
