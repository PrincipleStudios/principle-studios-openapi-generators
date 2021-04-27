using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PrincipleStudios.OpenApi.Transformations
{
    public class SchemaSourceTransformer : IOpenApiSourceTransformer
    {
        private readonly IOpenApiSchemaTransformer openApiSchemaTransformer;
        private readonly Regex _2xxRegex = new Regex("2[0-9]{2}");

        public SchemaSourceTransformer(IOpenApiSchemaTransformer openApiSchemaTransformer)
        {
            this.openApiSchemaTransformer = openApiSchemaTransformer;
        }

        public IEnumerable<SourceEntry> ToSourceEntries(OpenApiDocument document, OpenApiTransformDiagnostic diagnostic)
        {
            var allSchemas = new HashSet<(OpenApiSchema schema, string openApiPath)>();
            var baseSchemas = new Queue<(OpenApiSchema schema, string name, string openApiPath, bool skip)>(new[]
            {
                from schema in document.Components?.Schemas?.Values.AsEnumerable() ?? Enumerable.Empty<OpenApiSchema>()
                select (schema, new[] { openApiSchemaTransformer.UseReferenceName(schema) }.AsEnumerable(), $"#/components/schemas/{schema.Reference.Id.ToOpenApiPathContext()}", false),

                from path in document.Paths
                from operation in path.Value.Operations
                from param in operation.Value.Parameters
                select (param.Schema, new[] { operation.Value.OperationId, param.Name }.AsEnumerable(), $"#/paths/{path.Key.ToOpenApiPathContext()}/{operation.Key}/parameters/{param.Name.ToOpenApiPathContext()}/schema", false),

                from path in document.Paths
                from operation in path.Value.Operations
                let onlyResponseType = (operation.Value.RequestBody?.Content.Count ?? 0) == 1
                from requestType in (operation.Value.RequestBody?.Content.AsEnumerable() ?? Enumerable.Empty<KeyValuePair<string, OpenApiMediaType>>())
                let isForm = requestType.Key == "application/x-www-form-urlencoded"
                select (requestType.Value.Schema, new[] { operation.Value.OperationId, onlyResponseType ? "" : requestType.Key, "request" }.AsEnumerable(), $"#/paths/{path.Key.ToOpenApiPathContext()}/{operation.Key}/requestBody/content/{requestType.Key.ToOpenApiPathContext()}/schema", isForm),

                from path in document.Paths
                from operation in path.Value.Operations
                let onlyResponse = operation.Value.Responses.Count == 1
                from response in operation.Value.Responses
                let responseName = onlyResponse ? ""
                    : _2xxRegex.IsMatch(response.Key) && operation.Value.Responses.Keys.Count(_2xxRegex.IsMatch) == 1 ? ""
                    : response.Key == "default" && !operation.Value.Responses.ContainsKey("other") ? "other"
                    : response.Key
                where response.Value.Reference == null
                let onlyResponseType = response.Value.Content.Count == 1
                from responseType in response.Value.Content
                select (responseType.Value.Schema, new[] { operation.Value.OperationId, responseName, onlyResponseType ? "" : responseType.Key, "response" }.AsEnumerable(), $"#/paths/{path.Key.ToOpenApiPathContext()}/{operation.Key}/responses/{response.Key.ToOpenApiPathContext()}/content/{responseType.Key.ToOpenApiPathContext()}/schema", false),

                from response in document.Components?.Responses?.AsEnumerable() ?? Enumerable.Empty<KeyValuePair<string, OpenApiResponse>>()
                where response.Value.Reference == null
                from responseType in response.Value.Content
                select (responseType.Value.Schema, new[] { response.Key, responseType.Key }.AsEnumerable(), $"#/components/responses/{response.Key.ToOpenApiPathContext()}/content/{responseType.Key.ToOpenApiPathContext()}/schema", false),
            }.SelectMany(p => p).Select(p => (p.Item1, string.Join(" ", p.Item2), p.Item3, p.Item4)));

            while (baseSchemas.Count > 0)
            {
                var entry = baseSchemas.Dequeue();
                var name = entry.name;
                if (entry.schema.Reference != null)
                {
                    name = entry.schema.Reference.Id;
                    if (baseSchemas.Any(e => e.schema == entry.schema && e.openApiPath.Length <= entry.openApiPath.Length) || allSchemas.Any(e => e.schema == entry.schema))
                        continue;
                }
                else if (entry.schema.Reference == null && openApiSchemaTransformer.MakeReference(entry.schema))
                {
                    entry.schema.Reference = new OpenApiReference { Id = name, Type = ReferenceType.Schema };
                }
                if (entry.schema.Reference != null && !allSchemas.Any(e => e.schema == entry.schema) && !entry.skip)
                {
                    allSchemas.Add((entry.schema, entry.openApiPath));
                }

                if (entry.schema.Type == "object" && entry.schema.Properties != null)
                {
                    foreach (var p in entry.schema.Properties)
                    {
                        baseSchemas.Enqueue((p.Value, name + " " + p.Key, $"{entry.openApiPath}", false));
                    }
                }
                if (entry.schema.Type == "array")
                {
                    baseSchemas.Enqueue((entry.schema.Items, name + " items", $"{entry.openApiPath}/items", false));
                }
                //for (var i = 0; i < entry.schema.AllOf.Count; i++)
                //{
                //    baseSchemas.Enqueue((entry.schema.AllOf[i], context + " part" + i.ToString()));
                //}
                for (var i = 0; i < entry.schema.AnyOf.Count; i++)
                {
                    baseSchemas.Enqueue((entry.schema.AnyOf[i], name + " option" + i.ToString(), $"{entry.openApiPath}/anyOf/{i}", false));
                }
                for (var i = 0; i < entry.schema.OneOf.Count; i++)
                {
                    baseSchemas.Enqueue((entry.schema.OneOf[i], name + " option" + i.ToString(), $"{entry.openApiPath}/oneOf/{i}", false));
                }
            }

            foreach (var componentSchema in allSchemas)
            {
                if (SafeTransform(componentSchema.schema, componentSchema.openApiPath, diagnostic) is SourceEntry e)
                    yield return e;
            }
        }

        private SourceEntry? SafeTransform(OpenApiSchema componentSchema, string openApiPath, OpenApiTransformDiagnostic diagnostic)
        {
            try
            {
                if (componentSchema.UnresolvedReference)
                {
                    diagnostic.Errors.Add(new(openApiPath, $"Unrsolved external reference: {componentSchema.Reference.Id} @ {componentSchema.Reference.ExternalResource}"));
                    return null;
                }
                if (!openApiSchemaTransformer.UseReference(componentSchema))
                    return null;
                return openApiSchemaTransformer.TransformSchema(componentSchema, diagnostic);
            }
            catch (Exception ex)
            {
                diagnostic.Errors.Add(new(openApiPath, $"Unhandled exception: {ex.Message}"));
                return null;
            }
        }

    }
}
