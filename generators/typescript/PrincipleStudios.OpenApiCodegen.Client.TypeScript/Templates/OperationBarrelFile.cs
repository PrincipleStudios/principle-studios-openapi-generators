using PrincipleStudios.OpenApi.TypeScript.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript.Templates
{
    public record OperationBarrelFileModel(PartialHeader header, OperationReference[] operations);

    public record OperationReference(string path, string methodName);
}
