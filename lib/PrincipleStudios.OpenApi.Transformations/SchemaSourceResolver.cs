using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations
{
    public delegate void SchemaCallback(OpenApiSchema schema, OpenApiContext context, out bool shouldRecurse);
    public abstract class SchemaSourceResolver<TInlineDataType> : ISchemaSourceResolver<TInlineDataType>
    {
        public SchemaSourceResolver()
            : this(new DefaultSchemaVisitor())
        {
        }

        public SchemaSourceResolver(IOpenApiDocumentVisitor<SchemaCallback> schemaVisitor)
        {
            this.schemaVisitor = schemaVisitor;
        }

        private readonly Dictionary<OpenApiSchema, SchemaSourceEntry> referencedSchemas = new Dictionary<OpenApiSchema, SchemaSourceEntry>();
        private readonly IOpenApiDocumentVisitor<SchemaCallback> schemaVisitor;

        public TInlineDataType ToInlineDataType(OpenApiSchema schema, OpenApiContext context, OpenApiTransformDiagnostic diagnostic)
        {
            context.AssertLast(schema);

            EnsureRegistered(schema, context, diagnostic);

            return referencedSchemas[schema].Inline;
        }

        protected IEnumerable<TInlineDataType> RegisteredInlineDataTypes => referencedSchemas.Values.Select(sourceEntry => sourceEntry.Inline);

        protected abstract SchemaSourceResolver<TInlineDataType>.SchemaSourceEntry ToInlineDataTypeWithReference(OpenApiSchema schema, OpenApiContext context, OpenApiTransformDiagnostic diagnostic);

        public IEnumerable<SourceEntry> GetSources(OpenApiTransformDiagnostic diagnostic) => referencedSchemas.Values.Where(sourceEntry => sourceEntry.SourceEntry != null).Select(sourceEntry => sourceEntry.SourceEntry!.Value);

        public void EnsureRegistered(OpenApiSchema schema, OpenApiContext context, OpenApiTransformDiagnostic diagnostic)
        {
            var newSchemas = new Dictionary<OpenApiSchema, List<OpenApiContext>>();
            this.schemaVisitor.Visit(schema, context, (OpenApiSchema nestedSchema, OpenApiContext nestedContext, out bool shouldRecurse) =>
            {
                shouldRecurse = !(newSchemas.ContainsKey(nestedSchema) || referencedSchemas.ContainsKey(nestedSchema));
                if (!newSchemas.ContainsKey(nestedSchema))
                    newSchemas.Add(nestedSchema, new());
                newSchemas[nestedSchema].Add(nestedContext);
            });
            
            foreach (var s in newSchemas)
            {
                if (s.Key.UnresolvedReference)
                {
                    diagnostic.Errors.Add(new OpenApiTransformError(s.Value.First(), $"Unresolved external reference: {s.Key.Reference.Id} @ {s.Key.Reference.ExternalResource}"));
                    referencedSchemas[schema] = new SchemaSourceEntry
                    {
                        Inline = UnresolvedReferencePlaceholder(),
                        SourceEntry = null,
                    };
                    continue;
                }

                referencedSchemas[schema] = ToInlineDataTypeWithReference(s.Key, s.Value.First() /* TODO - don't just use the first one?? */, diagnostic);
            }
        }

        protected abstract TInlineDataType UnresolvedReferencePlaceholder();

        public struct SchemaSourceEntry
        {
            public TInlineDataType Inline { get; set; }
            public SourceEntry? SourceEntry { get; set; }
        }
    }

    public class DefaultSchemaVisitor : OpenApiDocumentVisitor<SchemaCallback>
    {
        public override void Visit(OpenApiSchema schema, OpenApiContext context, SchemaCallback callback)
        {
            callback(schema, context, out var shouldRecurse);
            if (shouldRecurse)
                base.Visit(schema, context, callback);
        }
    }
}
