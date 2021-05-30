using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.Transformations;

namespace PrincipleStudios.OpenApi.CSharp
{
    public static class PathControllerTransformerFactory
    {
        public static ISourceProvider BuildCSharpPathControllerSourceProvider(this OpenApiDocument document, string versionInfo, string? documentNamespace, CSharpSchemaOptions options)
        {
            ISourceProvider? result;
            var handlebarsFactory = new HandlebarsFactory(ControllerHandlebarsTemplateProcess.CreateHandlebars);
            ISchemaSourceResolver<InlineDataType> schemaResolver = new CSharpSchemaSourceResolver(documentNamespace ?? "", options, handlebarsFactory, versionInfo);
            var controllerTransformer = new CSharpPathControllerTransformer(schemaResolver, document, documentNamespace ?? "", options, versionInfo, handlebarsFactory);

            result = new CompositeOpenApiSourceProvider(
                new PathControllerSourceTransformer(document, controllerTransformer),
                new DotNetMvcAddServicesHelperTransformer(document, controllerTransformer),
                schemaResolver
            );
            return result;
        }

    }
}
