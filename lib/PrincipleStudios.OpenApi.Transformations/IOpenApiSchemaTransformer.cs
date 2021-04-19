using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace PrincipleStudios.OpenApi.Transformations
{
    public interface IOpenApiSchemaTransformer
    {
        bool UseInline(OpenApiSchema schema);
        bool UseReference(OpenApiSchema schema);

        SourceEntry TransformParameter(OpenApiOperation operation, OpenApiParameter parameter);
        SourceEntry TransformResponse(OpenApiOperation operation, KeyValuePair<string, OpenApiResponse> response, OpenApiMediaType mediaType);
        SourceEntry TransformComponentSchema(string key, OpenApiSchema schema);
    }
}