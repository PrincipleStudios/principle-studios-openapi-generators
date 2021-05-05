using Microsoft.OpenApi.Models;

namespace PrincipleStudios.OpenApi.Transformations
{
    public interface ISchemaSourceResolver<TInlineDataType> : ISourceProvider
    {
        TInlineDataType ToInlineDataType(OpenApiSchema schema);
    }
}