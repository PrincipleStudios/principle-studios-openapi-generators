using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.Transformations;
using PrincipleStudios.OpenApi.TypeScript;
using System.Collections.Generic;
using System.Linq;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript
{
	public static class OperationTransformerFactory
	{

		public static ISourceProvider BuildTypeScriptOperationSourceProvider(this OpenApiDocument document, string versionInfo, TypeScriptSchemaOptions options)
		{
			ISourceProvider? result;
			var handlebarsFactory = new HandlebarsFactory(OperationHandlebarsTemplateProcess.CreateHandlebars);
			ISchemaSourceResolver<InlineDataType> schemaResolver = new TypeScriptSchemaSourceResolver(options, handlebarsFactory, versionInfo);
			var operationTransformer = new TypeScriptOperationTransformer(schemaResolver, document, options, versionInfo, handlebarsFactory);

			var operationsSourceProvider = new OperationSourceTransformer(document, operationTransformer);

			result = new CompositeOpenApiSourceProvider(
				operationsSourceProvider,
				new AllOperationsBarrelTransformer(operationsSourceProvider, operationTransformer),
				schemaResolver
			);
			return result;
		}

	}

	public class AllOperationsBarrelTransformer : ISourceProvider
	{
		private readonly OperationSourceTransformer operationsSourceProvider;
		private TypeScriptOperationTransformer operationTransformer;

		public AllOperationsBarrelTransformer(OperationSourceTransformer operationsSourceProvider, TypeScriptOperationTransformer operationTransformer)
		{
			this.operationsSourceProvider = operationsSourceProvider;
			this.operationTransformer = operationTransformer;
		}

		public IEnumerable<SourceEntry> GetSources(OpenApiTransformDiagnostic diagnostic)
		{
			yield return operationTransformer.TransformBarrelFileHelper(operationsSourceProvider.GetOperations(diagnostic).Select(op => op.operation), diagnostic);
		}
	}
}
