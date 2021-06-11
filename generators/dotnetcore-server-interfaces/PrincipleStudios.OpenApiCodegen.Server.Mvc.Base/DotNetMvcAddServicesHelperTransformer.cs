using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.Transformations;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApi.CSharp
{
    public class DotNetMvcAddServicesHelperTransformer : ISourceProvider
    {
        private readonly OpenApiDocument document;
        private CSharpControllerTransformer schemaTransformer;

        public DotNetMvcAddServicesHelperTransformer(OpenApiDocument document, CSharpControllerTransformer schemaTransformer)
        {
            this.document = document;
            this.schemaTransformer = schemaTransformer;
        }

        public IEnumerable<SourceEntry> GetSources(OpenApiTransformDiagnostic diagnostic)
        {
            yield return schemaTransformer.TransformAddServicesHelper(document.Paths, diagnostic);
        }
    }
}
