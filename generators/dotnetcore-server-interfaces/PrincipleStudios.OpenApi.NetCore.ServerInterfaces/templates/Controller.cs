using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApi.NetCore.ServerInterfaces.templates
{
    public record ControllerTemplate(
        PartialHeader header,

        string packageName,
        string className,

        ControllerOperation[] operations
    );

    public record ControllerOperation();
}
