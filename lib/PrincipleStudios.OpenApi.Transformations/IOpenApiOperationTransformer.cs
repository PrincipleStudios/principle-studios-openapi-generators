using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApiCodegen;

namespace PrincipleStudios.OpenApi.Transformations
{
	public interface IOpenApiOperationTransformer
	{
		SourceEntry TransformOperation(OpenApiOperation operation, OpenApiContext context, OpenApiTransformDiagnostic diagnostic);
	}

}