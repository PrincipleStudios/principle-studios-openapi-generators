using System;
using System.Collections.Generic;

namespace PrincipleStudios.OpenApi.Transformations.Diagnostics;

public class MultipleDiagnosticException(IReadOnlyList<DiagnosticBase> diagnostics) : Exception
{
	public IReadOnlyList<DiagnosticBase> Diagnostics => diagnostics;
}