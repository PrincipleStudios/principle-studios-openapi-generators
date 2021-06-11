using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace PrincipleStudios.OpenApi.Transformations
{
    public interface IOpenApiOperationControllerTransformer
    {
        SourceEntry TransformController(string groupName, OperationGroupData groupData, OpenApiTransformDiagnostic diagnostic);
    }

    public class OperationGroupData
    {
        public string? Summary { get; set; }
        public string? Description { get; set; }
        public List<(OpenApiOperation operation, OpenApiContext context)> Operations { get; } = new ();

        public void Deconstruct(out string? summary, out string? description, out IEnumerable<(OpenApiOperation operation, OpenApiContext context)> operations)
        {
            summary = Summary;
            description = Description;
            operations = Operations;
        }
    }

}