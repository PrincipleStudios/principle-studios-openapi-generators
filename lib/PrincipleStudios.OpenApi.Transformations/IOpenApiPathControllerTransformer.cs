using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace PrincipleStudios.OpenApi.Transformations
{
    public interface IOpenApiPathControllerTransformer
    {
        IEnumerable<SourceEntry> TransformController(string key, OpenApiPathItem value);
    }
}