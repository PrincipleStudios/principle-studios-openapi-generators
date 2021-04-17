using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApi.NetCore.ServerInterfaces
{
    public static class CSharpNaming
    {
        public static string ToClassName(string key) => ToTitleCaseIdentifier(key);
        public static string ToPropertyName(string key) => ToTitleCaseIdentifier(key);

        public static string ToTitleCaseIdentifier(string key) => ToIdentifier(ToTitleCase(key));

        // TODO - check for reserved words? Probably not necessary when doing title case
        private static string ToIdentifier(string key) =>
            Regex.IsMatch(key, "^[a-zA-Z]") ? key : ("_" + key);

        private static string ToTitleCase(string key) =>
            string.Join("", Regex.Split(key, "[^a-zA-Z0-9]+")
                .Where(s => s is { Length: > 1 })
                .Select(s => s.ToUpper() == s
                    ? char.ToUpper(s[0]) + s[1..].ToLower() // assume acronym, which gets lowercased, such as `HttpMethod` or `CorsPolicy`.
                    : char.ToUpper(s[0]) + s[1..])
            );

    }
}
