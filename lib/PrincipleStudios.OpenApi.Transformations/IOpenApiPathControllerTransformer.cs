using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace PrincipleStudios.OpenApi.Transformations
{
    public interface IOpenApiPathControllerTransformer
    {
        SourceEntry TransformController(string key, OpenApiPathItem value, OpenApiTransformDiagnostic diagnostic);
    }
}