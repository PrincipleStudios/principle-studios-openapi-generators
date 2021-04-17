using Microsoft.OpenApi.Models;
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

        bool hasDescriptionOrSummary,
        string? summary,
        string? description,

        ControllerOperation[] operations
    );

    public record ControllerOperation(string httpMethod, string summary, string description, string name, string path, string? requestBodyType, IEnumerable<OperationParameter> allParams);

    public record OperationParameter(
        string? rawName,
        string paramName,
        string? description,
        string dataType,
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
}
