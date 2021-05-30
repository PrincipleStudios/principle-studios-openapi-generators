using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.CSharp.templates;
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
    public class CSharpPathControllerTransformer : IOpenApiPathControllerTransformer
    {
        private readonly ISchemaSourceResolver<InlineDataType> csharpSchemaResolver;
        private readonly OpenApiDocument document;
        private readonly string baseNamespace;
        private readonly CSharpSchemaOptions options;
        private readonly string versionInfo;
        private readonly HandlebarsFactory handlebarsFactory;

        public CSharpPathControllerTransformer(ISchemaSourceResolver<InlineDataType> csharpSchemaResolver, OpenApiDocument document, string baseNamespace, CSharpSchemaOptions options, string versionInfo, HandlebarsFactory handlebarsFactory)
        {
            this.csharpSchemaResolver = csharpSchemaResolver;
            this.document = document;
            this.baseNamespace = baseNamespace;
            this.options = options;
            this.versionInfo = versionInfo;
            this.handlebarsFactory = handlebarsFactory;
        }

        public SourceEntry TransformController(OpenApiPathItem pathItem, OpenApiContext context, OpenApiTransformDiagnostic diagnostic)
        {
            csharpSchemaResolver.EnsureSchemasRegistered(document, OpenApiContext.From(document), diagnostic);

            var path = context.GetLastKeyFor(pathItem);

            var className = CSharpNaming.ToClassName(path + " controller base", options.ReservedIdentifiers);

            var operations = new List<ControllerOperation>();
            var visitor = new ControllerOperationVisitor(csharpSchemaResolver, options);
            visitor.Visit(pathItem, context, new ControllerOperationVisitor.Argument(path, diagnostic, operations.Add));

            var template = new templates.ControllerTemplate(
                header: new templates.PartialHeader(
                    appName: document.Info.Title,
                    appDescription: document.Info.Description,
                    version: document.Info.Version,
                    infoEmail: document.Info.Contact?.Email,
                    codeGeneratorVersionInfo: versionInfo
                ),

                packageName: baseNamespace,
                className: className,
                hasDescriptionOrSummary: (pathItem.Summary?.Trim() + pathItem.Description?.Trim()) is { Length: > 0 },
                summary: pathItem.Summary,
                description: pathItem.Description,

                operations: operations.ToArray()
            );

            var entry = handlebarsFactory.Handlebars.ProcessController(template);
            return new SourceEntry
            {
                Key = $"{baseNamespace}.{className}.cs",
                SourceText = entry,
            };
        }

        internal SourceEntry TransformAddServicesHelper(OpenApiPaths paths, OpenApiTransformDiagnostic diagnostic)
        {
            return new SourceEntry
            {
                Key = $"{baseNamespace}.AddServicesExtensions.cs",
                SourceText = handlebarsFactory.Handlebars.ProcessAddServices(new templates.AddServicesModel(
                    header: new templates.PartialHeader(
                        appName: document.Info.Title,
                        appDescription: document.Info.Description,
                        version: document.Info.Version,
                        infoEmail: document.Info.Contact?.Email,
                        codeGeneratorVersionInfo: versionInfo
                    ),
                    methodName: CSharpNaming.ToMethodName(document.Info.Title, options.ReservedIdentifiers),
                    packageName: baseNamespace,
                    controllers: (from p in paths
                                  let genericTypeName = CSharpNaming.ToClassName($"T {p.Key} controller", options.ReservedIdentifiers)
                                  let className = CSharpNaming.ToClassName(p.Key + " controller base", options.ReservedIdentifiers)
                                  select new templates.ControllerReference(genericTypeName, className)
                                  ).ToArray()
                )),
            };
        }

        class ControllerOperationVisitor : OpenApiDocumentVisitor<ControllerOperationVisitor.Argument>
        {
            private readonly ISchemaSourceResolver<InlineDataType> csharpSchemaResolver;
            private readonly CSharpSchemaOptions options;

            public record Argument(string Path, OpenApiTransformDiagnostic Diagnostic, RegisterControllerOperation RegisterControllerOperation, OperationBuilder? Builder = null);
            public delegate void RegisterControllerOperation(templates.ControllerOperation operation);

            public class OperationBuilder
            {
                public OperationBuilder(OpenApiOperation operation)
                {
                    Operation = operation;
                }

                public List<Func<OperationParameter[], OperationRequestBody>> RequestBodies { get; } = new();

                public OperationResponse? DefaultResponse { get; set; }
                public Dictionary<int, OperationResponse> StatusResponses { get; } = new();
                public List<OperationSecurityRequirement> SecurityRequirements { get; } = new();
                public List<OperationParameter> SharedParameters { get; } = new();
                public OpenApiOperation Operation { get; }
            }

            public ControllerOperationVisitor(ISchemaSourceResolver<InlineDataType> csharpSchemaResolver, CSharpSchemaOptions options)
            {
                this.csharpSchemaResolver = csharpSchemaResolver;
                this.options = options;
            }

            public override void Visit(OpenApiOperation operation, OpenApiContext context, Argument argument)
            {
                var httpMethod = context.GetLastKeyFor(operation);

                var builder = new OperationBuilder(operation);

                base.Visit(operation, context, argument with { Builder = builder });

                var sharedParameters = builder.SharedParameters.ToArray();
                argument.RegisterControllerOperation(
                    new templates.ControllerOperation(
                     httpMethod: httpMethod,
                     summary: operation.Summary,
                     description: operation.Description,
                     name: CSharpNaming.ToTitleCaseIdentifier(operation.OperationId, options.ReservedIdentifiers),
                     path: argument.Path,
                     requestBodies: builder.RequestBodies.DefaultIfEmpty(OperationRequestBodyFactory(operation.OperationId, null, Enumerable.Empty<OperationParameter>())).Select(transform => transform(sharedParameters)).ToArray(),
                     responses: new templates.OperationResponses(
                         defaultResponse: builder.DefaultResponse,
                         statusResponse: new(builder.StatusResponses)
                     ),
                     securityRequirements: builder.SecurityRequirements.ToArray()
                 ));
            }

            public override void Visit(OpenApiParameter param, OpenApiContext context, Argument argument)
            {
                var dataTypeBase = csharpSchemaResolver.ToInlineDataType(param.Schema, context.Append(nameof(param.Schema), null, param.Schema), argument.Diagnostic);
                var dataType = param.Required ? dataTypeBase : dataTypeBase.MakeNullable();
                argument.Builder?.SharedParameters.Add(new templates.OperationParameter(
                    rawName: param.Name,
                    paramName: CSharpNaming.ToParameterName(param.Name, options.ReservedIdentifiers),
                    description: param.Description,
                    dataType: dataType.text,
                    dataTypeNullable: dataType.nullable,
                    isPathParam: param.In == ParameterLocation.Path,
                    isQueryParam: param.In == ParameterLocation.Query,
                    isHeaderParam: param.In == ParameterLocation.Header,
                    isCookieParam: param.In == ParameterLocation.Cookie,
                    isBodyParam: false,
                    isFormParam: false,
                    required: param.Required,
                    pattern: param.Schema.Pattern,
                    minLength: param.Schema.MinLength,
                    maxLength: param.Schema.MaxLength,
                    minimum: param.Schema.Minimum,
                    maximum: param.Schema.Maximum
                ));
            }
            public override void Visit(OpenApiResponse response, OpenApiContext context, Argument argument)
            {
                var responseKey = context.GetLastKeyFor(response);
                int? statusCode = int.TryParse(responseKey, out var s) ? s : null;

                var result = new OperationResponse(
                    description: response.Description,
                    content: (from entry in response.Content.DefaultIfEmpty(new("", new OpenApiMediaType()))
                              let entryContext = context.Append(nameof(response.Content), entry.Key, entry.Value)
                              let dataType = entry.Value.Schema != null ? csharpSchemaResolver.ToInlineDataType(entry.Value.Schema, entryContext.Append(nameof(entry.Value.Schema), null, entry.Value.Schema), argument.Diagnostic) : null
                              select new OperationResponseContentOption(
                                  mediaType: entry.Key,
                                  responseMethodName: CSharpNaming.ToTitleCaseIdentifier($"{(response.Content.Count > 1 ? entry.Key : "")} {(statusCode == null ? "other status code" : $"status code {statusCode}")}", options.ReservedIdentifiers),
                                  dataType: dataType?.text
                              )).ToArray(),
                    headers: (from entry in response.Headers
                              let entryContext = context.Append(nameof(response.Headers), entry.Key, entry.Value)
                              let required = entry.Value.Required
                              let dataTypeBase = csharpSchemaResolver.ToInlineDataType(entry.Value.Schema, entryContext.Append(nameof(entry.Value.Schema), null, entry.Value.Schema), argument.Diagnostic)
                              let dataType = required ? dataTypeBase : dataTypeBase.MakeNullable()
                              select new templates.OperationResponseHeader(
                                  rawName: entry.Key,
                                  paramName: CSharpNaming.ToParameterName("header " + entry.Key, options.ReservedIdentifiers),
                                  description: entry.Value.Description,
                                  dataType: dataType.text,
                                  dataTypeNullable: dataType.nullable,
                                  required: entry.Value.Required,
                                  pattern: entry.Value.Schema.Pattern,
                                  minLength: entry.Value.Schema.MinLength,
                                  maxLength: entry.Value.Schema.MaxLength,
                                  minimum: entry.Value.Schema.Minimum,
                                  maximum: entry.Value.Schema.Maximum
                              )).ToArray()
                );

                if (statusCode.HasValue)
                    argument.Builder?.StatusResponses.Add(statusCode.Value, result);
                else if (responseKey == "default" && argument.Builder != null)
                    argument.Builder.DefaultResponse = result;
                else
                    argument.Diagnostic.Errors.Add(new OpenApiTransformError(context, $"Unknown response status: {responseKey}"));
            }
            //public override void Visit(OpenApiRequestBody requestBody, OpenApiContext context, Argument argument)
            //{
            //}
            public override void Visit(OpenApiMediaType mediaType, OpenApiContext context, Argument argument)
            {
                // All media type visitations should be for request bodies
                var mimeType = context.GetLastKeyFor(mediaType);

                var isForm = mimeType == "application/x-www-form-urlencoded";

                var singleContentType = argument.Builder?.Operation.RequestBody.Content.Count <= 1;

                argument.Builder?.RequestBodies.Add(OperationRequestBodyFactory(argument.Builder?.Operation.OperationId + (singleContentType ? "" : mimeType), mimeType, isForm ? GetFormParams() : GetStandardParams()));

                IEnumerable<OperationParameter> GetFormParams() =>
                    from param in mediaType.Schema.Properties
                    let required = mediaType.Schema.Required.Contains(param.Key)
                    let dataTypeBase = csharpSchemaResolver.ToInlineDataType(param.Value, context.Append(nameof(mediaType.Schema), null, mediaType.Schema).Append(nameof(mediaType.Schema.Properties), param.Key, param.Value), argument.Diagnostic)
                    let dataType = required ? dataTypeBase : dataTypeBase.MakeNullable()
                    select new templates.OperationParameter(
                        rawName: param.Key,
                        paramName: CSharpNaming.ToParameterName(param.Key, options.ReservedIdentifiers),
                        description: null,
                        dataType: dataType.text,
                        dataTypeNullable: dataType.nullable,
                        isPathParam: false,
                        isQueryParam: false,
                        isHeaderParam: false,
                        isCookieParam: false,
                        isBodyParam: false,
                        isFormParam: true,
                        required: mediaType.Schema.Required.Contains(param.Key),
                        pattern: param.Value.Pattern,
                        minLength: param.Value.MinLength,
                        maxLength: param.Value.MaxLength,
                        minimum: param.Value.Minimum,
                        maximum: param.Value.Maximum
                    );
                IEnumerable<OperationParameter> GetStandardParams() =>
                    from ct in new[] { mediaType }
                    let dataType = csharpSchemaResolver.ToInlineDataType(ct.Schema, context.Append(nameof(ct.Schema), null, ct.Schema), argument.Diagnostic)
                    select new templates.OperationParameter(
                       rawName: null,
                       paramName: CSharpNaming.ToParameterName(argument.Builder?.Operation.OperationId + " body", options.ReservedIdentifiers),
                       description: null,
                       dataType: dataType.text,
                       dataTypeNullable: dataType.nullable,
                       isPathParam: false,
                       isQueryParam: false,
                       isHeaderParam: false,
                       isCookieParam: false,
                       isBodyParam: true,
                       isFormParam: false,
                       required: true,
                       pattern: mediaType.Schema.Pattern,
                       minLength: mediaType.Schema.MinLength,
                       maxLength: mediaType.Schema.MaxLength,
                       minimum: mediaType.Schema.Minimum,
                       maximum: mediaType.Schema.Maximum
                   );
            }

            private Func<OperationParameter[], OperationRequestBody> OperationRequestBodyFactory(string operationName, string? requestBodyMimeType, IEnumerable<OperationParameter> parameters)
            {
                return sharedParams => new templates.OperationRequestBody(
                     name: CSharpNaming.ToTitleCaseIdentifier(operationName, options.ReservedIdentifiers),
                     requestBodyType: requestBodyMimeType,
                     allParams: sharedParams.Concat(parameters)
                 );
            }

            public override void Visit(OpenApiSecurityRequirement securityRequirement, OpenApiContext context, Argument argument)
            {
                argument.Builder?.SecurityRequirements.Add(new OperationSecurityRequirement(
                                     (from scheme in securityRequirement
                                      select new templates.OperationSecuritySchemeRequirement(scheme.Key.Reference.Id, scheme.Value.ToArray())).ToArray())
                                 );
            }
        }
    }
}
