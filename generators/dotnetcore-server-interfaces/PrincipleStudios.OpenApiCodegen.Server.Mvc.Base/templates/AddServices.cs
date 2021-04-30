using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApi.CSharp.templates
{
    public record AddServicesModel(templates.PartialHeader header, string methodName, string packageName, ControllerReference[] controllers);

    public record ControllerReference(string genericTypeName, string className);
}
