using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApi.NetCore.ServerInterfaces
{
    class CSharpTagControllerTransformer : CSharpSchemaTransformer, IOpenApiTagControllerTransformer
    {
        public CSharpTagControllerTransformer(OpenApiDocument document, string baseNamespace) : base(document, baseNamespace)
        {
        }

        public IEnumerable<SourceEntry> TransformController(string key, IEnumerable<(OpenApiPathItem path, KeyValuePair<OperationType, OpenApiOperation> operation)> operations, OpenApiDocument document)
        {
            var className = CSharpNaming.ToClassName(key);

            var entry = HandlebarsTemplateProcess.ProcessController(new templates.ControllerTemplate(
                header: new templates.PartialHeader(
                    appName: document.Info.Title,
                    appDescription: document.Info.Description,
                    version: document.Info.Version,
                    infoEmail: document.Info.Contact?.Email
                ),

                packageName: baseNamespace,
                className: className,

                operations: new templates.ControllerOperation[] { }
            ), handlebars.Value);
            yield return new SourceEntry
            {
                Key = $"{baseNamespace}.{className}.cs",
                SourceText = entry,
            };
        }
    }
}
