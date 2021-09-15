using PrincipleStudios.OpenApi.TypeScript.templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScriptRxJs.templates
{
    public record OperationBarrelFileModel(PartialHeader header, OperationReference[] operations);

    public record OperationReference(string path, string methodName);
}
