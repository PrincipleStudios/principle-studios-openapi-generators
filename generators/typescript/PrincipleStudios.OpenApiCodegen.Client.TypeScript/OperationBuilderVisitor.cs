using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.Transformations;
using PrincipleStudios.OpenApi.TypeScript;
using PrincipleStudios.OpenApi.TypeScript.templates;
using PrincipleStudios.OpenApiCodegen.Client.TypeScript.templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript
{

    class OperationBuilderVisitor : OpenApiDocumentVisitor<OperationBuilderVisitor.Argument>
    {
        private const string formMimeType = "application/x-www-form-urlencoded";
        private readonly ISchemaSourceResolver<InlineDataType> typeScriptSchemaResolver;
        private readonly TypeScriptSchemaOptions options;

        public record Argument(OpenApiTransformDiagnostic Diagnostic, OperationBuilder Builder);

        public class OperationBuilder
        {
            public OperationBuilder(OpenApiOperation operation)
            {
                Operation = operation;
            }

            public List<OperationRequestBody> RequestBodies { get; } = new();

            public OperationResponse? DefaultResponse { get; set; }
            public Dictionary<int, OperationResponse> StatusResponses { get; } = new();
            public List<OperationSecurityRequirement> SecurityRequirements { get; } = new();
            public List<OperationParameter> SharedParameters { get; } = new();
            public OpenApiOperation Operation { get; }
        }

        public OperationBuilderVisitor(ISchemaSourceResolver<InlineDataType> typeScriptSchemaResolver, TypeScriptSchemaOptions options)
        {
            this.typeScriptSchemaResolver = typeScriptSchemaResolver;
            this.options = options;
        }

        public override void Visit(OpenApiOperation operation, OpenApiContext context, Argument argument)
        {
            var httpMethod = context.GetLastKeyFor(operation);
            if (httpMethod == null)
                throw new ArgumentException("Expected HTTP method from context", nameof(context));
            var path = context.Where(c => c.Element is OpenApiPathItem).Last().Key;
            if (path == null)
                throw new ArgumentException("Context is not initialized properly - key expected for path items", nameof(context));

            base.Visit(operation, context, argument);
        }

        public override void Visit(OpenApiParameter param, OpenApiContext context, Argument argument)
        {
            var dataTypeBase = typeScriptSchemaResolver.ToInlineDataType(param.Schema)();
            var dataType = param.Required ? dataTypeBase : dataTypeBase.MakeNullable();
            argument.Builder?.SharedParameters.Add(new templates.OperationParameter(
                rawName: param.Name,
                rawNameWithCurly: $"{{{param.Name}}}",
                paramName: TypeScriptNaming.ToParameterName(param.Name, options.ReservedIdentifiers()),
                description: param.Description,
                dataType: dataType.text,
                dataTypeEnumerable: dataType.isEnumerable,
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

        internal Operation ToOperationTemplate(OpenApiOperation operation, string httpMethod, string path, OperationBuilder builder)
        {
            var sharedParameters = builder.SharedParameters.ToArray();
            return (
                new templates.Operation(
                    httpMethod: httpMethod,
                    summary: operation.Summary,
                    description: operation.Description,
                    name: TypeScriptNaming.ToMethodName(operation.OperationId, options.ReservedIdentifiers()),
                    path: path,
                    allowNoBody: operation.RequestBody == null || !operation.RequestBody.Required || !operation.RequestBody.Content.Any(),
                    hasFormRequest: operation.RequestBody?.Content.Any(kvp => kvp.Key == formMimeType) ?? false,
                    imports: typeScriptSchemaResolver.GetImportStatements(GetSchemas(), "./operation/").ToArray(),
                    sharedParams: sharedParameters,
                    requestBodies: builder.RequestBodies.ToArray(),
                    responses: new templates.OperationResponses(
                        defaultResponse: builder.DefaultResponse,
                        statusResponse: new(builder.StatusResponses)
                    ),
                    securityRequirements: builder.SecurityRequirements.ToArray(),
                    hasQueryParams: operation.Parameters.Any(p => p.In == ParameterLocation.Query)
                ));

            IEnumerable<OpenApiSchema> GetSchemas()
            {
                return from set in new[]
                        {
                        from resp in operation.Responses.Values
                        from body in resp.Content.Values
                        select body.Schema,
                        from p in operation.Parameters
                        select p.Schema,
                        from mediaType in operation.RequestBody?.Content.Values ?? Enumerable.Empty<OpenApiMediaType>()
                        select mediaType.Schema
                    }
                        from schema in set
                        select schema;

            }
        }

        public override void Visit(OpenApiResponse response, OpenApiContext context, Argument argument)
        {
            var responseKey = context.GetLastKeyFor(response);
            int? statusCode = int.TryParse(responseKey, out var s) ? s : null;
            var statusCodeName = statusCode.HasValue ? ((System.Net.HttpStatusCode)statusCode).ToString("g") : "other status code";
            if (statusCodeName == responseKey)
                statusCodeName = $"status code {statusCode}";

            var result = new OperationResponse(
                description: response.Description,
                content: (from entry in response.Content.DefaultIfEmpty(new("", new OpenApiMediaType()))
                          let entryContext = context.Append(nameof(response.Content), entry.Key, entry.Value)
                          let dataType = entry.Value.Schema != null ? typeScriptSchemaResolver.ToInlineDataType(entry.Value.Schema)() : null
                          select new OperationResponseContentOption(
                              mediaType: entry.Key,
                              responseMethodName: TypeScriptNaming.ToTitleCaseIdentifier($"{(response.Content.Count > 1 ? entry.Key : "")} {statusCodeName}", options.ReservedIdentifiers()),
                              dataType: dataType?.text
                          )).ToArray(),
                headers: (from entry in response.Headers
                          let entryContext = context.Append(nameof(response.Headers), entry.Key, entry.Value)
                          let required = entry.Value.Required
                          let dataTypeBase = typeScriptSchemaResolver.ToInlineDataType(entry.Value.Schema)()
                          let dataType = required ? dataTypeBase : dataTypeBase.MakeNullable()
                          select new templates.OperationResponseHeader(
                              rawName: entry.Key,
                              paramName: TypeScriptNaming.ToParameterName("header " + entry.Key, options.ReservedIdentifiers()),
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

            var isForm = mimeType == formMimeType;

            var singleContentType = argument.Builder?.Operation.RequestBody.Content.Count <= 1;

            argument.Builder?.RequestBodies.Add(OperationRequestBody(mimeType, isForm, isForm ? GetFormParams() : GetStandardParams()));

            IEnumerable<OperationParameter> GetFormParams() =>
                from param in mediaType.Schema.Properties
                let required = mediaType.Schema.Required.Contains(param.Key)
                let dataTypeBase = typeScriptSchemaResolver.ToInlineDataType(param.Value)()
                let dataType = required ? dataTypeBase : dataTypeBase.MakeNullable()
                select new templates.OperationParameter(
                    rawName: param.Key,
                    rawNameWithCurly: $"{{{param.Key}}}",
                    paramName: TypeScriptNaming.ToParameterName(param.Key, options.ReservedIdentifiers()),
                    description: null,
                    dataType: dataType.text,
                    dataTypeEnumerable: dataType.isEnumerable,
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
                let dataType = typeScriptSchemaResolver.ToInlineDataType(ct.Schema)()
                select new templates.OperationParameter(
                   rawName: null,
                   rawNameWithCurly: null,
                   paramName: TypeScriptNaming.ToParameterName("body", options.ReservedIdentifiers()),
                   description: null,
                   dataType: dataType.text,
                   dataTypeEnumerable: dataType.isEnumerable,
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

        public static OperationRequestBody OperationRequestBody(string? requestBodyMimeType, bool isForm, IEnumerable<OperationParameter> parameters)
        {
            return new templates.OperationRequestBody(
                 requestBodyType: requestBodyMimeType,
                 isForm: isForm,
                 allParams: parameters
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