using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.Transformations;
using PrincipleStudios.OpenApi.TypeScript;
using PrincipleStudios.OpenApi.TypeScript.Templates;
using PrincipleStudios.OpenApiCodegen.Client.TypeScript.Templates;
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
            var dataType = typeScriptSchemaResolver.ToInlineDataType(param.Schema)();
            argument.Builder?.SharedParameters.Add(new Templates.OperationParameter(
                RawName: param.Name,
                RawNameWithCurly: $"{{{param.Name}}}",
                ParamName: TypeScriptNaming.ToParameterName(param.Name, options.ReservedIdentifiers()),
                Description: param.Description,
                DataType: dataType.text,
                DataTypeEnumerable: dataType.isEnumerable,
                DataTypeNullable: dataType.nullable,
                IsPathParam: param.In == ParameterLocation.Path,
                IsQueryParam: param.In == ParameterLocation.Query,
                IsHeaderParam: param.In == ParameterLocation.Header,
                IsCookieParam: param.In == ParameterLocation.Cookie,
                IsBodyParam: false,
                IsFormParam: false,
                Required: param.Required,
                Pattern: param.Schema.Pattern,
                MinLength: param.Schema.MinLength,
                MaxLength: param.Schema.MaxLength,
                Minimum: param.Schema.Minimum,
                Maximum: param.Schema.Maximum
            ));
        }

        internal Operation ToOperationTemplate(OpenApiOperation operation, string httpMethod, string path, OperationBuilder builder)
        {
            var sharedParameters = builder.SharedParameters.ToArray();
            return (
                new Templates.Operation(
                    HttpMethod: httpMethod,
                    Summary: operation.Summary,
                    Description: operation.Description,
                    Name: TypeScriptNaming.ToMethodName(operation.OperationId, options.ReservedIdentifiers()),
                    Path: path,
                    AllowNoBody: operation.RequestBody == null || !operation.RequestBody.Required || !operation.RequestBody.Content.Any(),
                    HasFormRequest: operation.RequestBody?.Content.Any(kvp => kvp.Key == formMimeType) ?? false,
                    Imports: typeScriptSchemaResolver.GetImportStatements(GetSchemas(), Enumerable.Empty<OpenApiSchema>(), "./operation/").ToArray(),
                    SharedParams: sharedParameters,
                    RequestBodies: builder.RequestBodies.ToArray(),
                    Responses: new Templates.OperationResponses(
                        DefaultResponse: builder.DefaultResponse,
                        StatusResponse: new(builder.StatusResponses)
                    ),
                    SecurityRequirements: builder.SecurityRequirements.ToArray(),
                    HasQueryParams: operation.Parameters.Any(p => p.In == ParameterLocation.Query)
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
            if (!statusCode.HasValue || !HttpStatusCodes.StatusCodeNames.TryGetValue(statusCode.Value, out var statusCodeName))
                statusCodeName = "other status code";
            if (statusCodeName == responseKey)
                statusCodeName = $"status code {statusCode}";

            var result = new OperationResponse(
                Description: response.Description,
                Content: (from entry in response.Content.DefaultIfEmpty(new(string.Empty, new OpenApiMediaType()))
                          where entry.Key == string.Empty || options.AllowedMimeTypes.Contains(entry.Key)
                          let entryContext = context.Append(nameof(response.Content), entry.Key, entry.Value)
                          let dataType = entry.Value.Schema != null ? typeScriptSchemaResolver.ToInlineDataType(entry.Value.Schema)() : null
                          select new OperationResponseContentOption(
                              MediaType: entry.Key,
                              ResponseMethodName: TypeScriptNaming.ToTitleCaseIdentifier($"{(response.Content.Count > 1 ? entry.Key : "")} {statusCodeName}", options.ReservedIdentifiers()),
                              DataType: dataType?.text
                          )).ToArray(),
                Headers: (from entry in response.Headers
                          let entryContext = context.Append(nameof(response.Headers), entry.Key, entry.Value)
                          let required = entry.Value.Required
                          let dataType = typeScriptSchemaResolver.ToInlineDataType(entry.Value.Schema)()
                          select new Templates.OperationResponseHeader(
                              RawName: entry.Key,
                              ParamName: TypeScriptNaming.ToParameterName("header " + entry.Key, options.ReservedIdentifiers()),
                              Description: entry.Value.Description,
                              DataType: dataType.text,
                              DataTypeNullable: dataType.nullable,
                              Required: entry.Value.Required,
                              Pattern: entry.Value.Schema.Pattern,
                              MinLength: entry.Value.Schema.MinLength,
                              MaxLength: entry.Value.Schema.MaxLength,
                              Minimum: entry.Value.Schema.Minimum,
                              Maximum: entry.Value.Schema.Maximum
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
            var mimeType = context.GetLastKeyFor(mediaType)!;
            if (!options.AllowedMimeTypes.Contains(mimeType))
                return;

            var isForm = mimeType == formMimeType;

            var singleContentType = argument.Builder?.Operation.RequestBody.Content.Count <= 1;

            argument.Builder?.RequestBodies.Add(OperationRequestBody(mimeType, isForm, isForm ? GetFormParams() : GetStandardParams()));

            IEnumerable<OperationParameter> GetFormParams() =>
                from param in mediaType.Schema.Properties
                let required = mediaType.Schema.Required.Contains(param.Key)
                let dataType = typeScriptSchemaResolver.ToInlineDataType(param.Value)()
                select new Templates.OperationParameter(
                    RawName: param.Key,
                    RawNameWithCurly: $"{{{param.Key}}}",
                    ParamName: TypeScriptNaming.ToParameterName(param.Key, options.ReservedIdentifiers()),
                    Description: null,
                    DataType: dataType.text,
                    DataTypeEnumerable: dataType.isEnumerable,
                    DataTypeNullable: dataType.nullable,
                    IsPathParam: false,
                    IsQueryParam: false,
                    IsHeaderParam: false,
                    IsCookieParam: false,
                    IsBodyParam: false,
                    IsFormParam: true,
                    Required: mediaType.Schema.Required.Contains(param.Key),
                    Pattern: param.Value.Pattern,
                    MinLength: param.Value.MinLength,
                    MaxLength: param.Value.MaxLength,
                    Minimum: param.Value.Minimum,
                    Maximum: param.Value.Maximum
                );
            IEnumerable<OperationParameter> GetStandardParams() =>
                from ct in new[] { mediaType }
                let dataType = typeScriptSchemaResolver.ToInlineDataType(ct.Schema)()
                select new Templates.OperationParameter(
                   RawName: null,
                   RawNameWithCurly: null,
                   ParamName: TypeScriptNaming.ToParameterName("body", options.ReservedIdentifiers()),
                   Description: null,
                   DataType: dataType.text,
                   DataTypeEnumerable: dataType.isEnumerable,
                   DataTypeNullable: dataType.nullable,
                   IsPathParam: false,
                   IsQueryParam: false,
                   IsHeaderParam: false,
                   IsCookieParam: false,
                   IsBodyParam: true,
                   IsFormParam: false,
                   Required: true,
                   Pattern: mediaType.Schema.Pattern,
                   MinLength: mediaType.Schema.MinLength,
                   MaxLength: mediaType.Schema.MaxLength,
                   Minimum: mediaType.Schema.Minimum,
                   Maximum: mediaType.Schema.Maximum
               );
        }

        public static OperationRequestBody OperationRequestBody(string requestBodyMimeType, bool isForm, IEnumerable<OperationParameter> parameters)
        {
            return new Templates.OperationRequestBody(
                 RequestBodyType: requestBodyMimeType,
                 IsForm: isForm,
                 AllParams: parameters
             );
        }

        public override void Visit(OpenApiSecurityRequirement securityRequirement, OpenApiContext context, Argument argument)
        {
            argument.Builder?.SecurityRequirements.Add(new OperationSecurityRequirement(
                                 (from scheme in securityRequirement
                                  select new Templates.OperationSecuritySchemeRequirement(scheme.Key.Reference.Id, scheme.Value.ToArray())).ToArray())
                             );
        }
    }
}