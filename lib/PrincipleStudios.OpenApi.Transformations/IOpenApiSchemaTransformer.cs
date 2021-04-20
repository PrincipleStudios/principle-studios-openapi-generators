using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace PrincipleStudios.OpenApi.Transformations
{
    public interface IOpenApiSchemaTransformer
    {
        bool MakeReference(OpenApiSchema schema);
        bool UseInline(OpenApiSchema schema);
        bool UseReference(OpenApiSchema schema);
        string UseReferenceName(OpenApiSchema schema);

        SourceEntry TransformSchema(OpenApiSchema schema);
    }
}