using PrincipleStudios.OpenApi.TypeScript.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript.Templates
{
    public record OperationTemplate(
        PartialHeader Header,

        Operation Operation
    );

    public record Operation(
        string HttpMethod,
        string Summary,
        string Description,
        string Name,
        string Path,
        bool AllowNoBody,
        bool HasFormRequest,
        bool HasQueryParams,
        IEnumerable<OperationParameter> SharedParams,
        ImportStatement[] Imports,
        OperationRequestBody[] RequestBodies,
        OperationResponses Responses,
        OperationSecurityRequirement[] SecurityRequirements
    )
    {
        public bool HasQueryInPath => Path.Contains('?');
    };

    public record OperationParameter(
        string? RawName,
        string? RawNameWithCurly,
        string ParamName,
        string? Description,
        string DataType,
        bool DataTypeEnumerable,
        bool DataTypeNullable,
        bool IsPathParam,
        bool IsQueryParam,
        bool IsHeaderParam,
        bool IsCookieParam,
        bool IsBodyParam,
        bool IsFormParam,
        bool Required,
        string Pattern,
        int? MinLength,
        int? MaxLength,
        decimal? Minimum,
        decimal? Maximum
    )
    {
        public bool IsUrlParam => IsPathParam || IsQueryParam;
    } 

    public record OperationResponses(
        OperationResponse? DefaultResponse,
        Dictionary<int, OperationResponse> StatusResponse
    );

    public record OperationResponse(
        string Description,
        OperationResponseContentOption[] Content,
        OperationResponseHeader[] Headers
    );

    public record OperationResponseContentOption(
        string MediaType,
        string ResponseMethodName,
        string? DataType
    );

    public record OperationRequestBody(string? RequestBodyType, bool IsForm, IEnumerable<OperationParameter> AllParams);

    public record OperationSecurityRequirement(
        OperationSecuritySchemeRequirement[] Schemes
    );
    public record OperationSecuritySchemeRequirement(
        string SchemeName,
        string[] ScopeNames
    );

    public record OperationResponseHeader(
        string? RawName,
        string ParamName,
        string? Description,
        string DataType,
        bool DataTypeNullable,
        bool Required,
        string Pattern,
        int? MinLength,
        int? MaxLength,
        decimal? Minimum,
        decimal? Maximum
    );
}
