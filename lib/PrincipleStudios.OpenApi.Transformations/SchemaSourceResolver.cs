using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations
{
    public abstract class SchemaSourceResolver<TInlineDataType> : ISchemaSourceResolver<TInlineDataType>
    {
        protected readonly IOpenApiSchemaTransformer openApiSchemaTransformer;
        private readonly Dictionary<OpenApiSchema, SchemaSourceEntry> referencedSchemas = new Dictionary<OpenApiSchema, SchemaSourceEntry>();

        public SchemaSourceResolver(IOpenApiSchemaTransformer openApiSchemaTransformer)
        {
            this.openApiSchemaTransformer = openApiSchemaTransformer;
        }

        public TInlineDataType ToInlineDataType(OpenApiSchema schema, OpenApiTransformDiagnostic diagnostic)
        {
            if (referencedSchemas.ContainsKey(schema))
                return referencedSchemas[schema].Inline;
            var result = ToInlineDataTypeWithReference(schema, diagnostic);
            referencedSchemas[schema] = result;
            return result.Inline;
        }

        protected IEnumerable<TInlineDataType> RegisteredInlineDataTypes => referencedSchemas.Values.Select(sourceEntry => sourceEntry.Inline);

        protected abstract SchemaSourceResolver<TInlineDataType>.SchemaSourceEntry ToInlineDataTypeWithReference(OpenApiSchema schema, OpenApiTransformDiagnostic diagnostic);

        public IEnumerable<SourceEntry> GetSources(OpenApiTransformDiagnostic diagnostic) => referencedSchemas.Values.Where(sourceEntry => sourceEntry.SourceEntry.HasValue).Select(sourceEntry => sourceEntry.SourceEntry!.Value);


        public struct SchemaSourceEntry
        {
            public TInlineDataType Inline { get; set; }
            public SourceEntry? SourceEntry { get; set; }
        }
    }
}
