using Microsoft.OpenApi.Any;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApi.TypeScript
{
    public static class PrimitiveToJsonValue
    {
        public static string GetPrimitiveValue(IOpenApiAny any)
        {
            switch (any)
            {
                case OpenApiNull _: return Newtonsoft.Json.JsonConvert.SerializeObject(null);
                case OpenApiBinary b: return Newtonsoft.Json.JsonConvert.SerializeObject(b.Value);
                case OpenApiBoolean b: return Newtonsoft.Json.JsonConvert.SerializeObject(b.Value);
                case OpenApiByte b: return Newtonsoft.Json.JsonConvert.SerializeObject(b.Value);
                case OpenApiDate b: return Newtonsoft.Json.JsonConvert.SerializeObject(b.Value);
                case OpenApiDateTime b: return Newtonsoft.Json.JsonConvert.SerializeObject(b.Value);
                case OpenApiDouble b: return Newtonsoft.Json.JsonConvert.SerializeObject(b.Value);
                case OpenApiFloat b: return Newtonsoft.Json.JsonConvert.SerializeObject(b.Value);
                case OpenApiInteger b: return Newtonsoft.Json.JsonConvert.SerializeObject(b.Value);
                case OpenApiLong b: return Newtonsoft.Json.JsonConvert.SerializeObject(b.Value);
                case OpenApiPassword b: return Newtonsoft.Json.JsonConvert.SerializeObject(b.Value);
                case OpenApiString b: return Newtonsoft.Json.JsonConvert.SerializeObject(b.Value);
                default: throw new NotImplementedException("Unsupported type for enum: " + any.AnyType.ToString("g"));
            };
        }
    }
}
