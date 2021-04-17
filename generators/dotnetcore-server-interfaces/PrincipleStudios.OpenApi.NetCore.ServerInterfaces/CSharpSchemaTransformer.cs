using HandlebarsDotNet;
using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PrincipleStudios.OpenApi.NetCore.ServerInterfaces
{
    public class CSharpSchemaTransformer : IOpenApiSchemaTransformer
    {
        private readonly string baseNamespace;
        private readonly OpenApiDocument document;
        private readonly Lazy<IHandlebars> handlebars = new Lazy<IHandlebars>(() => HandlebarsTemplateProcess.CreateHandlebars());

        public CSharpSchemaTransformer(OpenApiDocument document, string baseNamespace)
        {
            this.baseNamespace = baseNamespace;
            this.document = document;
        }

        public bool UseInline(OpenApiSchema schema, OpenApiComponents components)
        {
            // C# can't inline things that must be referenced, and vice versa. 
            return !UseReference(schema, components);
        }

        public bool UseReference(OpenApiSchema schema, OpenApiComponents components)
        {
            return schema switch
            {
                { Reference: { IsLocal: false } } => throw new ArgumentException("Cannot handle external reference"),
                { Reference: { Id: string refName, IsLocal: true } } when components.Schemas.ContainsKey(refName) && components.Schemas[refName] != schema => UseReference(components.Schemas[refName], components),
                { AllOf: { Count: > 1 } } => true,
                { AnyOf: { Count: > 1 } } => true,
                { Enum: { Count: > 1 } } => true,
                { Properties: { Count: > 1 } } => true,
                { Type: "string" or "number" or "integer" } => false,
                { Type: "array", Items: OpenApiSchema inner } => UseReference(inner, components),
                _ => throw new NotSupportedException(),
            };
        }

        public SourceEntry TransformComponentSchema(string key, OpenApiSchema schema)
        {
            return TransformSchema(baseNamespace, CSharpNaming.ToClassName(schema.Reference.Id), schema);
        }

        public SourceEntry TransformParameter(OpenApiOperation operation, OpenApiParameter parameter)
        {
            throw new System.NotImplementedException();
        }

        public SourceEntry TransformResponse(OpenApiOperation operation, KeyValuePair<string, OpenApiResponse> response, OpenApiMediaType mediaType)
        {
            throw new System.NotImplementedException();
        }


        private SourceEntry TransformSchema(string targetNamespace, string className, OpenApiSchema schema)
        {
            var entry = HandlebarsTemplateProcess.ProcessModel(new templates.ModelTemplate(
                appName: document.Info.Title,
                appDescription: document.Info.Description,
                version: document.Info.Version,
                infoEmail: document.Info.Contact?.Email,

                packageName: targetNamespace,
                className: className,

                model: schema switch
                {
                    { Enum: { Count: > 0 }, Type: "string" } => ToEnumModel(className, schema),
                    { Type: "object" } => ToObjectModel(className, schema),
                    _ => throw new NotSupportedException()
                }
            ), handlebars.Value);
            return new SourceEntry
            {
                Key = $"{targetNamespace}.{className}.cs",
                SourceText = entry,
            };
        }

        private templates.Model ToObjectModel(string className, OpenApiSchema schema)
        {
            return new templates.Model(
                isEnum: false,
                description: schema.Description,
                classname: className,
                parent: null, // TODO
                vars: (from entry in schema.Properties
                       select new templates.ModelVar(
                           baseName: entry.Key, 
                           name: CSharpNaming.ToPropertyName(entry.Key), 
                           required: schema.Required.Contains(entry.Key)
                        )).ToArray()
            );
        }

        private templates.Model ToEnumModel(string className, OpenApiSchema schema)
        {
            throw new NotImplementedException();
        }
    }
}