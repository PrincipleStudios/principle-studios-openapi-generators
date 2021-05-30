using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace PrincipleStudios.OpenApi.Transformations
{
    public interface IOpenApiPathControllerTransformer
    {
        SourceEntry TransformController(OpenApiPathItem value, OpenApiContext baseContext, OpenApiTransformDiagnostic diagnostic);
    }
}