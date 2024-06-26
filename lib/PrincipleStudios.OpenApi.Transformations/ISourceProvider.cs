using System.Collections.Generic;
using PrincipleStudios.OpenApiCodegen;

namespace PrincipleStudios.OpenApi.Transformations
{
	public interface ISourceProvider
	{
		IEnumerable<SourceEntry> GetSources(OpenApiTransformDiagnostic diagnostic);
	}
}