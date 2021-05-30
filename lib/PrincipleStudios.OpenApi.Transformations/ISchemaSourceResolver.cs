using Microsoft.OpenApi.Models;

namespace PrincipleStudios.OpenApi.Transformations
{
    public interface ISchemaSourceResolver<TInlineDataType> : ISourceProvider
    {
        TInlineDataType ToInlineDataType(OpenApiSchema schema, OpenApiContext context, OpenApiTransformDiagnostic diagnostic);
        void EnsureSchemasRegistered(Microsoft.OpenApi.Interfaces.IOpenApiElement element, OpenApiContext context, OpenApiTransformDiagnostic diagnostic);
    }
}