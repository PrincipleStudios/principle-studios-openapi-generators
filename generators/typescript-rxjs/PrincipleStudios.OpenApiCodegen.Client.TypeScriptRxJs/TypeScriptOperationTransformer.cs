using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.Transformations;
using PrincipleStudios.OpenApi.TypeScript;
using PrincipleStudios.OpenApi.TypeScript.templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScriptRxJs
{
    public class TypeScriptOperationTransformer : IOpenApiOperationTransformer
    {
        private readonly ISchemaSourceResolver<InlineDataType> typeScriptSchemaResolver;
        private readonly OpenApiDocument document;
        private readonly TypeScriptSchemaOptions options;
        private readonly string versionInfo;
        private readonly HandlebarsFactory handlebarsFactory;

        public TypeScriptOperationTransformer(ISchemaSourceResolver<InlineDataType> typeScriptSchemaResolver, OpenApiDocument document, TypeScriptSchemaOptions options, string versionInfo, HandlebarsFactory handlebarsFactory)
        {
            this.typeScriptSchemaResolver = typeScriptSchemaResolver;
            this.document = document;
            this.options = options;
            this.versionInfo = versionInfo;
            this.handlebarsFactory = handlebarsFactory;
        }

        public SourceEntry TransformOperation(OpenApiOperation operation, OpenApiContext context, OpenApiTransformDiagnostic diagnostic)
        {
            typeScriptSchemaResolver.EnsureSchemasRegistered(document, OpenApiContext.From(document), diagnostic);

            var operationName = TypeScriptNaming.ToMethodName(operation.OperationId, options.ReservedIdentifiers());

            var template = new templates.OperationTemplate(
                header: new PartialHeader(
                    appName: document.Info.Title,
                    appDescription: document.Info.Description,
                    version: document.Info.Version,
                    infoEmail: document.Info.Contact?.Email,
                    codeGeneratorVersionInfo: versionInfo
                ),

                operation: ToOperation(operation, context, diagnostic)
            );

            var entry = handlebarsFactory.Handlebars.ProcessOperation(template);
            return new SourceEntry
            {
                Key = $"operation/{operationName}.ts",
                SourceText = entry,
            };
        }

        private templates.Operation ToOperation(OpenApiOperation operation, OpenApiContext context, OpenApiTransformDiagnostic diagnostic)
        {
            var httpMethod = context.GetLastKeyFor(operation);
            if (httpMethod == null)
                throw new ArgumentException("Expected HTTP method from context", nameof(context));
            var path = context.Where(c => c.Element is OpenApiPathItem).Last().Key;
            if (path == null)
                throw new ArgumentException("Context is not initialized properly - key expected for path items", nameof(context));

            var builder = new OperationBuilderVisitor.OperationBuilder(operation);
            var visitor = new OperationBuilderVisitor(typeScriptSchemaResolver, options);
            visitor.Visit(operation, context, new OperationBuilderVisitor.Argument(diagnostic, builder));

            return visitor.ToOperationTemplate(operation, httpMethod.ToUpper(), path, builder);
        }

        //internal SourceEntry TransformBarrelFileHelper(IEnumerable<string> groups, OpenApiTransformDiagnostic diagnostic)
        //{
        //    return new SourceEntry
        //    {
        //        Key = $"index.ts",
        //        SourceText = handlebarsFactory.Handlebars.ProcessBarrelFile(new templates.AddServicesModel(
        //            header: new templates.PartialHeader(
        //                appName: document.Info.Title,
        //                appDescription: document.Info.Description,
        //                version: document.Info.Version,
        //                infoEmail: document.Info.Contact?.Email,
        //                codeGeneratorVersionInfo: versionInfo
        //            ),
        //            methodName: TypeScriptNaming.ToMethodName(document.Info.Title, options.ReservedIdentifiers()),
        //            packageName: baseNamespace,
        //            controllers: (from p in groups
        //                          let genericTypeName = TypeScriptNaming.ToClassName($"T {p}", options.ReservedIdentifiers())
        //                          let className = TypeScriptNaming.ToClassName(p + " base", options.ReservedIdentifiers())
        //                          select new templates.ControllerReference(genericTypeName, className)
        //                          ).ToArray()
        //        )),
        //    };
        //}

    }
}
