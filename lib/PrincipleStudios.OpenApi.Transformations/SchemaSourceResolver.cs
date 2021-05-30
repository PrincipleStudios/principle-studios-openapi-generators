using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations
{
    public delegate void SchemaCallback(OpenApiSchema schema, OpenApiContext context);
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

        public InlineDataTypeResolver<TInlineDataType> ToInlineDataType(OpenApiSchema schema, OpenApiContext context, OpenApiTransformDiagnostic diagnostic)
        {
            context.AssertLast(schema);

            EnsureSchemasRegistered(schema, context, diagnostic);

            return referencedSchemas[schema].Inline;
        }

        public IEnumerable<SourceEntry> GetSources(OpenApiTransformDiagnostic diagnostic) => 
            from entry in referencedSchemas.Values
            let sourceEntry = entry.GetSourceEntry?.Invoke()
            where sourceEntry != null
            select sourceEntry.Value;

        public void EnsureSchemasRegistered(Microsoft.OpenApi.Interfaces.IOpenApiElement element, OpenApiContext context, OpenApiTransformDiagnostic diagnostic)
        {
            var newSchemas = new Dictionary<OpenApiSchema, List<OpenApiContext>>();
            this.schemaVisitor.VisitAny(element, context, (nestedSchema, nestedContext) =>
            {
                if (!newSchemas.ContainsKey(nestedSchema))
                    newSchemas.Add(nestedSchema, new());
                newSchemas[nestedSchema].Add(nestedContext);
            });
            
            foreach (var s in newSchemas)
            {
                if (referencedSchemas.ContainsKey(s.Key))
                {
                    referencedSchemas[s.Key].AllContext.AddRange(s.Value);
                    continue;
                }

                if (s.Key.UnresolvedReference)
                {
                    diagnostic.Errors.Add(new OpenApiTransformError(s.Value.First(), $"Unresolved external reference: {s.Key.Reference.Id} @ {s.Key.Reference.ExternalResource}"));
                    referencedSchemas[s.Key] = new SchemaSourceEntry
                    {
                        Schema = s.Key,
                        Inline = UnresolvedReferencePlaceholder,
                        AllContext = new List<OpenApiContext>(s.Value),
                        GetSourceEntry = null,
                    };
                    continue;
                }

                var allContext = new List<OpenApiContext>(s.Value);
                referencedSchemas[s.Key] = new SchemaSourceEntry
                {
                    Schema = s.Key,
                    Inline = () => GetInlineDataType(s.Key, allContext, diagnostic),
                    AllContext = allContext,
                    GetSourceEntry = () => GetSourceEntry(s.Key, allContext, diagnostic),
                };
            }
        }

        protected abstract TInlineDataType GetInlineDataType(OpenApiSchema schema, IEnumerable<OpenApiContext> allContexts, OpenApiTransformDiagnostic diagnostic);
        protected abstract SourceEntry? GetSourceEntry(OpenApiSchema schema, IEnumerable<OpenApiContext> allContexts, OpenApiTransformDiagnostic diagnostic);

        protected abstract TInlineDataType UnresolvedReferencePlaceholder();

        public delegate SourceEntry? SourceEntryResolver();

        private struct SchemaSourceEntry
        {
            public OpenApiSchema Schema { get; set; }
            public InlineDataTypeResolver<TInlineDataType> Inline { get; set; }
            public List<OpenApiContext> AllContext { get; set; }
            public SourceEntryResolver? GetSourceEntry { get; set; }
        }
    }

    public class DefaultSchemaVisitor : OpenApiDocumentVisitor<SchemaCallback>
    {
        public override void Visit(OpenApiSchema schema, OpenApiContext context, SchemaCallback callback)
        {
            callback(schema, context);
            base.Visit(schema, context, callback);
        }
    }
}
