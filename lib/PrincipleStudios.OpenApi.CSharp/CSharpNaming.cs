using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApi.CSharp
{
    public static class CSharpNaming
    {
        public static string ToClassName(string key, ICollection<string> reservedIdentifiers) => ToTitleCaseIdentifier(key, reservedIdentifiers);
        public static string ToPropertyName(string key, ICollection<string> reservedIdentifiers) => ToTitleCaseIdentifier(key, reservedIdentifiers);

        public static string ToTitleCaseIdentifier(string key, ICollection<string> reservedIdentifiers) => ToIdentifier(ToTitleCase(key), reservedIdentifiers);
        public static string ToCamelCaseIdentifier(string key, ICollection<string> reservedIdentifiers) => ToIdentifier(ToCamelCase(key), reservedIdentifiers);

        public static string ToMethodName(string key, ICollection<string> reservedIdentifiers) => ToTitleCaseIdentifier(key, reservedIdentifiers);
        public static string ToParameterName(string key, ICollection<string> reservedIdentifiers) => ToCamelCaseIdentifier(key, reservedIdentifiers);

        // TODO - check for reserved words... Probably not necessary when doing title case
        private static string ToIdentifier(string key, ICollection<string> reservedIdentifiers) =>
            Regex.IsMatch(key, "^[a-zA-Z]") && !reservedIdentifiers.Contains(key) ? key : ("_" + key);

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

        public static string? ToNamespace(string? rootNamespace, string? projectDir, string? identity, string? link, ICollection<string> reservedIdentifiers)
        {
            var prefix = rootNamespace is { Length: > 0 } ? Enumerable.Repeat(rootNamespace, 1) : Enumerable.Empty<string>();
            
            if (link == null)
            {
                if (identity == null || projectDir == null || !identity.StartsWith(projectDir))
                    throw new InvalidOperationException($"No link provided and '{identity}' does not start with '{projectDir}'; unable to determine root namespace");
                link = identity.Substring(projectDir.Length);
            }

            var directoryParts = new Regex(@"[/\\]").Split(System.IO.Path.GetDirectoryName(link)).Where(t => t is { Length: > 0 });

            return string.Join(".", prefix.Concat(directoryParts.Select(v => ToTitleCaseIdentifier(v, reservedIdentifiers))));
        }
    }
}
