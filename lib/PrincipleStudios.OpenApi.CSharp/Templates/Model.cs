using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApi.CSharp.Templates
{
    public record ModelTemplate<TModel>(
        PartialHeader Header,

        string PackageName,

        TModel Model
    ) where TModel : Model;

    public record Model(
        string Description,
        string ClassName
    );

    public record EnumModel(
        string Description,
        string ClassName,
        bool IsString,
        EnumVar[] EnumVars
    ) : Model(Description, ClassName);

    public record TypeUnionModel(
        string Description,
        string ClassName,
        bool AllowAnyOf,
        string? DiscriminatorProperty,
        TypeUnionEntry[] TypeEntries
    ) : Model(Description, ClassName);

    public record TypeUnionEntry(
        string TypeName,
        string Identifier,
        string? DiscriminatorValue
    );

    public record EnumVar(
        string Name,
        string Value
    );

    public record ObjectModel(
        string Description,
        string ClassName,
        string? Parent,
        ModelVar[] Vars
    ) : Model(Description, ClassName);

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
