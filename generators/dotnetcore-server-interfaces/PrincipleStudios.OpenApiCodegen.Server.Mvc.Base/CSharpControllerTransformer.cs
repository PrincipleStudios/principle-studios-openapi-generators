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

namespace PrincipleStudios.OpenApi.CSharp
{
    public class CSharpControllerTransformer : IOpenApiOperationControllerTransformer
    {
        private readonly ISchemaSourceResolver<InlineDataType> csharpSchemaResolver;
        private readonly OpenApiDocument document;
        private readonly string baseNamespace;
        private readonly CSharpServerSchemaOptions options;
        private readonly string versionInfo;
        private readonly HandlebarsFactory handlebarsFactory;

        public CSharpControllerTransformer(ISchemaSourceResolver<InlineDataType> csharpSchemaResolver, OpenApiDocument document, string baseNamespace, CSharpServerSchemaOptions options, string versionInfo, HandlebarsFactory handlebarsFactory)
        {
            this.csharpSchemaResolver = csharpSchemaResolver;
            this.document = document;
            this.baseNamespace = baseNamespace;
            this.options = options;
            this.versionInfo = versionInfo;
            this.handlebarsFactory = handlebarsFactory;
        }

        public SourceEntry TransformController(string groupName, OperationGroupData groupData, OpenApiTransformDiagnostic diagnostic)
        {
            var (summary, description, operations) = groupData;
            csharpSchemaResolver.EnsureSchemasRegistered(document, OpenApiContext.From(document), diagnostic);

            var className = CSharpNaming.ToClassName(groupName + " base", options.ReservedIdentifiers());

            var resultOperations = new List<ControllerOperation>();
            var visitor = new ControllerOperationVisitor(csharpSchemaResolver, options, controllerClassName: className);
            foreach (var (operation, context) in operations)
                visitor.Visit(operation, context, new ControllerOperationVisitor.Argument(diagnostic, resultOperations.Add));

            var template = new Templates.ControllerTemplate(
                header: new Templates.PartialHeader(
                    appName: document.Info.Title,
                    appDescription: document.Info.Description,
                    version: document.Info.Version,
                    infoEmail: document.Info.Contact?.Email,
                    codeGeneratorVersionInfo: versionInfo
                ),

                packageName: baseNamespace,
                className: className,
                hasDescriptionOrSummary: (summary?.Trim() + description?.Trim()) is { Length: > 0 },
                summary: summary,
                description: description,

                operations: resultOperations.ToArray()
            );

            var entry = handlebarsFactory.Handlebars.ProcessController(template);
            return new SourceEntry
            {
                Key = $"{baseNamespace}.{className}.cs",
                SourceText = entry,
            };
        }

        public string SanitizeGroupName(string groupName)
        {
            return CSharpNaming.ToClassName(groupName + " controller", options.ReservedIdentifiers());
        }

        internal SourceEntry TransformAddServicesHelper(IEnumerable<string> groups, OpenApiTransformDiagnostic diagnostic)
        {
            return new SourceEntry
            {
                Key = $"{baseNamespace}.AddServicesExtensions.cs",
                SourceText = handlebarsFactory.Handlebars.ProcessAddServices(new Templates.AddServicesModel(
                    header: new Templates.PartialHeader(
                        appName: document.Info.Title,
                        appDescription: document.Info.Description,
                        version: document.Info.Version,
                        infoEmail: document.Info.Contact?.Email,
                        codeGeneratorVersionInfo: versionInfo
                    ),
                    methodName: CSharpNaming.ToMethodName(document.Info.Title, options.ReservedIdentifiers()),
                    packageName: baseNamespace,
                    controllers: (from p in groups
                                  let genericTypeName = CSharpNaming.ToClassName($"T {p}", options.ReservedIdentifiers())
                                  let className = CSharpNaming.ToClassName(p + " base", options.ReservedIdentifiers())
                                  select new Templates.ControllerReference(genericTypeName, className)
                                  ).ToArray()
                )),
            };
        }
    }
}
