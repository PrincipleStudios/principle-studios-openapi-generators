using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.templates
{
    public record ModelTemplate<TModel>(
        PartialHeader header,

        string packageName,

        TModel model
    ) where TModel : Model;

    public record Model(
        string description,
        string className
    );

    public record ObjectModel(
        bool isEnum,
        string description,
        string className,
        string? parent,
        ModelVar[] vars
    ) : Model(description, className);

    public record ModelVar(
        string baseName,
        string dataType,
        string name,
        bool required
    );
}
