using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApi.CSharp.Templates
{
    public record AddServicesModel(Templates.PartialHeader header, string methodName, string packageName, ControllerReference[] controllers);

    public record ControllerReference(string genericTypeName, string className);
}
