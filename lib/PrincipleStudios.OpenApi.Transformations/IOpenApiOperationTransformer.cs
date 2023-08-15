using Microsoft.OpenApi.Models;

namespace PrincipleStudios.OpenApi.Transformations
{
	public interface IOpenApiOperationTransformer
	{
		SourceEntry TransformOperation(OpenApiOperation operation, OpenApiContext context, OpenApiTransformDiagnostic diagnostic);
	}

}