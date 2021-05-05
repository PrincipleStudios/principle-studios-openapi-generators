using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace PrincipleStudios.OpenApi.Transformations
{
    public interface IOpenApiDocumentVisitor
    {
        void Visit(OpenApiCallback callback, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiComponents components, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiDocument document, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiEncoding encoding, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiExample example, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiExternalDocs docs, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiHeader header, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiInfo info, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiLicense license, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiLink link, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiMediaType mediaType, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiOAuthFlow oauthFlow, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiOAuthFlows oAuthFlows, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiOperation operation, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiParameter parameter, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiPathItem pathItem, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiPaths paths, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiRequestBody requestBody, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiResponse response, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiResponses responses, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiSchema schema, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiSecurityRequirement securityRequirement, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiSecurityScheme securityScheme, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiServer server, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiServerVariable serverVariable, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiTag tag, IEnumerable<OpenApiContextEntry> context);
        void Visit(OpenApiXml xml, IEnumerable<OpenApiContextEntry> context);
        void Visit(RuntimeExpressionAnyWrapper runtimeExpressionAnyWrapper, IEnumerable<OpenApiContextEntry> context);
        void VisitAny(IOpenApiElement openApiElement, IEnumerable<OpenApiContextEntry> context);
        void VisitUnknown(IOpenApiElement openApiElement, IEnumerable<OpenApiContextEntry> context);
    }

    public interface IOpenApiAnyVisitor
    {
        void Visit(Microsoft.OpenApi.Any.OpenApiArray _Array, IEnumerable<OpenApiContextEntry> context);
        void Visit(Microsoft.OpenApi.Any.OpenApiBinary _Binary, IEnumerable<OpenApiContextEntry> context);
        void Visit(Microsoft.OpenApi.Any.OpenApiBoolean _Boolean, IEnumerable<OpenApiContextEntry> context);
        void Visit(Microsoft.OpenApi.Any.OpenApiByte _Byte, IEnumerable<OpenApiContextEntry> context);
        void Visit(Microsoft.OpenApi.Any.OpenApiDate _Date, IEnumerable<OpenApiContextEntry> context);
        void Visit(Microsoft.OpenApi.Any.OpenApiDateTime _DateTime, IEnumerable<OpenApiContextEntry> context);
        void Visit(Microsoft.OpenApi.Any.OpenApiDouble _Double, IEnumerable<OpenApiContextEntry> context);
        void Visit(Microsoft.OpenApi.Any.OpenApiFloat _Float, IEnumerable<OpenApiContextEntry> context);
        void Visit(Microsoft.OpenApi.Any.OpenApiInteger _Integer, IEnumerable<OpenApiContextEntry> context);
        void Visit(Microsoft.OpenApi.Any.OpenApiLong _Long, IEnumerable<OpenApiContextEntry> context);
        void Visit(Microsoft.OpenApi.Any.OpenApiNull _Null, IEnumerable<OpenApiContextEntry> context);
        void Visit(Microsoft.OpenApi.Any.OpenApiObject _Object, IEnumerable<OpenApiContextEntry> context);
        void Visit(Microsoft.OpenApi.Any.OpenApiPassword _Password, IEnumerable<OpenApiContextEntry> context);
        void Visit(Microsoft.OpenApi.Any.OpenApiString _String, IEnumerable<OpenApiContextEntry> context);
        void VisitAny(Microsoft.OpenApi.Any.IOpenApiAny any, IEnumerable<OpenApiContextEntry> context);
        void VisitUnknown(IOpenApiAny anyElement, IEnumerable<OpenApiContextEntry> context);
    }
}