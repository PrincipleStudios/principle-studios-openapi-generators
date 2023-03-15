using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations;

public static class SharedOpenApiExtensions
{
    public static bool UseOptionalAsNullable(this OpenApiSchema objectSchema, bool useLegacyByDefault = false)
    {
        if (!objectSchema.Extensions.TryGetValue("x-PS-Optional-As-Nullable", out var optionalAsNullable) || optionalAsNullable is not Microsoft.OpenApi.Any.OpenApiBoolean { Value: var result })
            return useLegacyByDefault;

        return result;
    }
}
