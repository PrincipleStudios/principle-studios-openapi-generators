using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations
{
    public class TagControllerSourceTransformer : IOpenApiSourceTransformer
    {
        private readonly IOpenApiTagControllerTransformer tagControllerTransformer;
        private readonly string defaultTagName;

        public TagControllerSourceTransformer(IOpenApiTagControllerTransformer tagControllerTransformer, string defaultTagName = "default")
        {
            this.tagControllerTransformer = tagControllerTransformer;
            this.defaultTagName = defaultTagName;
        }

        public IEnumerable<SourceEntry> ToSourceEntries(OpenApiDocument document)
        {
            var controllers = from path in document.Paths.Values
                              from operation in path.Operations
                              let firstTag = operation.Value.Tags.Select(t => t.Name).FirstOrDefault() ?? defaultTagName
                              group (path, operation) by firstTag;

            foreach (var controller in controllers)
            {
                foreach (var entry in tagControllerTransformer.TransformController(controller.Key, controller, document))
                {
                    yield return entry;
                }
            }
        }
    }
}
