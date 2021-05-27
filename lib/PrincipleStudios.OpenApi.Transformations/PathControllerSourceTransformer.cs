using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations
{
    public class PathControllerSourceTransformer : ISourceProvider
    {
        private readonly OpenApiDocument document;
        private readonly IOpenApiPathControllerTransformer pathControllerTransformer;

        public PathControllerSourceTransformer(OpenApiDocument document, IOpenApiPathControllerTransformer pathControllerTransformer)
        {
            this.document = document;
            this.pathControllerTransformer = pathControllerTransformer;
        }

        public IEnumerable<SourceEntry> GetSources(OpenApiTransformDiagnostic diagnostic)
        {
            foreach (var controller in document.Paths)
            {
                if (SafeTransform(controller.Key, controller.Value, diagnostic) is SourceEntry entry)
                    yield return entry;
            }
        }

        private SourceEntry? SafeTransform(string key, OpenApiPathItem value, OpenApiTransformDiagnostic diagnostic)
        {
            try
            {
                return pathControllerTransformer.TransformController(key, value, diagnostic);
            }
            catch (Exception ex)
            {
                diagnostic.Errors.Add(new($"#/paths/{key.ToOpenApiPathContext()}", $"Unhandled exception: {ex.Message}"));
                return null;
            }
        }
    }
}
