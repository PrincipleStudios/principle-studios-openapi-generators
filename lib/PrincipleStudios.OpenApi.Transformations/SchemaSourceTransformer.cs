using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations
{
    public class SchemaSourceTransformer : IOpenApiSourceTransformer
    {
        private readonly IOpenApiSchemaTransformer openApiSchemaTransformer;

        public SchemaSourceTransformer(IOpenApiSchemaTransformer openApiSchemaTransformer)
        {
            this.openApiSchemaTransformer = openApiSchemaTransformer;
        }

        public IEnumerable<SourceEntry> ToSourceEntries(OpenApiDocument document)
        {
            var extraSchemas = new HashSet<OpenApiSchema>();
            var baseSchemas = new Stack<(OpenApiSchema schema, string context)>(new[]
            {
                from path in document.Paths
                from operation in path.Value.Operations
                from param in operation.Value.Parameters
                select (param.Schema, new[] { operation.Value.OperationId, param.Name }.AsEnumerable()),

                from path in document.Paths
                from operation in path.Value.Operations
                from requestType in (operation.Value.RequestBody?.Content.AsEnumerable() ?? Enumerable.Empty<KeyValuePair<string, OpenApiMediaType>>())
                select (requestType.Value.Schema, new[] { operation.Value.OperationId, requestType.Key, "request" }.AsEnumerable()),

                from path in document.Paths
                from operation in path.Value.Operations
                from response in operation.Value.Responses
                where response.Value.Reference == null
                from responseType in response.Value.Content
                select (responseType.Value.Schema, new[] { operation.Value.OperationId, response.Key, responseType.Key }.AsEnumerable()),

                from schema in document.Components?.Schemas?.Values.AsEnumerable() ?? Enumerable.Empty<OpenApiSchema>()
                select (schema, new[] { openApiSchemaTransformer.UseReferenceName(schema) }.AsEnumerable()),

                from response in document.Components?.Responses?.AsEnumerable() ?? Enumerable.Empty<KeyValuePair<string, OpenApiResponse>>()
                where response.Value.Reference == null
                from responseType in response.Value.Content
                select (responseType.Value.Schema, new[] { response.Key, responseType.Key }.AsEnumerable()),
            }.SelectMany(p => p).Select(p => (p.Item1, string.Join(" ", p.Item2))));

            for (var entry = baseSchemas.Pop(); baseSchemas.Count > 0; entry = baseSchemas.Pop())
            {
                var context = entry.context;
                if (entry.schema.Reference != null)
                {
                    context = entry.schema.Reference.Id;
                }
                else if (entry.schema.Reference == null && openApiSchemaTransformer.MakeReference(entry.schema))
                {
                    entry.schema.Reference = new OpenApiReference { Id = context, Type = ReferenceType.Schema };
                    extraSchemas.Add(entry.schema);
                }

                if (entry.schema.Type == "object" && entry.schema.Properties != null)
                {
                    foreach (var p in entry.schema.Properties)
                    {
                        baseSchemas.Push((p.Value, context + " " + p.Key));
                    }
                }
                if (entry.schema.Type == "array")
                {
                    baseSchemas.Push((entry.schema.Items, context + " items"));
                }
                //for (var i = 0; i < entry.schema.AllOf.Count; i++)
                //{
                //    baseSchemas.Push((entry.schema.AllOf[i], context + " part" + i.ToString()));
                //}
                for (var i = 0; i < entry.schema.AnyOf.Count; i++)
                {
                    baseSchemas.Push((entry.schema.AnyOf[i], context + " option" + i.ToString()));
                }
                for (var i = 0; i < entry.schema.OneOf.Count; i++)
                {
                    baseSchemas.Push((entry.schema.OneOf[i], context + " option" + i.ToString()));
                }
            }

            foreach (var componentSchema in (document.Components?.Schemas?.Values.AsEnumerable() ?? Enumerable.Empty<OpenApiSchema>()).Concat(extraSchemas))
            {
                if (openApiSchemaTransformer.UseReference(componentSchema))
                    yield return openApiSchemaTransformer.TransformSchema(componentSchema);
            }
        }

    }
}
