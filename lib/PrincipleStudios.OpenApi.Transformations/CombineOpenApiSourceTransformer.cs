using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations
{
    public class CombineOpenApiSourceTransformer : IOpenApiSourceTransformer
    {
        private readonly IOpenApiSourceTransformer[] transformers;

        public CombineOpenApiSourceTransformer(params IOpenApiSourceTransformer[] transformers)
        {
            this.transformers = transformers;
        }

        public IEnumerable<SourceEntry> ToSourceEntries(OpenApiDocument document)
        {
            foreach (var transformer in transformers)
                foreach (var entry in transformer.ToSourceEntries(document))
                    yield return entry;
        }
    }
}
