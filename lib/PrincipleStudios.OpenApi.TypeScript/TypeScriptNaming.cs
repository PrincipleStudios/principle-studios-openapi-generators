using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PrincipleStudios.OpenApi.TypeScript
{
	public static class TypeScriptNaming
	{
		public static string ToClassName(string key, IEnumerable<string> reservedIdentifiers) => ToTitleCaseIdentifier(key, reservedIdentifiers);
		public static string ToPropertyName(string key, IEnumerable<string> reservedIdentifiers) => ToCamelCaseIdentifier(key, reservedIdentifiers);

		public static string ToTitleCaseIdentifier(string key, IEnumerable<string> reservedIdentifiers) => ToIdentifier(ToTitleCase(key), reservedIdentifiers);
		public static string ToCamelCaseIdentifier(string key, IEnumerable<string> reservedIdentifiers) => ToIdentifier(ToCamelCase(key), reservedIdentifiers);

		public static string ToMethodName(string key, IEnumerable<string> reservedIdentifiers) => ToCamelCaseIdentifier(key, reservedIdentifiers);
		public static string ToParameterName(string key, IEnumerable<string> reservedIdentifiers) => ToCamelCaseIdentifier(key, reservedIdentifiers);

		private static string ToIdentifier(string key, IEnumerable<string> reservedIdentifiers) =>
			Regex.IsMatch(key, "^[a-zA-Z]") && !reservedIdentifiers.Contains(key) ? key : ("_" + key);

		private static string ToTitleCase(string key) =>
			string.Join("", Regex.Split(key, "[^a-zA-Z0-9]+")
				.Where(s => s is { Length: > 0 })
				.Select(s => s.ToUpper() == s
					? char.ToUpper(s[0]) + s.Substring(1).ToLower() // assume acronym, which gets lowercased, such as `HttpMethod` or `CorsPolicy`.
					: char.ToUpper(s[0]) + s.Substring(1))
			);

		private static string ToCamelCase(string key) =>
			ToTitleCase(key) switch
			{
				string s => char.ToLower(s[0]) + s.Substring(1)
			};
	}
}
