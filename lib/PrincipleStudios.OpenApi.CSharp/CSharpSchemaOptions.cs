using System;
using System.Collections.Generic;
using System.Linq;

namespace PrincipleStudios.OpenApi.CSharp
{
    public class CSharpSchemaOptions
    {
        public string ControllerNameExtension { get; set; } = "dotnet-mvc-server-controller";
        public List<string> GlobalReservedIdentifiers { get; } = new();
        public Dictionary<string, List<string>> ContextualReservedIdentifiers { get; } = new();
        public string MapType { get; set; } = "global::System.Collections.Generic.Dictionary<string, {}>";
        public string ArrayType { get; set; } = "global::System.Collections.Generic.IEnumerable<{}>";
        public string FallbackType { get; set; } = "object";
        public Dictionary<string, OpenApiTypeFormats> Types { get; } = new();

        internal string Find(string type, string? format)
        {
            if (!Types.TryGetValue(type, out var formats))
                return FallbackType;
            if (format == null || !formats.Formats.TryGetValue(format, out var result))
                return formats.Default;
            return result;
        }

        internal string ToArrayType(string type)
        {
            return ArrayType.Replace("{}", type);
        }

        internal string ToMapType(string type)
        {
            return MapType.Replace("{}", type);
        }

        public static System.IO.Stream GetDefaultOptionsJson() =>
            typeof(CSharpSchemaOptions).Assembly.GetManifestResourceStream($"{typeof(CSharpSchemaOptions).Namespace}.csharp.config.yaml");

        public IEnumerable<string> ReservedIdentifiers(string? scope = null, params string[] extraReserved) => 
            (
                scope is not null && ContextualReservedIdentifiers.TryGetValue(scope, out var scopedContextualIdentifiers) 
                    ? GlobalReservedIdentifiers.Concat(scopedContextualIdentifiers) 
                    : GlobalReservedIdentifiers
            ).Concat(
                extraReserved
            );
    }

    public class OpenApiTypeFormats
    {
        public string Default { get; set; } = "object";
        public Dictionary<string, string> Formats { get; } = new ();
    }
}