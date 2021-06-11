using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PrincipleStudios.OpenApi.Transformations
{
    public class PathControllerSourceTransformer : OperationGroupingSourceTransformer
    {
        public delegate string? OperationToGroupOverride(OpenApiOperation operation, OpenApiContext context);


        public PathControllerSourceTransformer(OpenApiDocument document, IOpenApiOperationControllerTransformer operationControllerTransformer, OperationToGroupOverride? operationToGroupOverride = null)
            : base(document, GetGroup(operationToGroupOverride), operationControllerTransformer)
        {
        }

        private static OperationToGroup GetGroup(OperationToGroupOverride? operationToGroupOverride)
        {
            return (operation, context) =>
            {
                var group = operationToGroupOverride?.Invoke(operation, context);
                if (group != null)
                    return (group, null, null);

                var pathEntry = context.Where(c => c.Element is OpenApiPathItem).Last();
                if (pathEntry is not { Key: string path, Element: OpenApiPathItem pathItem })
                    throw new ArgumentException("Context is not initialized properly - key expected for path items", nameof(context));
                return (path, pathItem.Summary, pathItem.Description);
            };
        }
    }
}
