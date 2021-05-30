using Microsoft.OpenApi.Models;

namespace PrincipleStudios.OpenApi.Transformations
{
    public delegate TInlineDataType InlineDataTypeResolver<TInlineDataType>();

    public interface ISchemaSourceResolver<TInlineDataType> : ISourceProvider
    {
        InlineDataTypeResolver<TInlineDataType> ToInlineDataType(OpenApiSchema schema, OpenApiContext context, OpenApiTransformDiagnostic diagnostic);
        void EnsureSchemasRegistered(Microsoft.OpenApi.Interfaces.IOpenApiElement element, OpenApiContext context, OpenApiTransformDiagnostic diagnostic);
    }
}