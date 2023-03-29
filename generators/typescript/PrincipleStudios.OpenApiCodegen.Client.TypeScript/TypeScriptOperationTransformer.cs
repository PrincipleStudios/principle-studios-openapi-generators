using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.Transformations;
using PrincipleStudios.OpenApi.TypeScript;
using PrincipleStudios.OpenApi.TypeScript.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript
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

        public string OperationFileName(OpenApiOperation operation)
        {
            var operationName = TypeScriptNaming.ToMethodName(operation.OperationId, options.ReservedIdentifiers());
            return $"operations/{operationName}.ts";
        }

        public SourceEntry TransformOperation(OpenApiOperation operation, OpenApiContext context, OpenApiTransformDiagnostic diagnostic)
        {
            typeScriptSchemaResolver.EnsureSchemasRegistered(document, OpenApiContext.From(document), diagnostic);


            var template = new Templates.OperationTemplate(
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
                Key = OperationFileName(operation),
                SourceText = entry,
            };
        }

        private Templates.Operation ToOperation(OpenApiOperation operation, OpenApiContext context, OpenApiTransformDiagnostic diagnostic)
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

        internal SourceEntry TransformBarrelFileHelper(IEnumerable<OpenApiOperation> operations, OpenApiTransformDiagnostic diagnostic)
        {
            var thisPath = $"operations/index.ts";
            return new SourceEntry
            {
                Key = thisPath,
                SourceText = handlebarsFactory.Handlebars.ProcessBarrelFile(new Templates.OperationBarrelFileModel(
                    header: new PartialHeader(
                        appName: document.Info.Title,
                        appDescription: document.Info.Description,
                        version: document.Info.Version,
                        infoEmail: document.Info.Contact?.Email,
                        codeGeneratorVersionInfo: versionInfo
                    ),
                    operations: (from op in operations
                                 select new Templates.OperationReference(OperationFileName(op).ToNodePath(thisPath), TypeScriptNaming.ToMethodName(op.OperationId, options.ReservedIdentifiers()))
                                 ).ToArray()
                )),
            };
        }

    }
}
