using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.Transformations;
using System.Linq;

namespace PrincipleStudios.OpenApi.CSharp
{
	public static class ClientTransformerFactory
	{
		public static ISourceProvider BuildCSharpClientSourceProvider(this OpenApiDocument document, string versionInfo, string? documentNamespace, CSharpSchemaOptions options)
		{
			ISourceProvider? result;
			var handlebarsFactory = new HandlebarsFactory(ControllerHandlebarsTemplateProcess.CreateHandlebars);
			ISchemaSourceResolver<InlineDataType> schemaResolver = new CSharpSchemaSourceResolver(documentNamespace ?? "", options, handlebarsFactory, versionInfo);
			var controllerTransformer = new CSharpClientTransformer(schemaResolver, document, documentNamespace ?? "", options, versionInfo, handlebarsFactory);

			result = new CompositeOpenApiSourceProvider(
				controllerTransformer
			);
			return result;
		}

	}
}
