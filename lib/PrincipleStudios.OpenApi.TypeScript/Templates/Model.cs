using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApi.TypeScript.Templates
{
    public record ModelTemplate<TModel>(
        PartialHeader Header,

        string PackageName,

        TModel Model
    ) where TModel : Model;

    public record ImportStatement(
        string[] Members,
        string Path
    );

    public record Model(
        string Description,
        string ClassName
    );

    public record EnumModel(
        string Description,
        string ClassName,
        string ConstName,
        bool IsString,
        EnumVar[] EnumVars
    ) : Model(Description, ClassName);


    public record EnumVar(
        string Value
    );

    public record ArrayModel(
        string Description,
        string ClassName,
        string Item,
        ImportStatement[] Imports
    ) : Model(Description, ClassName);

    public record ObjectModel(
        ImportStatement[] Imports,
        string Description,
        string ClassName,
        string? Parent,
        ModelVar[] Vars
    ) : Model(Description, ClassName);

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
        string BaseName,
        string DataType,
        bool Nullable,
        bool IsContainer,
        string Name,
        bool Required,
        bool Optional
    );
}
