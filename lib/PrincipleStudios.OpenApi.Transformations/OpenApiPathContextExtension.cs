using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations
{
    public static class OpenApiPathContextExtension
    {
        public static string ToOpenApiPathContext(this string original) => original.Replace("/", "~1");
        public static string ToOpenApiPathContextString(this OpenApiContext original) => 
            "#/" + string.Join("/", original.SelectMany(p => new[] { p.Property, p.Key }).Where(p => p is not null).Select(p => p?.Replace("/", "~1")));
    }
}
