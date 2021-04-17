using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace PrincipleStudios.OpenApi.Transformations
{
    public interface IOpenApiTagControllerTransformer
    {
        IEnumerable<SourceEntry> TransformController(string key, IEnumerable<(OpenApiPathItem path, KeyValuePair<OperationType, OpenApiOperation> operation)> operations, OpenApiDocument document);
    }
}