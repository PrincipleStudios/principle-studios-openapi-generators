using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApi.NetCore.ServerInterfaces.templates
{
    public record ModelTemplate(
        string? appName,
        string? appDescription,
        string? version,
        string? infoEmail,

        string packageName, 
        string className,

        Model model
    ) : PartialHeader(appName, appDescription, version, infoEmail);

    public record Model(
        bool isEnum,
        string description,
        string classname,
        string? parent,
        ModelVar[] vars
    );

    public record ModelVar(
        string baseName,
        string name,
        bool required
    );
}
