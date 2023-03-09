using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApi.TypeScript.templates
{
    public record ModelTemplate<TModel>(
        PartialHeader header,

        string packageName,

        TModel model
    ) where TModel : Model;

    public record ImportStatement(
        string[] members,
        string path
    );

    public record Model(
        string description,
        string className
    );

    public record EnumModel(
        string description,
        string className,
        bool isString,
        EnumVar[] enumVars
    ) : Model(description, className);


    public record EnumVar(
        string value
    );

    public record ArrayModel(
        string Description,
        string ClassName,
        string Item,
        ImportStatement[] Imports
    ) : Model(Description, ClassName);

    public record ObjectModel(
        ImportStatement[] imports,
        string description,
        string className,
        string? parent,
        ModelVar[] vars
    ) : Model(description, className);

    public record TypeUnionModel(
        ImportStatement[] Imports,
        string Description,
        string ClassName,
        bool AllowAnyOf,
        string? DiscriminatorProperty,
        TypeUnionEntry[] TypeEntries
    ) : Model(Description, ClassName);

    public record TypeUnionEntry(
        string TypeName,
        string? DiscriminatorValue
    );

    public record ModelVar(
        string baseName,
        string dataType,
        bool nullable,
        bool isContainer,
        string name,
        bool required
    );
}
