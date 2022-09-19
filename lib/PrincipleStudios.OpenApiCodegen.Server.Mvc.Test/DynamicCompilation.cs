using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc
{
    internal class DynamicCompilation
    {
        public static readonly string[] NewtonsoftCompilationRefPaths = {
            Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location)!, "netstandard.dll"),
            Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location)!, "System.Runtime.dll"),
            typeof(System.AttributeUsageAttribute).Assembly.Location,
            typeof(System.Linq.Enumerable).Assembly.Location,
            typeof(System.ComponentModel.TypeConverter).Assembly.Location,
            typeof(System.ComponentModel.TypeConverterAttribute).Assembly.Location,
            typeof(System.ComponentModel.DataAnnotations.RequiredAttribute).Assembly.Location,
            typeof(System.Runtime.Serialization.EnumMemberAttribute).Assembly.Location,

            typeof(Microsoft.Extensions.DependencyInjection.IServiceCollection).Assembly.Location,
            typeof(Microsoft.Extensions.DependencyInjection.IMvcBuilder).Assembly.Location,
            typeof(Microsoft.Extensions.DependencyInjection.MvcServiceCollectionExtensions).Assembly.Location,
            typeof(Microsoft.AspNetCore.Mvc.NewtonsoftJson.JsonSerializerSettingsProvider).Assembly.Location,

            typeof(Microsoft.Net.Http.Headers.MediaTypeHeaderValue).Assembly.Location,
            typeof(Microsoft.Extensions.Primitives.StringSegment).Assembly.Location,
            typeof(Microsoft.AspNetCore.Mvc.IActionResult).Assembly.Location,
            typeof(Microsoft.AspNetCore.Mvc.ObjectResult).Assembly.Location,
            typeof(Microsoft.AspNetCore.Http.HttpContext).Assembly.Location,
            typeof(Microsoft.AspNetCore.Http.IHeaderDictionary).Assembly.Location,
            typeof(Newtonsoft.Json.JsonConvert).Assembly.Location,
            typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute).Assembly.Location,
        };
    }
}
