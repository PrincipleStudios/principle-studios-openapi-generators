using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrincipleStudios.OpenApi.TypeScript
{
    public class TypeScriptSchemaOptions
    {
        public List<string> AllowedMimeTypes { get; set; } = new();
        public List<string> GlobalReservedIdentifiers { get; set; } = new();
        public Dictionary<string, List<string>> ContextualReservedIdentifiers { get; set; } = new();
        public string MapType { get; set; } = "Record<string, {}>";
        public string ArrayType { get; set; } = "Array<{}>";
        public string FallbackType { get; set; } = "any";
        public Dictionary<string, OpenApiTypeFormats> Types { get; set; } = new();

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
            typeof(TypeScriptSchemaOptions).Assembly.GetManifestResourceStream($"{typeof(TypeScriptSchemaOptions).Namespace}.typescript.config.yaml")!;

        public IEnumerable<string> ReservedIdentifiers(string? scope = null, params string[] extraReserved) =>
            (
                scope is not null && ContextualReservedIdentifiers.ContainsKey(scope)
                    ? GlobalReservedIdentifiers.Concat(ContextualReservedIdentifiers[scope])
                    : GlobalReservedIdentifiers
            ).Concat(
                extraReserved
            );
    }

    public class OpenApiTypeFormats
    {
        public string Default { get; set; } = "object";
        public Dictionary<string, string> Formats { get; set; } = new();
    }
}
