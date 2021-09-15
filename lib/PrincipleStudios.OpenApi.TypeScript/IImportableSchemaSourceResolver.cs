using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.Transformations;
using System.Collections.Generic;

namespace PrincipleStudios.OpenApi.TypeScript
{
    public interface IImportableSchemaSourceResolver<TInlineDataType> : ISchemaSourceResolver<TInlineDataType>
    {
        IEnumerable<templates.ImportStatement> GetImportStatements(IEnumerable<OpenApiSchema> schemasReferenced, string path);
    }
}