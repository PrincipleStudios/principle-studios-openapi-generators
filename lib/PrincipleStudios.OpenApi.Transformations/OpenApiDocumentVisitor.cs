using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrincipleStudios.OpenApi.Transformations
{
    public abstract class OpenApiDocumentVisitor<TArgument> : IOpenApiDocumentVisitor<TArgument>, IOpenApiAnyVisitor<TArgument>
    {
        public virtual void VisitAny(IOpenApiElement openApiElement, OpenApiContext context, TArgument argument) => OpenApiDocumentVisitorExtensions.VisitAny(this, openApiElement, context, argument);
        public virtual void VisitAny(Microsoft.OpenApi.Any.IOpenApiAny any, OpenApiContext context, TArgument argument) => OpenApiDocumentVisitorExtensions.VisitAny(this, any, context, argument);


        public virtual void Visit(OpenApiDocument document, OpenApiContext context, TArgument argument)
        {
            this.VisitHelper(document.Servers, context, nameof(document.Servers), argument);
            this.VisitHelper(document.ExternalDocs, context, nameof(document.ExternalDocs), argument);
            this.VisitHelper(document.Components, context, nameof(document.Components), argument);
            this.VisitHelper<TArgument, OpenApiPaths>(document.Paths, context, nameof(document.Paths), argument);
            this.VisitHelper(document.Info, context, nameof(document.Info), argument);
            this.VisitHelper(document.SecurityRequirements, context, nameof(document.SecurityRequirements), argument);
            this.VisitHelper(document.Tags, context, nameof(document.Tags), argument);
        }

        public virtual void Visit(OpenApiCallback callback, OpenApiContext context, TArgument argument) { }

        public virtual void Visit(OpenApiComponents components, OpenApiContext context, TArgument argument)
        {
            this.VisitHelper(components.Callbacks, context, nameof(components.Callbacks), argument);
            this.VisitHelper(components.Examples, context, nameof(components.Examples), argument);
            this.VisitHelper(components.Headers, context, nameof(components.Headers), argument);
            this.VisitHelper(components.Links, context, nameof(components.Links), argument);
            this.VisitHelper(components.Parameters, context, nameof(components.Parameters), argument);
            this.VisitHelper(components.RequestBodies, context, nameof(components.RequestBodies), argument);
            this.VisitHelper(components.Responses, context, nameof(components.Responses), argument);
            this.VisitHelper(components.Schemas, context, nameof(components.Schemas), argument);
            this.VisitHelper(components.SecuritySchemes, context, nameof(components.SecuritySchemes), argument);
        }

        public virtual void Visit(OpenApiEncoding encoding, OpenApiContext context, TArgument argument)
        {
            this.VisitHelper(encoding.Headers, context, nameof(encoding.Headers), argument);
        }
        public virtual void Visit(OpenApiExample example, OpenApiContext context, TArgument argument)
        {
            this.VisitHelper(example.Value, context, nameof(example.Value), argument);
        }
        public virtual void Visit(OpenApiExternalDocs docs, OpenApiContext context, TArgument argument) { }
        public virtual void Visit(OpenApiHeader header, OpenApiContext context, TArgument argument)
        {
            this.VisitHelper(header.Examples, context, nameof(header.Examples), argument);
            this.VisitHelper(header.Example, context, nameof(header.Example), argument);
            this.VisitHelper(header.Schema, context, nameof(header.Schema), argument);
            this.VisitHelper(header.Content, context, nameof(header.Content), argument);
        }

        public virtual void Visit(OpenApiInfo info, OpenApiContext context, TArgument argument)
        {
            this.VisitHelper(info.Contact, context, nameof(info.Contact), argument);
            this.VisitHelper(info.License, context, nameof(info.License), argument);
        }

        public virtual void Visit(OpenApiLicense license, OpenApiContext context, TArgument argument) { }
        public virtual void Visit(OpenApiLink link, OpenApiContext context, TArgument argument)
        {
            this.VisitHelper(link.Parameters, context, nameof(link.Parameters), argument);
            this.VisitHelper(link.RequestBody, context, nameof(link.RequestBody), argument);
            this.VisitHelper(link.Server, context, nameof(link.Server), argument);
        }
        public virtual void Visit(OpenApiMediaType mediaType, OpenApiContext context, TArgument argument)
        {
            this.VisitHelper(mediaType.Schema, context, nameof(mediaType.Schema), argument);
            this.VisitHelper(mediaType.Example, context, nameof(mediaType.Example), argument);
            this.VisitHelper(mediaType.Examples, context, nameof(mediaType.Examples), argument);
            this.VisitHelper(mediaType.Encoding, context, nameof(mediaType.Encoding), argument);
        }
        public virtual void Visit(OpenApiOAuthFlow oauthFlow, OpenApiContext context, TArgument argument) { }
        public virtual void Visit(OpenApiOAuthFlows oAuthFlows, OpenApiContext context, TArgument argument)
        {
            this.VisitHelper(oAuthFlows.Implicit, context, nameof(oAuthFlows.Implicit), argument);
            this.VisitHelper(oAuthFlows.Password, context, nameof(oAuthFlows.Password), argument);
            this.VisitHelper(oAuthFlows.ClientCredentials, context, nameof(oAuthFlows.ClientCredentials), argument);
            this.VisitHelper(oAuthFlows.AuthorizationCode, context, nameof(oAuthFlows.AuthorizationCode), argument);
        }
        public virtual void Visit(OpenApiOperation operation, OpenApiContext context, TArgument argument)
        {
            this.VisitHelper(operation.Security, context, nameof(operation.Security), argument);
            this.VisitHelper(operation.Callbacks, context, nameof(operation.Callbacks), argument);
            this.VisitHelper(operation.Responses, context, nameof(operation.Responses), argument);
            this.VisitHelper(operation.RequestBody, context, nameof(operation.RequestBody), argument);
            this.VisitHelper(operation.Parameters, context, nameof(operation.Parameters), argument);
            this.VisitHelper(operation.ExternalDocs, context, nameof(operation.ExternalDocs), argument);
            this.VisitHelper(operation.Tags, context, nameof(operation.Tags), argument);
            this.VisitHelper(operation.Servers, context, nameof(operation.Servers), argument);
        }
        public virtual void Visit(OpenApiParameter parameter, OpenApiContext context, TArgument argument)
        {
            this.VisitHelper(parameter.Example, context, nameof(parameter.Example), argument);
            this.VisitHelper(parameter.Examples, context, nameof(parameter.Examples), argument);
            this.VisitHelper(parameter.Schema, context, nameof(parameter.Schema), argument);
            this.VisitHelper(parameter.Content, context, nameof(parameter.Content), argument);
        }
        public virtual void Visit(OpenApiPathItem pathItem, OpenApiContext context, TArgument argument)
        {
            this.VisitHelper(pathItem.Operations, context, nameof(pathItem.Operations), argument);
            this.VisitHelper(pathItem.Servers, context, nameof(pathItem.Servers), argument);
            this.VisitHelper(pathItem.Parameters, context, nameof(pathItem.Parameters), argument);
        }

        public virtual void Visit(OpenApiPaths paths, OpenApiContext context, TArgument argument)
        {
            this.VisitHelper<TArgument, string, OpenApiPathItem>(paths, context, argument, property: null);
        }

        public virtual void Visit(OpenApiRequestBody requestBody, OpenApiContext context, TArgument argument)
        {
            this.VisitHelper(requestBody.Content, context, nameof(requestBody.Content), argument);
        }
        public virtual void Visit(OpenApiResponse response, OpenApiContext context, TArgument argument)
        {
            this.VisitHelper(response.Headers, context, nameof(response.Headers), argument);
            this.VisitHelper(response.Content, context, nameof(response.Content), argument);
            this.VisitHelper(response.Links, context, nameof(response.Links), argument);
        }
        public virtual void Visit(OpenApiResponses responses, OpenApiContext context, TArgument argument)
        {
            this.VisitHelper<TArgument, string, OpenApiResponse>(responses, context, argument, property: null);
        }
        public virtual void Visit(OpenApiSchema schema, OpenApiContext context, TArgument argument)
        {
            if (context.Take(context.Entries.Count - 1).Any(e => e.Element == schema))
            {
                DetectedLoop(schema);
                return;
            }
            this.VisitHelper(schema.OneOf, context, nameof(schema.OneOf), argument);
            this.VisitHelper(schema.Items, context, nameof(schema.Items), argument);
            this.VisitHelper(schema.Properties, context, nameof(schema.Properties), argument);
            this.VisitHelper(schema.AdditionalProperties, context, nameof(schema.AdditionalProperties), argument);
            this.VisitHelper(schema.Discriminator, context, nameof(schema.Discriminator), argument);
            this.VisitHelper(schema.Example, context, nameof(schema.Example), argument);
            this.VisitHelper(schema.Enum, context, nameof(schema.Enum), argument);
            this.VisitHelper(schema.ExternalDocs, context, nameof(schema.ExternalDocs), argument);
            this.VisitHelper(schema.Xml, context, nameof(schema.Xml), argument);
            this.VisitHelper(schema.Not, context, nameof(schema.Not), argument);
            this.VisitHelper(schema.AnyOf, context, nameof(schema.AnyOf), argument);
            this.VisitHelper(schema.AllOf, context, nameof(schema.AllOf), argument);
            this.VisitHelper(schema.Default, context, nameof(schema.Default), argument);
        }

        public virtual void Visit(OpenApiSecurityRequirement securityRequirement, OpenApiContext context, TArgument argument)
        {
            this.VisitHelper(securityRequirement.Keys, context, nameof(securityRequirement.Keys), argument);
        }

        public virtual void Visit(OpenApiSecurityScheme securityScheme, OpenApiContext context, TArgument argument)
        {
            this.VisitHelper(securityScheme.Flows, context, nameof(securityScheme.Flows), argument);
        }
        public virtual void Visit(OpenApiServer server, OpenApiContext context, TArgument argument)
        {
            this.VisitHelper(server.Variables, context, nameof(server.Variables), argument);
        }
        public virtual void Visit(OpenApiServerVariable serverVariable, OpenApiContext context, TArgument argument) { }

        public virtual void Visit(OpenApiTag tag, OpenApiContext context, TArgument argument)
        {
            this.VisitHelper(tag.ExternalDocs, context, nameof(tag.ExternalDocs), argument);
        }

        public virtual void Visit(OpenApiDiscriminator discriminator, OpenApiContext context, TArgument argument) { }

        public virtual void Visit(OpenApiXml xml, OpenApiContext context, TArgument argument) { }
        public virtual void Visit(RuntimeExpressionAnyWrapper runtimeExpressionAnyWrapper, OpenApiContext context, TArgument argument)
        {
            this.VisitHelper(runtimeExpressionAnyWrapper.Any, context, nameof(runtimeExpressionAnyWrapper.Any), argument);
        }

        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiArray _Array, OpenApiContext context, TArgument argument)
        {
            this.VisitHelper(_Array.AsEnumerable(), context, argument, property: null);
        }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiBinary _Binary, OpenApiContext context, TArgument argument) { }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiBoolean _Boolean, OpenApiContext context, TArgument argument) { }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiByte _Byte, OpenApiContext context, TArgument argument) { }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiDate _Date, OpenApiContext context, TArgument argument) { }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiDateTime _DateTime, OpenApiContext context, TArgument argument) { }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiDouble _Double, OpenApiContext context, TArgument argument) { }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiFloat _Float, OpenApiContext context, TArgument argument) { }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiInteger _Integer, OpenApiContext context, TArgument argument) { }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiLong _Long, OpenApiContext context, TArgument argument) { }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiNull _Null, OpenApiContext context, TArgument argument) { }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiObject _Object, OpenApiContext context, TArgument argument)
        {
            this.VisitHelper<TArgument, string, Microsoft.OpenApi.Any.IOpenApiAny>(_Object, context, argument, property: null);
        }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiPassword _Password, OpenApiContext context, TArgument argument) { }
        public virtual void Visit(Microsoft.OpenApi.Any.OpenApiString _String, OpenApiContext context, TArgument argument) { }

        protected virtual void DetectedLoop(OpenApiSchema schema) { }

        public virtual void VisitUnknown(IOpenApiElement openApiElement, OpenApiContext context, TArgument argument)
        {
            throw new NotSupportedException();
        }

        public virtual void VisitUnknown(IOpenApiAny anyElement, OpenApiContext context, TArgument argument)
        {
            throw new NotSupportedException();
        }

        public void Visit(OpenApiContact contact, OpenApiContext context, TArgument? argument) { }
    }
}