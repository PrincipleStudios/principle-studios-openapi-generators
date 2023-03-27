using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.Transformations;
using System.Linq;

namespace PrincipleStudios.OpenApi.CSharp
{
    public static class PathControllerTransformerFactory
    {
        public static ISourceProvider BuildCSharpPathControllerSourceProvider(this OpenApiDocument document, string versionInfo, string? documentNamespace, CSharpServerSchemaOptions options)
        {
            ISourceProvider? result;
            var handlebarsFactory = new HandlebarsFactory(ControllerHandlebarsTemplateProcess.CreateHandlebars);
            ISchemaSourceResolver<InlineDataType> schemaResolver = new CSharpSchemaSourceResolver(documentNamespace ?? "", options, handlebarsFactory, versionInfo);
            var controllerTransformer = new CSharpControllerTransformer(schemaResolver, document, documentNamespace ?? "", options, versionInfo, handlebarsFactory);

            var operationGrouping =
                new PathControllerSourceTransformer(document, controllerTransformer, (operation, context) =>
                {
                    var extensionValue = context.Reverse()
                        .Select(ctx => ctx.Element is not Microsoft.OpenApi.Interfaces.IOpenApiExtensible extensible ? null
                            : !extensible.Extensions.TryGetValue($"x-{options.ControllerNameExtension}", out var value) ? null
                            : value is not Microsoft.OpenApi.Any.OpenApiString s ? null
                            : s.Value)
                        .Where(value => value != null)
                        .FirstOrDefault();
                    return extensionValue;
                });

            result = new CompositeOpenApiSourceProvider(
                operationGrouping,
                new DotNetMvcAddServicesHelperTransformer(controllerTransformer, operationGrouping),
                schemaResolver
            );
            return result;
        }

    }
}
