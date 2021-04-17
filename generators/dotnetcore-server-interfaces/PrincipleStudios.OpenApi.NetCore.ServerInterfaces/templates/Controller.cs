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

    public record ControllerOperation(string httpMethod, string summary, string description, string name, string path, IEnumerable<OperationParameter> allParams);

    public record OperationParameter(string paramName, string description, string dataType);
}
