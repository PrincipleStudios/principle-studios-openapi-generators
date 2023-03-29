using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApi.CSharp.Templates
{
    public record AddServicesModel(Templates.PartialHeader Header, string MethodName, string PackageName, ControllerReference[] Controllers);

    public record ControllerReference(string GenericTypeName, string ClassName);
}
