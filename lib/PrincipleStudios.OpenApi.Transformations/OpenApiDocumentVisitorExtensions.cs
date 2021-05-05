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
        public static void VisitAny(IOpenApiDocumentVisitor visitor, IOpenApiElement openApiElement, IEnumerable<OpenApiContextEntry> context)
        {
            switch (openApiElement)
            {
                case OpenApiDocument document: visitor.Visit(document, context); break;
                case OpenApiCallback callback: visitor.Visit(callback, context); break;
                case OpenApiComponents components: visitor.Visit(components, context); break;
                case OpenApiEncoding encoding: visitor.Visit(encoding, context); break;
                case OpenApiExample example: visitor.Visit(example, context); break;
                case OpenApiExternalDocs docs: visitor.Visit(docs, context); break;
                case OpenApiHeader header: visitor.Visit(header, context); break;
                case OpenApiInfo info: visitor.Visit(info, context); break;
                case OpenApiLicense license: visitor.Visit(license, context); break;
                case OpenApiLink link: visitor.Visit(link, context); break;
                case OpenApiMediaType mediaType: visitor.Visit(mediaType, context); break;
                case OpenApiOAuthFlow oauthFlow: visitor.Visit(oauthFlow, context); break;
                case OpenApiOAuthFlows oAuthFlows: visitor.Visit(oAuthFlows, context); break;
                case OpenApiOperation operation: visitor.Visit(operation, context); break;
                case OpenApiParameter parameter: visitor.Visit(parameter, context); break;
                case OpenApiPathItem pathItem: visitor.Visit(pathItem, context); break;
                case OpenApiPaths paths: visitor.Visit(paths, context); break;
                case OpenApiRequestBody requestBody: visitor.Visit(requestBody, context); break;
                case OpenApiResponse response: visitor.Visit(response, context); break;
                case OpenApiResponses responses: visitor.Visit(responses, context); break;
                case OpenApiSchema schema: visitor.Visit(schema, context); break;
                case OpenApiSecurityRequirement securityRequirement: visitor.Visit(securityRequirement, context); break;
                case OpenApiSecurityScheme securityScheme: visitor.Visit(securityScheme, context); break;
                case OpenApiServer server: visitor.Visit(server, context); break;
                case OpenApiServerVariable serverVariable: visitor.Visit(serverVariable, context); break;
                case OpenApiTag tag: visitor.Visit(tag, context); break;
                case OpenApiXml xml: visitor.Visit(xml, context); break;
                case RuntimeExpressionAnyWrapper runtimeExpressionAnyWrapper: visitor.Visit(runtimeExpressionAnyWrapper, context); break;
                case Microsoft.OpenApi.Any.IOpenApiAny any when visitor is IOpenApiAnyVisitor anyVisitor: anyVisitor.VisitAny(any, context); break;
                default: visitor.VisitUnknown(openApiElement, context); break;
            };
        }

        // Intentionally not an extension method, so that the overridden functionality is more likely to be used
        public static void VisitAny(IOpenApiAnyVisitor visitor, Microsoft.OpenApi.Any.IOpenApiAny anyElement, IEnumerable<OpenApiContextEntry> context)
        {
            switch (anyElement)
            {
                case Microsoft.OpenApi.Any.OpenApiArray _Array: visitor.Visit(_Array, context); break;
                case Microsoft.OpenApi.Any.OpenApiBinary _Binary: visitor.Visit(_Binary, context); break;
                case Microsoft.OpenApi.Any.OpenApiBoolean _Boolean: visitor.Visit(_Boolean, context); break;
                case Microsoft.OpenApi.Any.OpenApiByte _Byte: visitor.Visit(_Byte, context); break;
                case Microsoft.OpenApi.Any.OpenApiDate _Date: visitor.Visit(_Date, context); break;
                case Microsoft.OpenApi.Any.OpenApiDateTime _DateTime: visitor.Visit(_DateTime, context); break;
                case Microsoft.OpenApi.Any.OpenApiDouble _Double: visitor.Visit(_Double, context); break;
                case Microsoft.OpenApi.Any.OpenApiFloat _Float: visitor.Visit(_Float, context); break;
                case Microsoft.OpenApi.Any.OpenApiInteger _Integer: visitor.Visit(_Integer, context); break;
                case Microsoft.OpenApi.Any.OpenApiLong _Long: visitor.Visit(_Long, context); break;
                case Microsoft.OpenApi.Any.OpenApiNull _Null: visitor.Visit(_Null, context); break;
                case Microsoft.OpenApi.Any.OpenApiObject _Object: visitor.Visit(_Object, context); break;
                case Microsoft.OpenApi.Any.OpenApiPassword _Password: visitor.Visit(_Password, context); break;
                case Microsoft.OpenApi.Any.OpenApiString _String: visitor.Visit(_String, context); break;
                default: visitor.VisitUnknown(anyElement, context); break;
            };
        }

        public static void VisitHelper<TKey, T>(this IOpenApiDocumentVisitor visitor, IEnumerable<KeyValuePair<TKey, T>> entries, IEnumerable<OpenApiContextEntry> context)
            where T : IOpenApiElement
        {
            foreach (var kvp in entries)
                visitor.VisitAny(kvp.Value, context.ConcatOne(new OpenApiContextEntry(kvp.Key!.ToString(), kvp.Value)));
        }

        public static void VisitHelper<T>(this IOpenApiDocumentVisitor visitor, IEnumerable<T> entries, IEnumerable<OpenApiContextEntry> context)
            where T : IOpenApiElement 
            => visitor.VisitHelper(entries.Select((value, key) => new KeyValuePair<int, T>(key, value)), context);

        public static void VisitHelper<TKey, T>(this IOpenApiDocumentVisitor visitor, IEnumerable<KeyValuePair<TKey, T>> entries, IEnumerable<OpenApiContextEntry> context, string sourceProperty)
            where T : IOpenApiElement
            => visitor.VisitHelper(entries, context.ConcatOne(new OpenApiContextEntry(sourceProperty)));

        public static void VisitHelper<T>(this IOpenApiDocumentVisitor visitor, IEnumerable<T> entries, IEnumerable<OpenApiContextEntry> context, string sourceProperty)
            where T : IOpenApiElement
            => visitor.VisitHelper(entries, context.ConcatOne(new OpenApiContextEntry(sourceProperty)));

        public static void VisitHelper<T>(this IOpenApiDocumentVisitor visitor, T e, IEnumerable<OpenApiContextEntry> context, string sourceProperty)
            where T : IOpenApiElement
        {
            visitor.VisitAny(e, context.ConcatOne(new OpenApiContextEntry(sourceProperty)).ConcatOne(new OpenApiContextEntry(e)));
        }
    }
}
