using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.CSharp.Templates;
using PrincipleStudios.OpenApi.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrincipleStudios.OpenApi.CSharp;
using Microsoft.OpenApi.Any;
using PrincipleStudios.OpenApiCodegen;

namespace PrincipleStudios.OpenApi.CSharp
{
	public class CSharpClientTransformer : ISourceProvider
	{
		private readonly ISchemaSourceResolver<InlineDataType> csharpSchemaResolver;
		private readonly OpenApiDocument document;
		private readonly string baseNamespace;
		private readonly CSharpSchemaOptions options;
		private readonly string versionInfo;
		private readonly HandlebarsFactory handlebarsFactory;

		public CSharpClientTransformer(ISchemaSourceResolver<InlineDataType> csharpSchemaResolver, OpenApiDocument document, string baseNamespace, CSharpSchemaOptions options, string versionInfo, HandlebarsFactory handlebarsFactory)
		{
			this.csharpSchemaResolver = csharpSchemaResolver;
			this.document = document;
			this.baseNamespace = baseNamespace;
			this.options = options;
			this.versionInfo = versionInfo;
			this.handlebarsFactory = handlebarsFactory;
		}

		public SourceEntry TransformOperations(OpenApiTransformDiagnostic diagnostic)
		{
			csharpSchemaResolver.EnsureSchemasRegistered(document, OpenApiContext.From(document), diagnostic);

			var className = CSharpNaming.ToClassName("operations", options.ReservedIdentifiers());

			var resultOperations = new List<Operation>();
			var visitor = new OperationVisitor(csharpSchemaResolver, options, controllerClassName: className);
			visitor.Visit(document, OpenApiContext.From(document), new OperationVisitor.Argument(diagnostic, resultOperations.Add));

			resultOperations = (from operation in resultOperations
								select operation with { Path = operation.Path.Substring(1) }).ToList();

			var template = new Templates.FullTemplate(
				Header: new Templates.PartialHeader(
					AppName: document.Info.Title,
					AppDescription: document.Info.Description,
					Version: document.Info.Version,
					InfoEmail: document.Info.Contact?.Email,
					CodeGeneratorVersionInfo: versionInfo
				),

				PackageName: baseNamespace,
				ClassName: className,

				Operations: resultOperations.ToArray()
			);

			var entry = handlebarsFactory.Handlebars.ProcessController(template);
			return new SourceEntry(
				Key: $"{baseNamespace}.{className}.cs",
				SourceText: entry
			);
		}

		public string SanitizeGroupName(string groupName)
		{
			return CSharpNaming.ToClassName(groupName + " client", options.ReservedIdentifiers());
		}

		internal SourceEntry TransformAddServicesHelper(OpenApiTransformDiagnostic diagnostic)
		{
			return new SourceEntry(
				Key: $"{baseNamespace}.AddServicesExtensions.cs",
				SourceText: handlebarsFactory.Handlebars.ProcessAddServices(new Templates.AddServicesModel(
					Header: new Templates.PartialHeader(
						AppName: document.Info.Title,
						AppDescription: document.Info.Description,
						Version: document.Info.Version,
						InfoEmail: document.Info.Contact?.Email,
						CodeGeneratorVersionInfo: versionInfo
					),
					MethodName: CSharpNaming.ToMethodName(document.Info.Title, options.ReservedIdentifiers()),
					PackageName: baseNamespace
				))
			);
		}

		public IEnumerable<SourceEntry> GetSources(OpenApiTransformDiagnostic diagnostic)
		{
			yield return TransformOperations(diagnostic);
			//yield return TransformAddServicesHelper(diagnostic);

			foreach (var source in csharpSchemaResolver.GetSources(diagnostic))
				yield return source;
		}
	}
}
