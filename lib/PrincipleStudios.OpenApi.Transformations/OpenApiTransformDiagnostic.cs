using System.Collections.Generic;

namespace PrincipleStudios.OpenApi.Transformations
{
	public class OpenApiTransformDiagnostic
	{
		public IList<OpenApiTransformError> Errors { get; } = new List<OpenApiTransformError>();
	}
}