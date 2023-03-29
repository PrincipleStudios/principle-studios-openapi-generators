using PrincipleStudios.OpenApi.TypeScript.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript.Templates
{
    public record OperationTemplate(
        PartialHeader header,

        Operation operation
    );

    public record Operation(
        string httpMethod,
        string summary,
        string description,
        string name,
        string path,
        bool allowNoBody,
        bool hasFormRequest,
        bool hasQueryParams,
        IEnumerable<OperationParameter> sharedParams,
        ImportStatement[] imports,
        OperationRequestBody[] requestBodies,
        OperationResponses responses,
        OperationSecurityRequirement[] securityRequirements
    );

    public record OperationParameter(
        string? rawName,
        string? rawNameWithCurly,
        string paramName,
        string? description,
        string dataType,
        bool dataTypeEnumerable,
        bool dataTypeNullable,
        bool isPathParam,
        bool isQueryParam,
        bool isHeaderParam,
        bool isCookieParam,
        bool isBodyParam,
        bool isFormParam,
        bool required,
        string pattern,
        int? minLength,
        int? maxLength,
        decimal? minimum,
        decimal? maximum
    );

    public record OperationResponses(
        OperationResponse? defaultResponse,
        Dictionary<int, OperationResponse> statusResponse
    );

    public record OperationResponse(
        string description,
        OperationResponseContentOption[] content,
        OperationResponseHeader[] headers
    );

    public record OperationResponseContentOption(
        string mediaType,
        string responseMethodName,
        string? dataType
    );

    public record OperationRequestBody(string? requestBodyType, bool isForm, IEnumerable<OperationParameter> allParams);

    public record OperationSecurityRequirement(
        OperationSecuritySchemeRequirement[] schemes
    );
    public record OperationSecuritySchemeRequirement(
        string schemeName,
        string[] scopeNames
    );

    public record OperationResponseHeader(
        string? rawName,
        string paramName,
        string? description,
        string dataType,
        bool dataTypeNullable,
        bool required,
        string pattern,
        int? minLength,
        int? maxLength,
        decimal? minimum,
        decimal? maximum
    );
}
