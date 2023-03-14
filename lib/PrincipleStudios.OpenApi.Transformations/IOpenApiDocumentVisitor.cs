using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace PrincipleStudios.OpenApi.Transformations
{
    public interface IOpenApiDocumentVisitor<TArgument>
    {
        void Visit(OpenApiCallback callback, OpenApiContext context, TArgument argument);
        void Visit(OpenApiComponents components, OpenApiContext context, TArgument argument);
        void Visit(OpenApiDocument document, OpenApiContext context, TArgument argument);
        void Visit(OpenApiEncoding encoding, OpenApiContext context, TArgument argument);
        void Visit(OpenApiExample example, OpenApiContext context, TArgument argument);
        void Visit(OpenApiExternalDocs docs, OpenApiContext context, TArgument argument);
        void Visit(OpenApiHeader header, OpenApiContext context, TArgument argument);
        void Visit(OpenApiInfo info, OpenApiContext context, TArgument argument);
        void Visit(OpenApiLicense license, OpenApiContext context, TArgument argument);
        void Visit(OpenApiLink link, OpenApiContext context, TArgument argument);
        void Visit(OpenApiMediaType mediaType, OpenApiContext context, TArgument argument);
        void Visit(OpenApiOAuthFlow oauthFlow, OpenApiContext context, TArgument argument);
        void Visit(OpenApiOAuthFlows oAuthFlows, OpenApiContext context, TArgument argument);
        void Visit(OpenApiOperation operation, OpenApiContext context, TArgument argument);
        void Visit(OpenApiParameter parameter, OpenApiContext context, TArgument argument);
        void Visit(OpenApiPathItem pathItem, OpenApiContext context, TArgument argument);
        void Visit(OpenApiPaths paths, OpenApiContext context, TArgument argument);
        void Visit(OpenApiRequestBody requestBody, OpenApiContext context, TArgument argument);
        void Visit(OpenApiResponse response, OpenApiContext context, TArgument argument);
        void Visit(OpenApiResponses responses, OpenApiContext context, TArgument argument);
        void Visit(OpenApiSchema schema, OpenApiContext context, TArgument argument);
        void Visit(OpenApiSecurityRequirement securityRequirement, OpenApiContext context, TArgument argument);
        void Visit(OpenApiSecurityScheme securityScheme, OpenApiContext context, TArgument argument);
        void Visit(OpenApiServer server, OpenApiContext context, TArgument argument);
        void Visit(OpenApiServerVariable serverVariable, OpenApiContext context, TArgument argument);
        void Visit(OpenApiTag tag, OpenApiContext context, TArgument argument);
        void Visit(OpenApiXml xml, OpenApiContext context, TArgument argument);
        void Visit(RuntimeExpressionAnyWrapper runtimeExpressionAnyWrapper, OpenApiContext context, TArgument argument);
        void Visit(OpenApiContact contact, OpenApiContext context, TArgument? argument);
        void Visit(OpenApiDiscriminator discriminator, OpenApiContext context, TArgument argument);
        void VisitAny(IOpenApiElement openApiElement, OpenApiContext context, TArgument argument);
        void VisitUnknown(IOpenApiElement openApiElement, OpenApiContext context, TArgument argument);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:ParameterNameStartsWithUnderscores")]
    public interface IOpenApiAnyVisitor<TArgument>
    {
        void Visit(Microsoft.OpenApi.Any.OpenApiArray _Array, OpenApiContext context, TArgument argument);
        void Visit(Microsoft.OpenApi.Any.OpenApiBinary _Binary, OpenApiContext context, TArgument argument);
        void Visit(Microsoft.OpenApi.Any.OpenApiBoolean _Boolean, OpenApiContext context, TArgument argument);
        void Visit(Microsoft.OpenApi.Any.OpenApiByte _Byte, OpenApiContext context, TArgument argument);
        void Visit(Microsoft.OpenApi.Any.OpenApiDate _Date, OpenApiContext context, TArgument argument);
        void Visit(Microsoft.OpenApi.Any.OpenApiDateTime _DateTime, OpenApiContext context, TArgument argument);
        void Visit(Microsoft.OpenApi.Any.OpenApiDouble _Double, OpenApiContext context, TArgument argument);
        void Visit(Microsoft.OpenApi.Any.OpenApiFloat _Float, OpenApiContext context, TArgument argument);
        void Visit(Microsoft.OpenApi.Any.OpenApiInteger _Integer, OpenApiContext context, TArgument argument);
        void Visit(Microsoft.OpenApi.Any.OpenApiLong _Long, OpenApiContext context, TArgument argument);
        void Visit(Microsoft.OpenApi.Any.OpenApiNull _Null, OpenApiContext context, TArgument argument);
        void Visit(Microsoft.OpenApi.Any.OpenApiObject _Object, OpenApiContext context, TArgument argument);
        void Visit(Microsoft.OpenApi.Any.OpenApiPassword _Password, OpenApiContext context, TArgument argument);
        void Visit(Microsoft.OpenApi.Any.OpenApiString _String, OpenApiContext context, TArgument argument);
        void VisitAny(Microsoft.OpenApi.Any.IOpenApiAny any, OpenApiContext context, TArgument argument);
        void VisitUnknown(IOpenApiAny anyElement, OpenApiContext context, TArgument argument);
    }
}