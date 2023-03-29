using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApi.CSharp.Templates
{
    public record ControllerTemplate(
        PartialHeader Header,

        string PackageName,
        string ClassName,

        bool HasDescriptionOrSummary,
        string? Summary,
        string? Description,

        ControllerOperation[] Operations
    );

    public record ControllerOperation(
        string HttpMethod,
        string Summary,
        string Description,
        string Name,
        string Path,
        OperationRequestBody[] RequestBodies,
        OperationResponses Responses,
        OperationSecurityRequirement[] SecurityRequirements
    );

    public record OperationParameter(
        string? RawName,
        string ParamName,
        string? Description,
        string DataType,
        bool DataTypeNullable,
        bool IsPathParam,
        bool IsQueryParam,
        bool IsHeaderParam,
        bool IsCookieParam,
        bool IsBodyParam,
        bool IsFormParam,
        bool Required,
        bool Optional,
        string Pattern,
        int? MinLength,
        int? MaxLength,
        decimal? Minimum,
        decimal? Maximum
    )
    {
        public bool HasMinLength => MinLength.HasValue;
        public bool HasMaxLength => MaxLength.HasValue;

        public bool HasMinimum => Minimum.HasValue;
        public bool HasMaximum => Maximum.HasValue;
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

    public record OperationRequestBody(string Name, string? RequestBodyType, IEnumerable<OperationParameter> AllParams);

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
