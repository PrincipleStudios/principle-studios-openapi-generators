using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.CSharp.templates;
using PrincipleStudios.OpenApi.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrincipleStudios.OpenApi.CSharp;

namespace PrincipleStudios.OpenApi.CSharp
{
    public class CSharpPathControllerTransformer : CSharpSchemaTransformer, IOpenApiPathControllerTransformer
    {
        public CSharpPathControllerTransformer(OpenApiDocument document, string baseNamespace) : base(document, baseNamespace, ControllerHandlebarsTemplateProcess.CreateHandlebars)
        {
        }

        public SourceEntry TransformController(string path, OpenApiPathItem pathItem)
        {
            var className = CSharpNaming.ToClassName(path);

            var template = new templates.ControllerTemplate(
                header: new templates.PartialHeader(
                    appName: document.Info.Title,
                    appDescription: document.Info.Description,
                    version: document.Info.Version,
                    infoEmail: document.Info.Contact?.Email
                ),

                packageName: baseNamespace,
                className: className,
                hasDescriptionOrSummary: (pathItem.Summary?.Trim() + pathItem.Description?.Trim()) is { Length: > 0 },
                summary: pathItem.Summary,
                description: pathItem.Description,

                operations: (from operation in pathItem.Operations
                             let context = new[] { operation.Value.OperationId }
                             let sharedParams = (
                                     from param in operation.Value.Parameters
                                     let dataType = ToInlineDataType(param.Schema, param.Required, context.ConcatOne(param.Name))
                                     select new templates.OperationParameter(
                                         rawName: param.Name,
                                         paramName: CSharpNaming.ToParameterName(param.Name),
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
                                     )
                                 )
                             let requestTypes = (operation.Value.RequestBody?.Content ?? Enumerable.Empty<KeyValuePair<string, OpenApiMediaType?>>())
                                .DefaultIfEmpty(new(null, null))
                                .ToArray()
                             let singleContentType = requestTypes.Length == 1
                             // TODO - support form request parameters
                             select new templates.ControllerOperation(
                                 httpMethod: operation.Key.ToString("g"),
                                 summary: operation.Value.Summary,
                                 description: operation.Value.Description,
                                 name: CSharpNaming.ToTitleCaseIdentifier(operation.Value.OperationId),
                                 path: path,
                                 requestBodies: (from contentType in requestTypes
                                                 select new templates.OperationRequestBody(
                                                     name: CSharpNaming.ToTitleCaseIdentifier(operation.Value.OperationId + (singleContentType ? "" : contentType.Key)),
                                                     requestBodyType: contentType.Key,
                                                     allParams: sharedParams.Concat(contentType.Value == null 
                                                        ? Enumerable.Empty<templates.OperationParameter>() 
                                                        : from ct in new[] { contentType.Value }
                                                          let dataType = ToInlineDataType(ct.Schema, true, context.ConcatOne(contentType.Key + "Request"))
                                                          select new templates.OperationParameter(
                                                             rawName: null,
                                                             paramName: CSharpNaming.ToParameterName(operation.Value.OperationId + " body"),
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
                                                             pattern: contentType.Value.Schema.Pattern,
                                                             minLength: contentType.Value.Schema.MinLength,
                                                             maxLength: contentType.Value.Schema.MaxLength,
                                                             minimum: contentType.Value.Schema.Minimum,
                                                             maximum: contentType.Value.Schema.Maximum
                                                         )
                                                     )
                                                 )).ToArray(),
                                 responses: new templates.OperationResponses(
                                     defaultResponse: operation.Value.Responses.ContainsKey("default") ? ToOperationResponse(operation.Value.Responses["default"], context) : null,
                                     statusResponse: (from response in operation.Value.Responses
                                                      let parsed = TryParse(response.Key)
                                                      where parsed.parsed
                                                      select (Key: parsed.value, Value: response.Value)).ToDictionary(p => p.Key, p => ToOperationResponse(p.Value, context))
                                )
                             )).ToArray()
            );

            var entry = handlebars.Value.ProcessController(template);
            return new SourceEntry
            {
                Key = $"{baseNamespace}.{className}ControllerBase.cs",
                SourceText = entry,
            };

            (bool parsed, int value) TryParse(string value)
            {
                var parsed = int.TryParse(value, out var temp);
                return (parsed, temp);
            }
        }

        private OperationResponse ToOperationResponse(OpenApiResponse openApiResponse, IEnumerable<string> context)
        {
            
            return new OperationResponse(
                description: openApiResponse.Description,
                content: (from entry in openApiResponse.Content
                          let responseContext = openApiResponse.Reference != null ? Enumerable.Empty<string>().ConcatOne(openApiResponse.Reference.Id) : context.ConcatOne(entry.Key)
                          let dataType = entry.Value.Schema != null ? ToInlineDataType(entry.Value.Schema, true, responseContext.ConcatOne("response")) : null
                          select new OperationResponseContentOption(
                              mediaType: entry.Key,
                              mediaTypeId: CSharpNaming.ToTitleCaseIdentifier(entry.Key),
                              dataType: dataType?.text
                          )).ToArray()
            );
        }
    }
}
