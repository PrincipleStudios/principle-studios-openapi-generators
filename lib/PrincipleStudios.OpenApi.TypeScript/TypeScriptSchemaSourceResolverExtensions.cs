using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrincipleStudios.OpenApi.TypeScript
{
    public static class TypeScriptSchemaSourceResolverExtensions
    {

        public static IEnumerable<templates.ImportStatement> GetImportStatements(this ISchemaSourceResolver<InlineDataType> sourceResolver, IEnumerable<OpenApiSchema> schemasReferenced, string path)
        {
            return from entry in schemasReferenced
                   let t = sourceResolver.ToInlineDataType(entry)()
                   from import in t.Imports
                   let refName = import.Member
                   let fileName = import.File
                   group refName by fileName into imports
                   let nodePath = ToNodePath(imports.Key, path)
                   orderby nodePath
                   select new templates.ImportStatement(imports.Distinct().OrderBy(a => a).ToArray(), nodePath);
        }

        public static string ToNodePath(string path, string fromPath)
        {
            if (path.StartsWith("..")) throw new ArgumentException("Cannot start with ..", nameof(path));
            if (fromPath.StartsWith("..")) throw new ArgumentException("Cannot start with ..", nameof(fromPath));
            path = Normalize(path);
            fromPath = Normalize(fromPath);
            var pathParts = path.Split('/');
            pathParts[pathParts.Length - 1] = System.IO.Path.GetFileNameWithoutExtension(pathParts[pathParts.Length - 1]);
            var fromPathParts = System.IO.Path.GetDirectoryName(fromPath).Split('/');
            var ignored = pathParts.TakeWhile((p, i) => i < fromPathParts.Length && p == fromPathParts[i]).Count();
            pathParts = pathParts.Skip(ignored).ToArray();
            fromPathParts = fromPathParts.Skip(ignored).ToArray();
            return string.Join("/", Enumerable.Repeat(".", 1).Concat(Enumerable.Repeat("..", fromPathParts.Length).Concat(pathParts)));

            string Normalize(string p)
            {
                p = p.Replace('\\', '/');
                if (p.StartsWith("./")) p = p.Substring(2);
                return p;
            }
        }
    }
}
