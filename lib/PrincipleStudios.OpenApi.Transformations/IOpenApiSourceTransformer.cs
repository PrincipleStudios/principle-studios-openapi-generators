using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations
{
    public interface IOpenApiSourceTransformer
    {
        IEnumerable<SourceEntry> ToSourceEntries(OpenApiDocument document);
    }
}
