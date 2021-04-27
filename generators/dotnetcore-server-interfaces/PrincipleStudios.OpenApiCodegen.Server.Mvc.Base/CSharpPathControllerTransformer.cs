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
        public CSharpPathControllerTransformer(OpenApiDocument document, string baseNamespace, CSharpSchemaOptions options) : base(document, baseNamespace, options, ControllerHandlebarsTemplateProcess.CreateHandlebars)
        {
        }

        public SourceEntry TransformController(string path, OpenApiPathItem pathItem, OpenApiTransformDiagnostic diagnostic)
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
                                     let dataType = ToInlineDataType(param.Schema, param.Required)
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
                             select new templates.ControllerOperation(
                                 httpMethod: operation.Key.ToString("g"),
                                 summary: operation.Value.Summary,
                                 description: operation.Value.Description,
                                 name: CSharpNaming.ToTitleCaseIdentifier(operation.Value.OperationId),
                                 path: path,
                                 requestBodies: (from contentType in requestTypes
                                                 let isForm = contentType.Key == "application/x-www-form-urlencoded"
                                                 select new templates.OperationRequestBody(
                                                     name: CSharpNaming.ToTitleCaseIdentifier(operation.Value.OperationId + (singleContentType ? "" : contentType.Key)),
                                                     requestBodyType: contentType.Key,
                                                     allParams: sharedParams.Concat(contentType.Value == null 
                                                        ? Enumerable.Empty<templates.OperationParameter>() 
                                                        : isForm ? from param in contentType.Value.Schema.Properties
                                                                   let dataType = ToInlineDataType(param.Value, contentType.Value.Schema.Required.Contains(param.Key))
                                                                   select new templates.OperationParameter(
                                                                       rawName: param.Key,
                                                                       paramName: CSharpNaming.ToParameterName(param.Key),
                                                                       description: null,
                                                                       dataType: dataType.text,
                                                                       dataTypeNullable: dataType.nullable,
                                                                       isPathParam: false,
                                                                       isQueryParam: false,
                                                                       isHeaderParam: false,
                                                                       isCookieParam: false,
                                                                       isBodyParam: false,
                                                                       isFormParam: true,
                                                                       required: contentType.Value.Schema.Required.Contains(param.Key),
                                                                       pattern: param.Value.Pattern,
                                                                       minLength: param.Value.MinLength,
                                                                       maxLength: param.Value.MaxLength,
                                                                       minimum: param.Value.Minimum,
                                                                       maximum: param.Value.Maximum
                                                                   )

                                                        : from ct in new[] { contentType.Value }
                                                          let dataType = ToInlineDataType(ct.Schema, true)
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
                                     defaultResponse: operation.Value.Responses.ContainsKey("default") ? ToOperationResponse(operation.Value.Responses["default"]) : null,
                                     statusResponse: (from response in operation.Value.Responses
                                                      let parsed = TryParse(response.Key)
                                                      where parsed.parsed
                                                      select (Key: parsed.value, Value: response.Value)).ToDictionary(p => p.Key, p => ToOperationResponse(p.Value))
                                 ),
                                 securityRequirements: (from requirement in operation.Value.Security
                                                        select new templates.OperationSecurityRequirements(string.Join(",", requirement.Keys.Select(k => k.Reference.Id)), string.Join(",", requirement.Values.SelectMany(v => v)))).ToArray()
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

        internal SourceEntry TransformAddServicesHelper(OpenApiPaths paths, OpenApiTransformDiagnostic diagnostic)
        {
            return new SourceEntry
            {
                Key = $"{baseNamespace}.AddServicesExtensions.cs",
                SourceText = handlebars.Value.ProcessAddServices(new templates.AddServicesModel(
                    header: new templates.PartialHeader(
                        appName: document.Info.Title,
                        appDescription: document.Info.Description,
                        version: document.Info.Version,
                        infoEmail: document.Info.Contact?.Email
                    ),
                    methodName: CSharpNaming.ToMethodName(document.Info.Title),
                    packageName: baseNamespace,
                    controllers: paths.Select(p => new templates.ControllerReference(CSharpNaming.ToClassName(p.Key))).ToArray()
                )),
            };
        }

        private OperationResponse ToOperationResponse(OpenApiResponse openApiResponse)
        {
            return new OperationResponse(
                description: openApiResponse.Description,
                content: (from entry in openApiResponse.Content
                          let dataType = entry.Value.Schema != null ? ToInlineDataType(entry.Value.Schema, true) : null
                          select new OperationResponseContentOption(
                              mediaType: entry.Key,
                              mediaTypeId: CSharpNaming.ToTitleCaseIdentifier(entry.Key),
                              dataType: dataType?.text
                          )).ToArray()
            );
        }
    }

    public static class PathControllerTransformerFactory
    {
        public static IOpenApiSourceTransformer ToOpenApiSourceTransformer(this CSharpPathControllerTransformer schemaTransformer)
        {
            return new CombineOpenApiSourceTransformer(
                new SchemaSourceTransformer(schemaTransformer),
                new PathControllerSourceTransformer(schemaTransformer),
                new DotNetMvcAddServicesHelperTransformer(schemaTransformer)
            );
        }
    }
}
