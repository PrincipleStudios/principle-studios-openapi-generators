using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApi.NetCore.ServerInterfaces
{
    public class CSharpPathControllerTransformer : CSharpSchemaTransformer, IOpenApiPathControllerTransformer
    {
        public CSharpPathControllerTransformer(OpenApiDocument document, string baseNamespace) : base(document, baseNamespace)
        {
        }

        public IEnumerable<SourceEntry> TransformController(string path, OpenApiPathItem pathItem)
        {
            var className = CSharpNaming.ToClassName(path);

            var entry = HandlebarsTemplateProcess.ProcessController(new templates.ControllerTemplate(
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
                             let sharedParams = (
                                     from param in operation.Value.Parameters
                                     select new templates.OperationParameter(
                                         rawName: param.Name,
                                         paramName: CSharpNaming.ToParameterName(param.Name),
                                         description: param.Description,
                                         dataType: ToInlineDataType(param.Schema),
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
                             let singleContentType = operation.Value.RequestBody switch
                             {
                                 null => true,
                                 { Content: { Count: <= 1 } } => true,
                                 _ => false,
                             }
                             from contentType in (operation.Value.RequestBody?.Content ?? Enumerable.Empty<KeyValuePair<string, OpenApiMediaType?>>()).DefaultIfEmpty(new(null, null))
                             let operationId = operation.Value.OperationId + (singleContentType ? "" : contentType.Key)
                             select new templates.ControllerOperation(
                                 httpMethod: operation.Key.ToString("g"),
                                 summary: operation.Value.Summary,
                                 description: operation.Value.Description,
                                 name: CSharpNaming.ToMethodName(operationId),
                                 path: path,
                                 requestBodyType: contentType.Key,
                                 allParams: sharedParams.Concat(contentType.Value == null ? Enumerable.Empty<templates.OperationParameter>() : new[]
                                 {
                                     new templates.OperationParameter(
                                         rawName: null, 
                                         paramName: CSharpNaming.ToParameterName(operation.Value.OperationId + " body"),
                                         description: null,
                                         dataType: ToInlineDataType(contentType.Value.Schema),
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
                                 })
                             )).ToArray()
            ), handlebars.Value);
            yield return new SourceEntry
            {
                Key = $"{baseNamespace}.{className}ControllerBase.cs",
                SourceText = entry,
            };
        }
    }
}
