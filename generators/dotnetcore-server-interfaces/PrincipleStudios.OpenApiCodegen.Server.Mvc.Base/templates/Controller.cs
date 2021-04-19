using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApi.CSharp.templates
{
    public record ControllerTemplate(
        PartialHeader header,

        string packageName,
        string className,

        bool hasDescriptionOrSummary,
        string? summary,
        string? description,

        ControllerOperation[] operations
    );

    public record ControllerOperation(string httpMethod, string summary, string description, string name, string path, OperationRequestBody[] requestBodies, OperationResponses responses);

    public record OperationParameter(
        string? rawName,
        string paramName,
        string? description,
        string dataType,
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
        decimal? maximum);

    public record OperationResponses(
        OperationResponse? defaultResponse,
        Dictionary<int, OperationResponse> statusResponse);

    public record OperationResponse(
        string description,
        OperationResponseContentOption[] content
        // TODO - headers
        // TODO - links
    );

    public record OperationResponseContentOption(
        string mediaType,
        string mediaTypeId,
        string? dataType
    );

    public record OperationRequestBody(string name, string requestBodyType, IEnumerable<OperationParameter> allParams);

}
