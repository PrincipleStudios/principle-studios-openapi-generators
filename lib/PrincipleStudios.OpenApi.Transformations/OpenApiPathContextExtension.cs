using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations
{
    public static class OpenApiPathContextExtension
    {
        public static string ToOpenApiPathContext(this string original) => original.Replace("/", "~1");
    }
}
