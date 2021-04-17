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
            // (Except with tuples, but those don't serialize/deserialize reliably yet.)
            return !UseReference(schema, components);
        }

        public bool UseReference(OpenApiSchema schema, OpenApiComponents components)
        {
            return schema switch
            {
                { UnresolvedReference: true } => throw new ArgumentException("Unable to resolve reference"),
                { AllOf: { Count: > 1 } } => true,
                { AnyOf: { Count: > 1 } } => true,
                { Enum: { Count: > 1 } } => true,
                { Properties: { Count: > 1 } } => true,
                { Type: "string" or "number" or "integer" } => false,
                { Type: "array", Items: OpenApiSchema inner } => UseReference(inner, components),
                _ => throw new NotSupportedException(),
            };
        }
        
        private string ToInlineDataType(OpenApiSchema schema)
        {
            // TODO: Allow configuration of this
            // from https://github.com/OAI/OpenAPI-Specification/blob/main/versions/3.0.3.md#data-types
            return schema switch
            {
                { Enum: { Count: > 1 } } => UseReferenceName(schema),
                { Type: "integer", Format: "int32" } => "int",
                { Type: "integer", Format: "int64" } => "long",
                { Type: "integer" } => "int",
                { Type: "number", Format: "float" } => "float",
                { Type: "number", Format: "double" } => "double",
                { Type: "number" } => "double",
                { Type: "string", Format: "byte" } => "string", // TODO - is there a way to automate base64 decoding without custom code?
                { Type: "string", Format: "binary" } => "string", // TODO - is there a way to automate octet decoding without custom code?
                { Type: "string", Format: "date" } => "string", // TODO - make DateOnly available if target is .NET 6
                { Type: "string", Format: "date-time" } => "global::System.DateTimeOffset",
                { Type: "string", Format: "uuid" or "guid" } => "global::System.Guid",
                { Type: "string" } => "string",
                _ => UseReferenceName(schema),
            };
        }

        private string UseReferenceName(OpenApiSchema schema)
        {
            return CSharpNaming.ToClassName(schema.Reference.Id);
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
                    _ => BuildObjectModel(schema) switch
                    {
                        ObjectModel model => ToObjectModel(className, schema, model),
                        null => throw new NotSupportedException()
                    },
                }
            ), handlebars.Value);
            return new SourceEntry
            {
                Key = $"{targetNamespace}.{className}.cs",
                SourceText = entry,
            };
        }

        private templates.Model ToObjectModel(string className, OpenApiSchema schema, ObjectModel objectModel)
        {
            if (objectModel == null)
                throw new ArgumentNullException(nameof(objectModel));
            var properties = objectModel.properties();
            var required = objectModel.required().ToHashSet();

            return new templates.Model(
                isEnum: false,
                description: schema.Description,
                classname: className,
                parent: null, // TODO
                vars: (from entry in properties
                       select new templates.ModelVar(
                           baseName: entry.Key,
                           dataType: ToInlineDataType(entry.Value),
                           name: CSharpNaming.ToPropertyName(entry.Key), 
                           required: required.Contains(entry.Key)
                        )).ToArray()
            );
        }

        record ObjectModel(Func<IDictionary<string, OpenApiSchema>> properties, Func<IEnumerable<string>> required);

        private ObjectModel? BuildObjectModel(OpenApiSchema schema) =>
            schema switch
            {
                { AllOf: { Count: > 0 } } => schema.AllOf.Select(BuildObjectModel).ToArray() switch {
                    ObjectModel[] models when models.All(v => v != null) => 
                        new ObjectModel(
                            properties: () => models.SelectMany(m => m!.properties()).ToDictionary(p => p.Key, p => p.Value),
                            required: () => models.SelectMany(m => m!.required()).Distinct()
                        ),
                    _ => null
                },
                { Type: "object" } => new ObjectModel(properties: () => schema.Properties, required: () => schema.Required),
                _ => null,
            };

        private templates.Model ToEnumModel(string className, OpenApiSchema schema)
        {
            throw new NotImplementedException();
        }
    }
}