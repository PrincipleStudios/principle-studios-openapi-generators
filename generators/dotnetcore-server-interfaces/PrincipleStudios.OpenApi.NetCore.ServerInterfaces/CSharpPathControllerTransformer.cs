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
                             select new templates.ControllerOperation(
                                 httpMethod: operation.Key.ToString("g"),
                                 summary: operation.Value.Summary,
                                 description: operation.Value.Description,
                                 name: CSharpNaming.ToMethodName(operation.Value.OperationId),
                                 path: path,
                                 allParams: from param in operation.Value.Parameters
                                            select new templates.OperationParameter(
                                                paramName: CSharpNaming.ToParameterName(param.Name),
                                                description: param.Description,
                                                dataType: ToInlineDataType(param.Schema)
                                            )
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
