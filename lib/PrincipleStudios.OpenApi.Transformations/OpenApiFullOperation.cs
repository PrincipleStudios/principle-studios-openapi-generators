using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace PrincipleStudios.OpenApi.Transformations
{
    public class OpenApiFullOperation
    {
        public OpenApiFullOperation(KeyValuePair<string, OpenApiPathItem> path, KeyValuePair<OperationType, OpenApiOperation> operation) : this(path.Key, path.Value, operation.Key, operation.Value)
        {
        }

        public OpenApiFullOperation(string path, OpenApiPathItem pathDetails, OperationType operationType, OpenApiOperation operation)
        {
            Path = path ?? throw new System.ArgumentException($"'{nameof(path)}' cannot be null or empty", nameof(path));
            PathDetails = pathDetails ?? throw new System.ArgumentNullException(nameof(pathDetails));
            OperationType = operationType;
            Operation = operation ?? throw new System.ArgumentNullException(nameof(operation));
        }

        public string Path { get; }
        public OpenApiPathItem PathDetails { get; }
        public OperationType OperationType { get; }
        public OpenApiOperation Operation { get; }
    }
}