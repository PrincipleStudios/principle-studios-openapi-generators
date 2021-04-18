using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations
{
    public class PathControllerSourceTransformer : IOpenApiSourceTransformer
    {
        private readonly IOpenApiPathControllerTransformer pathControllerTransformer;

        public PathControllerSourceTransformer(IOpenApiPathControllerTransformer pathControllerTransformer)
        {
            this.pathControllerTransformer = pathControllerTransformer;
        }

        public IEnumerable<SourceEntry> ToSourceEntries(OpenApiDocument document)
        {
            foreach (var controller in document.Paths)
            {
                yield return pathControllerTransformer.TransformController(controller.Key, controller.Value);
            }
        }
    }
}
