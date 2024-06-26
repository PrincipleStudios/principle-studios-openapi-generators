using System.Collections.Generic;
using PrincipleStudios.OpenApiCodegen;

namespace PrincipleStudios.OpenApi.Transformations
{

	public interface IOpenApiTagControllerTransformer
	{
		IEnumerable<SourceEntry> TransformController(string tag, IEnumerable<OpenApiFullOperation> operations, OpenApiTransformDiagnostic diagnostic);
	}
}