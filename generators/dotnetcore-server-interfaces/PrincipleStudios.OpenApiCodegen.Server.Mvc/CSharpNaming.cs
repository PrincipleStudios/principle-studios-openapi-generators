using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc
{
    public static class CSharpNaming
    {
        public static string ToClassName(string key) => ToTitleCaseIdentifier(key);
        public static string ToPropertyName(string key) => ToTitleCaseIdentifier(key);

        public static string ToTitleCaseIdentifier(string key) => ToIdentifier(ToTitleCase(key));
        public static string ToCamelCaseIdentifier(string key) => ToIdentifier(ToCamelCase(key));

        public static string ToMethodName(string key) => ToTitleCaseIdentifier(key);
        public static string ToParameterName(string key) => ToCamelCaseIdentifier(key);

        // TODO - check for reserved words... Probably not necessary when doing title case
        private static string ToIdentifier(string key) =>
            Regex.IsMatch(key, "^[a-zA-Z]") ? key : ("_" + key);

        private static string ToTitleCase(string key) =>
            string.Join("", Regex.Split(key, "[^a-zA-Z0-9]+")
                .Where(s => s is { Length: > 1 })
                .Select(s => s.ToUpper() == s
                    ? char.ToUpper(s[0]) + s.Substring(1).ToLower() // assume acronym, which gets lowercased, such as `HttpMethod` or `CorsPolicy`.
                    : char.ToUpper(s[0]) + s.Substring(1))
            );

        private static string ToCamelCase(string key) =>
            ToTitleCase(key) switch
            {
                string s => char.ToLower(s[0]) + s.Substring(1)
            };

        public static string? ToNamespace(string? rootNamespace, string? projectDir, string? identity, string? link)
        {
            var prefix = rootNamespace is { Length: > 0 } ? Enumerable.Repeat(rootNamespace, 1) : Enumerable.Empty<string>();
            
            if (link == null)
            {
                if (identity == null || projectDir == null || !identity.StartsWith(projectDir))
                    throw new InvalidOperationException($"No link provided and '{identity}' does not start with '{projectDir}'; unable to determine root namespace");
                link = identity.Substring(projectDir.Length);
            }

            var directoryParts = new Regex(@"[/\\]").Split(System.IO.Path.GetDirectoryName(link)).Where(t => t is { Length: > 0 });

            return string.Join(".", prefix.Concat(directoryParts.Select(ToTitleCaseIdentifier)));
        }
    }
}
