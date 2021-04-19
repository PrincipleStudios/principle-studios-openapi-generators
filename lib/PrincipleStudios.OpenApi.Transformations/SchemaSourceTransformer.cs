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
            // TODO - deep complex objects
            //foreach (var operation in document.Paths.SelectMany(path => path.Value.Operations.Values))
            //{
            //    foreach (var parameter in operation.Parameters)
            //    {
            //        if (!openApiSchemaTransformer.UseInline(parameter.Schema))
            //            yield return openApiSchemaTransformer.TransformParameter(operation, parameter);
            //    }
            //    foreach (var response in operation.Responses)
            //    {
            //        foreach (var mediaType in response.Value.Content.Values)
            //        {
            //            if (!openApiSchemaTransformer.UseInline(mediaType.Schema))
            //                yield return openApiSchemaTransformer.TransformResponse(operation, response, mediaType);
            //        }
            //    }
            //}

            foreach (var componentSchema in document.Components.Schemas)
            {
                if (!openApiSchemaTransformer.UseReference(componentSchema.Value))
                    yield return openApiSchemaTransformer.TransformComponentSchema(componentSchema.Key, componentSchema.Value);
            }
        }

    }
}
