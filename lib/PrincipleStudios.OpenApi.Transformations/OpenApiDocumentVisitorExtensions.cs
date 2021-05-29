using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations
{
    public static class OpenApiDocumentVisitorExtensions
    {
        // Intentionally not an extension method, so that the overridden functionality is more likely to be used
        public static void VisitAny<TArgument>(IOpenApiDocumentVisitor<TArgument> visitor, IOpenApiElement openApiElement, OpenApiContext context, TArgument argument)
        {
            switch (openApiElement)
            {
                case null: break;
                case OpenApiDocument document: visitor.Visit(document, context, argument); break;
                case OpenApiCallback callback: visitor.Visit(callback, context, argument); break;
                case OpenApiComponents components: visitor.Visit(components, context, argument); break;
                case OpenApiEncoding encoding: visitor.Visit(encoding, context, argument); break;
                case OpenApiExample example: visitor.Visit(example, context, argument); break;
                case OpenApiExternalDocs docs: visitor.Visit(docs, context, argument); break;
                case OpenApiHeader header: visitor.Visit(header, context, argument); break;
                case OpenApiInfo info: visitor.Visit(info, context, argument); break;
                case OpenApiLicense license: visitor.Visit(license, context, argument); break;
                case OpenApiLink link: visitor.Visit(link, context, argument); break;
                case OpenApiMediaType mediaType: visitor.Visit(mediaType, context, argument); break;
                case OpenApiOAuthFlow oauthFlow: visitor.Visit(oauthFlow, context, argument); break;
                case OpenApiOAuthFlows oAuthFlows: visitor.Visit(oAuthFlows, context, argument); break;
                case OpenApiOperation operation: visitor.Visit(operation, context, argument); break;
                case OpenApiParameter parameter: visitor.Visit(parameter, context, argument); break;
                case OpenApiPathItem pathItem: visitor.Visit(pathItem, context, argument); break;
                case OpenApiPaths paths: visitor.Visit(paths, context, argument); break;
                case OpenApiRequestBody requestBody: visitor.Visit(requestBody, context, argument); break;
                case OpenApiResponse response: visitor.Visit(response, context, argument); break;
                case OpenApiResponses responses: visitor.Visit(responses, context, argument); break;
                case OpenApiSchema schema: visitor.Visit(schema, context, argument); break;
                case OpenApiSecurityRequirement securityRequirement: visitor.Visit(securityRequirement, context, argument); break;
                case OpenApiSecurityScheme securityScheme: visitor.Visit(securityScheme, context, argument); break;
                case OpenApiServer server: visitor.Visit(server, context, argument); break;
                case OpenApiServerVariable serverVariable: visitor.Visit(serverVariable, context, argument); break;
                case OpenApiTag tag: visitor.Visit(tag, context, argument); break;
                case OpenApiXml xml: visitor.Visit(xml, context, argument); break;
                case RuntimeExpressionAnyWrapper runtimeExpressionAnyWrapper: visitor.Visit(runtimeExpressionAnyWrapper, context, argument); break;
                case Microsoft.OpenApi.Any.IOpenApiAny any when visitor is IOpenApiAnyVisitor<TArgument> anyVisitor: anyVisitor.VisitAny(any, context, argument); break;
                default: visitor.VisitUnknown(openApiElement, context, argument); break;
            };
        }

        // Intentionally not an extension method, so that the overridden functionality is more likely to be used
        public static void VisitAny<TArgument>(IOpenApiAnyVisitor<TArgument> visitor, Microsoft.OpenApi.Any.IOpenApiAny anyElement, OpenApiContext context, TArgument argument)
        {
            switch (anyElement)
            {
                case Microsoft.OpenApi.Any.OpenApiArray _Array: visitor.Visit(_Array, context, argument); break;
                case Microsoft.OpenApi.Any.OpenApiBinary _Binary: visitor.Visit(_Binary, context, argument); break;
                case Microsoft.OpenApi.Any.OpenApiBoolean _Boolean: visitor.Visit(_Boolean, context, argument); break;
                case Microsoft.OpenApi.Any.OpenApiByte _Byte: visitor.Visit(_Byte, context, argument); break;
                case Microsoft.OpenApi.Any.OpenApiDate _Date: visitor.Visit(_Date, context, argument); break;
                case Microsoft.OpenApi.Any.OpenApiDateTime _DateTime: visitor.Visit(_DateTime, context, argument); break;
                case Microsoft.OpenApi.Any.OpenApiDouble _Double: visitor.Visit(_Double, context, argument); break;
                case Microsoft.OpenApi.Any.OpenApiFloat _Float: visitor.Visit(_Float, context, argument); break;
                case Microsoft.OpenApi.Any.OpenApiInteger _Integer: visitor.Visit(_Integer, context, argument); break;
                case Microsoft.OpenApi.Any.OpenApiLong _Long: visitor.Visit(_Long, context, argument); break;
                case Microsoft.OpenApi.Any.OpenApiNull _Null: visitor.Visit(_Null, context, argument); break;
                case Microsoft.OpenApi.Any.OpenApiObject _Object: visitor.Visit(_Object, context, argument); break;
                case Microsoft.OpenApi.Any.OpenApiPassword _Password: visitor.Visit(_Password, context, argument); break;
                case Microsoft.OpenApi.Any.OpenApiString _String: visitor.Visit(_String, context, argument); break;
                default: visitor.VisitUnknown(anyElement, context, argument); break;
            };
        }

        public static void VisitHelper<TArgument, TKey, T>(this IOpenApiDocumentVisitor<TArgument> visitor, IEnumerable<KeyValuePair<TKey, T>> entries, OpenApiContext context, TArgument argument)
            where T : IOpenApiElement
        {
            foreach (var kvp in entries)
                visitor.VisitAny(kvp.Value, context.Append(kvp.Key!.ToString(), kvp.Value), argument);
        }

        public static void VisitHelper<TArgument, T>(this IOpenApiDocumentVisitor<TArgument> visitor, IEnumerable<T> entries, OpenApiContext context, TArgument argument)
            where T : IOpenApiElement 
            => visitor.VisitHelper(entries.Select((value, key) => new KeyValuePair<int, T>(key, value)), context, argument);

        public static void VisitHelper<TArgument, TKey, T>(this IOpenApiDocumentVisitor<TArgument> visitor, IEnumerable<KeyValuePair<TKey, T>> entries, OpenApiContext context, string sourceProperty, TArgument argument)
            where T : IOpenApiElement
            => visitor.VisitHelper(entries, context.Append(sourceProperty), argument);

        public static void VisitHelper<TArgument, T>(this IOpenApiDocumentVisitor<TArgument> visitor, IEnumerable<T> entries, OpenApiContext context, string sourceProperty, TArgument argument)
            where T : IOpenApiElement
            => visitor.VisitHelper(entries, context.Append(sourceProperty), argument);

        public static void VisitHelper<TArgument, T>(this IOpenApiDocumentVisitor<TArgument> visitor, T e, OpenApiContext context, string sourceProperty, TArgument argument)
            where T : IOpenApiElement
        {
            visitor.VisitAny(e, context.Append(sourceProperty, e), argument);
        }
    }
}
