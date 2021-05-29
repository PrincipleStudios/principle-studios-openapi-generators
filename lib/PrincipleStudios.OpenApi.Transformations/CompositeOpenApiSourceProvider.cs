using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations
{
    public class CompositeOpenApiSourceProvider : ISourceProvider
    {
        private readonly ISourceProvider[] sourceProviders;

        public CompositeOpenApiSourceProvider(params ISourceProvider[] sourceProviders)
        {
            this.sourceProviders = sourceProviders;
        }

        public IEnumerable<SourceEntry> GetSources(OpenApiTransformDiagnostic diagnostic)
        {
            foreach (var transformer in sourceProviders)
                foreach (var entry in transformer.GetSources(diagnostic))
                    yield return entry;
        }
    }
}
