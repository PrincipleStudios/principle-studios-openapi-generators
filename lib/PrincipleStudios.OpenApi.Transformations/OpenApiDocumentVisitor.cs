using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrincipleStudios.OpenApi.Transformations
{
    public abstract class OpenApiDocumentVisitor : IOpenApiDocumentVisitor, IOpenApiAnyVisitor
    {
        public virtual void VisitAny(IOpenApiElement openApiElement, IEnumerable<OpenApiContextEntry> context) => OpenApiDocumentVisitorExtensions.VisitAny(this, openApiElement, context);
        public virtual void VisitAny(Microsoft.OpenApi.Any.IOpenApiAny any, IEnumerable<OpenApiContextEntry> context) => OpenApiDocumentVisitorExtensions.VisitAny(this, any, context);


        public virtual void Visit(OpenApiDocument document, IEnumerable<OpenApiContextEntry> context)
        {
            this.VisitHelper(document.Servers, context, nameof(document.Servers));
            this.VisitHelper(document.ExternalDocs, context, nameof(document.ExternalDocs));
            this.VisitHelper(document.Components, context, nameof(document.Components));
            this.VisitHelper<OpenApiPaths>(document.Paths, context, nameof(document.Paths));
            this.VisitHelper(document.Info, context, nameof(document.Info));
            this.VisitHelper(document.SecurityRequirements, context, nameof(document.SecurityRequirements));
            this.VisitHelper(document.Tags, context, nameof(document.Tags));
        }

        public virtual void Visit(OpenApiCallback callback, IEnumerable<OpenApiContextEntry> context) { }

        public virtual void Visit(OpenApiComponents components, IEnumerable<OpenApiContextEntry> context)
        {
            this.VisitHelper(components.Callbacks, context, nameof(components.Callbacks));
            this.VisitHelper(components.Examples, context, nameof(components.Examples));
            this.VisitHelper(components.Headers, context, nameof(components.Headers));
            this.VisitHelper(components.Links, context, nameof(components.Links));
            this.VisitHelper(components.Parameters, context, nameof(components.Parameters));
            this.VisitHelper(components.RequestBodies, context, nameof(components.RequestBodies));
            this.VisitHelper(components.Responses, context, nameof(components.Responses));
            this.VisitHelper(components.Schemas, context, nameof(components.Schemas));
            this.VisitHelper(components.SecuritySchemes, context, nameof(components.SecuritySchemes));
        }

        public virtual void Visit(OpenApiEncoding encoding, IEnumerable<OpenApiContextEntry> context)
        {
            this.VisitHelper(encoding.Headers, context, nameof(encoding.Headers));
        }
        public virtual void Visit(OpenApiExample example, IEnumerable<OpenApiContextEntry> context)
        {
            this.VisitHelper(example.Value, context, nameof(example.Value));
        }
        public virtual void Visit(OpenApiExternalDocs docs, IEnumerable<OpenApiContextEntry> context) { }
        public virtual void Visit(OpenApiHeader header, IEnumerable<OpenApiContextEntry> context)
        {
            this.VisitHelper(header.Examples, context, nameof(header.Examples));
            this.VisitHelper(header.Example, context, nameof(header.Example));
            this.VisitHelper(header.Schema, context, nameof(header.Schema));
            this.VisitHelper(header.Content, context, nameof(header.Content));
        }

        public virtual void Visit(OpenApiInfo info, IEnumerable<OpenApiContextEntry> context)
        {
            this.VisitHelper(info.Contact, context, nameof(info.Contact));
            this.VisitHelper(info.License, context, nameof(info.License));
        }

        public virtual void Visit(OpenApiLicense license, IEnumerable<OpenApiContextEntry> context) { }
        public virtual void Visit(OpenApiLink link, IEnumerable<OpenApiContextEntry> context)
        {
            this.VisitHelper(link.Parameters, context, nameof(link.Parameters));
            this.VisitHelper(link.RequestBody, context, nameof(link.RequestBody));
            this.VisitHelper(link.Server, context, nameof(link.Server));
        }
        public virtual void Visit(OpenApiMediaType mediaType, IEnumerable<OpenApiContextEntry> context)
        {
            this.VisitHelper(mediaType.Schema, context, nameof(mediaType.Schema));
            this.VisitHelper(mediaType.Example, context, nameof(mediaType.Example));
            this.VisitHelper(mediaType.Examples, context, nameof(mediaType.Examples));
            this.VisitHelper(mediaType.Encoding, context, nameof(mediaType.Encoding));
        }
        public virtual void Visit(OpenApiOAuthFlow oauthFlow, IEnumerable<OpenApiContextEntry> context) { }
        public virtual void Visit(OpenApiOAuthFlows oAuthFlows, IEnumerable<OpenApiContextEntry> context)
        {
            this.VisitHelper(oAuthFlows.Implicit, context, nameof(oAuthFlows.Implicit));
            this.VisitHelper(oAuthFlows.Password, context, nameof(oAuthFlows.Password));
            this.VisitHelper(oAuthFlows.ClientCredentials, context, nameof(oAuthFlows.ClientCredentials));
            this.VisitHelper(oAuthFlows.AuthorizationCode, context, nameof(oAuthFlows.AuthorizationCode));
        }
        public virtual void Visit(OpenApiOperation operation, IEnumerable<OpenApiContextEntry> context)
        {
            this.VisitHelper(operation.Security, context, nameof(operation.Security));
            this.VisitHelper(operation.Callbacks, context, nameof(operation.Callbacks));
            this.VisitHelper(operation.Responses, context, nameof(operation.Responses));
            this.VisitHelper(operation.RequestBody, context, nameof(operation.RequestBody));
            this.VisitHelper(operation.Parameters, context, nameof(operation.Parameters));
            this.VisitHelper(operation.ExternalDocs, context, nameof(operation.ExternalDocs));
            this.VisitHelper(operation.Tags, context, nameof(operation.Tags));
            this.VisitHelper(operation.Servers, context, nameof(operation.Servers));
        }
        public virtual void Visit(OpenApiParameter parameter, IEnumerable<OpenApiContextEntry> context)
        {
            this.VisitHelper(parameter.Example, context, nameof(parameter.Example));
            this.VisitHelper(parameter.Examples, context, nameof(parameter.Examples));
            this.VisitHelper(parameter.Schema, context, nameof(parameter.Schema));
            this.VisitHelper(parameter.Content, context, nameof(parameter.Content));
        }
        public virtual void Visit(OpenApiPathItem pathItem, IEnumerable<OpenApiContextEntry> context)
        {
            this.VisitHelper(pathItem.Operations, context, nameof(pathItem.Operations));
            this.VisitHelper(pathItem.Servers, context, nameof(pathItem.Servers));
            this.VisitHelper(pathItem.Parameters, context, nameof(pathItem.Parameters));
        }

        public virtual void Visit(OpenApiPaths paths, IEnumerable<OpenApiContextEntry> context)
        {
            this.VisitHelper<string, OpenApiPathItem>(paths, context);
        }

        public virtual void Visit(OpenApiRequestBody requestBody, IEnumerable<OpenApiContextEntry> context)
        {
            this.VisitHelper(requestBody.Content, context, nameof(requestBody.Content));
        }
        public virtual void Visit(OpenApiResponse response, IEnumerable<OpenApiContextEntry> context)
        {
            this.VisitHelper(response.Headers, context, nameof(response.Headers));
            this.VisitHelper(response.Content, context, nameof(response.Content));
            this.VisitHelper(response.Links, context, nameof(response.Links));
        }
        public virtual void Visit(OpenApiResponses responses, IEnumerable<OpenApiContextEntry> context)
        {
            this.VisitHelper<string, OpenApiResponse>(responses, context);
        }
        public virtual void Visit(OpenApiSchema schema, IEnumerable<OpenApiContextEntry> context)
        {
            if (context.Any(e => e.Element == schema))
            {
                DetectedLoop(schema);
                return;
            }
            this.VisitHelper(schema.OneOf, context, nameof(schema.OneOf));
            this.VisitHelper(schema.Items, context, nameof(schema.Items));
            this.VisitHelper(schema.Properties, context, nameof(schema.Properties));
            this.VisitHelper(schema.AdditionalProperties, context, nameof(schema.AdditionalProperties));
            this.VisitHelper(schema.Discriminator, context, nameof(schema.Discriminator));
            this.VisitHelper(schema.Example, context, nameof(schema.Example));
            this.VisitHelper(schema.Enum, context, nameof(schema.Enum));
            this.VisitHelper(schema.ExternalDocs, context, nameof(schema.ExternalDocs));
            this.VisitHelper(schema.Xml, context, nameof(schema.Xml));
            this.VisitHelper(schema.Not, context, nameof(schema.Not));
            this.VisitHelper(schema.AnyOf, context, nameof(schema.AnyOf));
            this.VisitHelper(schema.AllOf, context, nameof(schema.AllOf));
            this.VisitHelper(schema.Default, context, nameof(schema.Default));
        }

        public virtual void Visit(OpenApiSecurityRequirement securityRequirement, IEnumerable<OpenApiContextEntry> context)
        {
            this.VisitHelper(securityRequirement.Keys, context, nameof(securityRequirement.Keys));
        }

        public virtual void Visit(OpenApiSecurityScheme securityScheme, IEnumerable<OpenApiContextEntry> context)
        {
            this.VisitHelper(securityScheme.Flows, context, nameof(securityScheme.Flows));
        }
        public virtual void Visit(OpenApiServer server, IEnumerable<OpenApiContextEntry> context)
        {
            this.VisitHelper(server.Variables, context, nameof(server.Variables));
        }
        public virtual void Visit(OpenApiServerVariable serverVariable, IEnumerable<OpenApiContextEntry> context) { }

        public virtual void Visit(OpenApiTag tag, IEnumerable<OpenApiContextEntry> context)
        {
            this.VisitHelper(tag.ExternalDocs, context, nameof(tag.ExternalDocs));
        }

        public virtual void Visit(OpenApiXml xml, IEnumerable<OpenApiContextEntry> context) { }
        public virtual void Visit(RuntimeExpressionAnyWrapper runtimeExpressionAnyWrapper, IEnumerable<OpenApiContextEntry> context)
        {
            this.VisitHelper(runtimeExpressionAnyWrapper.Any, context, nameof(runtimeExpressionAnyWrapper.Any));
        }

        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiArray _Array, IEnumerable<OpenApiContextEntry> context)
        {
            this.VisitHelper(_Array.AsEnumerable(), context);
        }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiBinary _Binary, IEnumerable<OpenApiContextEntry> context) { }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiBoolean _Boolean, IEnumerable<OpenApiContextEntry> context) { }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiByte _Byte, IEnumerable<OpenApiContextEntry> context) { }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiDate _Date, IEnumerable<OpenApiContextEntry> context) { }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiDateTime _DateTime, IEnumerable<OpenApiContextEntry> context) { }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiDouble _Double, IEnumerable<OpenApiContextEntry> context) { }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiFloat _Float, IEnumerable<OpenApiContextEntry> context) { }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiInteger _Integer, IEnumerable<OpenApiContextEntry> context) { }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiLong _Long, IEnumerable<OpenApiContextEntry> context) { }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiNull _Null, IEnumerable<OpenApiContextEntry> context) { }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiObject _Object, IEnumerable<OpenApiContextEntry> context)
        {
            this.VisitHelper<string, Microsoft.OpenApi.Any.IOpenApiAny>(_Object, context);
        }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiPassword _Password, IEnumerable<OpenApiContextEntry> context) { }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiString _String, IEnumerable<OpenApiContextEntry> context) { }

        protected virtual void DetectedLoop(OpenApiSchema schema) { }

        public virtual void VisitUnknown(IOpenApiElement openApiElement, IEnumerable<OpenApiContextEntry> context)
        {
            throw new NotSupportedException();
        }

        public virtual void VisitUnknown(IOpenApiAny anyElement, IEnumerable<OpenApiContextEntry> context)
        {
            throw new NotSupportedException();
        }

    }
}