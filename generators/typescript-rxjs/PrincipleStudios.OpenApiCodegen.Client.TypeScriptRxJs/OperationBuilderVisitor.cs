using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.Transformations;
using PrincipleStudios.OpenApi.TypeScript;
using PrincipleStudios.OpenApi.TypeScript.templates;
using PrincipleStudios.OpenApiCodegen.Client.TypeScriptRxJs.templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScriptRxJs
{

    class OperationBuilderVisitor : OpenApiDocumentVisitor<OperationBuilderVisitor.Argument>
    {
        private readonly ISchemaSourceResolver<InlineDataType> csharpSchemaResolver;
        private readonly TypeScriptSchemaOptions options;

        public record Argument(OpenApiTransformDiagnostic Diagnostic, OperationBuilder Builder);

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

        public OperationBuilderVisitor(ISchemaSourceResolver<InlineDataType> csharpSchemaResolver, TypeScriptSchemaOptions options)
        {
            this.csharpSchemaResolver = csharpSchemaResolver;
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
            var dataTypeBase = csharpSchemaResolver.ToInlineDataType(param.Schema)();
            var dataType = param.Required ? dataTypeBase : dataTypeBase.MakeNullable();
            argument.Builder?.SharedParameters.Add(new templates.OperationParameter(
                rawName: param.Name,
                paramName: TypeScriptNaming.ToParameterName(param.Name, options.ReservedIdentifiers()),
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
            var statusCodeName = statusCode.HasValue ? ((System.Net.HttpStatusCode)statusCode).ToString("g") : "other status code";
            if (statusCodeName == responseKey)
                statusCodeName = $"status code {statusCode}";

            var result = new OperationResponse(
                description: response.Description,
                content: (from entry in response.Content.DefaultIfEmpty(new("", new OpenApiMediaType()))
                          let entryContext = context.Append(nameof(response.Content), entry.Key, entry.Value)
                          let dataType = entry.Value.Schema != null ? csharpSchemaResolver.ToInlineDataType(entry.Value.Schema)() : null
                          select new OperationResponseContentOption(
                              mediaType: entry.Key,
                              responseMethodName: TypeScriptNaming.ToTitleCaseIdentifier($"{(response.Content.Count > 1 ? entry.Key : "")} {statusCodeName}", options.ReservedIdentifiers()),
                              dataType: dataType?.text
                          )).ToArray(),
                headers: (from entry in response.Headers
                          let entryContext = context.Append(nameof(response.Headers), entry.Key, entry.Value)
                          let required = entry.Value.Required
                          let dataTypeBase = csharpSchemaResolver.ToInlineDataType(entry.Value.Schema)()
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

            var isForm = mimeType == "application/x-www-form-urlencoded";

            var singleContentType = argument.Builder?.Operation.RequestBody.Content.Count <= 1;

            argument.Builder?.RequestBodies.Add(OperationRequestBodyFactory(argument.Builder?.Operation.OperationId + (singleContentType ? "" : mimeType), mimeType, isForm ? GetFormParams() : GetStandardParams()));

            IEnumerable<OperationParameter> GetFormParams() =>
                from param in mediaType.Schema.Properties
                let required = mediaType.Schema.Required.Contains(param.Key)
                let dataTypeBase = csharpSchemaResolver.ToInlineDataType(param.Value)()
                let dataType = required ? dataTypeBase : dataTypeBase.MakeNullable()
                select new templates.OperationParameter(
                    rawName: param.Key,
                    paramName: TypeScriptNaming.ToParameterName(param.Key, options.ReservedIdentifiers()),
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
                let dataType = csharpSchemaResolver.ToInlineDataType(ct.Schema)()
                select new templates.OperationParameter(
                   rawName: null,
                   paramName: TypeScriptNaming.ToParameterName(argument.Builder?.Operation.OperationId + " body", options.ReservedIdentifiers()),
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
                 name: TypeScriptNaming.ToTitleCaseIdentifier(operationName, options.ReservedIdentifiers()),
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