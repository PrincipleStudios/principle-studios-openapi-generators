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
        private readonly PathVisitor visitor;

        public PathControllerSourceTransformer(OpenApiDocument document, IOpenApiPathControllerTransformer pathControllerTransformer)
        {
            this.document = document;
            this.pathControllerTransformer = pathControllerTransformer;
            this.visitor = new PathVisitor(pathControllerTransformer);
        }

        public IEnumerable<SourceEntry> GetSources(OpenApiTransformDiagnostic diagnostic)
        {
            var result = new List<SourceEntry>();
            visitor.Visit(document, OpenApiContext.From(document), new PathVisitor.Argument(result.Add, diagnostic));
            return result;
        }

        class PathVisitor : OpenApiDocumentVisitor<PathVisitor.Argument>
        {
            private readonly IOpenApiPathControllerTransformer pathControllerTransformer;
            public record Argument(RegisterSourceEntry RegisterSourceEntry, OpenApiTransformDiagnostic Diagnostic);
            public delegate void RegisterSourceEntry(SourceEntry sourceEntry);

            public PathVisitor(IOpenApiPathControllerTransformer pathControllerTransformer)
            {
                this.pathControllerTransformer = pathControllerTransformer;
            }

            public override void Visit(OpenApiPathItem pathItem, OpenApiContext context, Argument argument)
            {
                try
                {
                    argument.RegisterSourceEntry(
                        pathControllerTransformer.TransformController(pathItem, context, argument.Diagnostic)
                    );
                }
                catch (Exception ex)
                {
                    argument.Diagnostic.Errors.Add(new(context, $"Unhandled exception: {ex.Message}"));
                }
            }

            public override void Visit(OpenApiExternalDocs ignored, OpenApiContext context, Argument argument) { }
            public override void Visit(OpenApiServer ignored, OpenApiContext context, Argument argument) { }
            public override void Visit(OpenApiComponents ignored, OpenApiContext context, Argument argument) { }
            public override void Visit(OpenApiInfo ignored, OpenApiContext context, Argument argument) { }
            public override void Visit(OpenApiSecurityRequirement ignored, OpenApiContext context, Argument argument) { }
            public override void Visit(OpenApiTag ignored, OpenApiContext context, Argument argument) { }


        }
    }
}
