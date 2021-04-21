using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.Transformations;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApi.CSharp
{
    public class DotNetMvcAddServicesHelperTransformer : IOpenApiSourceTransformer
    {
        private CSharpPathControllerTransformer schemaTransformer;

        public DotNetMvcAddServicesHelperTransformer(CSharpPathControllerTransformer schemaTransformer)
        {
            this.schemaTransformer = schemaTransformer;
        }

        public IEnumerable<SourceEntry> ToSourceEntries(OpenApiDocument document, OpenApiTransformDiagnostic diagnostic)
        {
            yield return schemaTransformer.TransformAddServicesHelper(document.Paths, diagnostic);
        }
    }
}
