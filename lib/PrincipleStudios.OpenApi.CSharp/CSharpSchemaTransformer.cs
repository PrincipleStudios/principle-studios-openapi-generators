using HandlebarsDotNet;
using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PrincipleStudios.OpenApi.CSharp
{
    public record InlineDataType(string text, bool nullable = false, bool isEnumerable = false)
    {
        // Assumes C#9 (thanks to source generators being new at that time)
        internal InlineDataType MakeNullable() =>
            nullable ? this : new(text + "?", nullable: true, isEnumerable: isEnumerable);
    }

    public class CSharpSchemaTransformer : IOpenApiSchemaTransformer
    {
        protected readonly string baseNamespace;
        protected readonly OpenApiDocument document;
        protected readonly Lazy<IHandlebars> handlebars;

        public CSharpSchemaTransformer(OpenApiDocument document, string baseNamespace, Func<IHandlebars> handlebarsFactory)
        {
            this.baseNamespace = baseNamespace;
            this.document = document;
            this.handlebars = new Lazy<IHandlebars>(handlebarsFactory);
        }

        public bool MakeReference(OpenApiSchema schema)
        {
            return schema switch
            {
                { Reference: not null, UnresolvedReference: false } => false,
                { Type: "string", Enum: { Count: > 1 } } => true,
                { AnyOf: { Count: > 1 } } => true,
                { OneOf: { Count: > 1 } } => true,
                { AllOf: { Count: > 1 } } => true,
                { Type: "object", Properties: { Count: 0 }, AdditionalProperties: OpenApiSchema dictionaryValueSchema } => false,
                { Type: "integer" } => false,
                { Type: "number" } => false,
                { Type: "string" } => false,
                { Type: "boolean" } => false,
                { Type: "array", Items: OpenApiSchema items } => false,
                { Properties: { Count: > 1 } } => true,
                _ => throw new NotSupportedException("Unkonwn schema"),
            };
        }

        public bool UseInline(OpenApiSchema schema)
        {
            // C# can't inline things that must be referenced, and vice versa.
            // (Except with tuples, but those don't serialize/deserialize reliably yet.)
            return !UseReference(schema);
        }

        public bool UseReference(OpenApiSchema schema)
        {
            return schema switch
            {
                { Type: "object", Properties: { Count: 0 }, AdditionalProperties: OpenApiSchema _ } => false,
                { UnresolvedReference: true } => throw new ArgumentException("Unable to resolve reference"),
                { AllOf: { Count: > 1 } } => true,
                { AnyOf: { Count: > 1 } } => true,
                { Type: "string", Enum: { Count: > 1 } } => true,
                { Properties: { Count: > 1 } } => true,
                { Type: "string" or "number" or "integer" or "boolean" } => false,
                { Type: "array", Items: OpenApiSchema inner } => UseReference(inner),
                _ => throw new NotSupportedException(),
            };
        }

        protected InlineDataType ToInlineDataType(OpenApiSchema schema, bool required)
        {
            // TODO: Allow configuration of this
            // from https://github.com/OAI/OpenAPI-Specification/blob/main/versions/3.0.3.md#data-types
            InlineDataType result = schema switch
            {
                { Reference: not null, UnresolvedReference: false } => new(UseReferenceName(schema)),
                { Type: "object", Properties: { Count: 0 }, AdditionalProperties: OpenApiSchema dictionaryValueSchema } => new($"global::System.Collections.Generic.Dictionary<string, {ToInlineDataType(dictionaryValueSchema, true).text}>", isEnumerable: true),
                { Type: "integer", Format: "int32" } => new("int"),
                { Type: "integer", Format: "int64" } => new("long"),
                { Type: "integer" } => new("int"),
                { Type: "number", Format: "float" } => new("float"),
                { Type: "number", Format: "double" } => new("double"),
                { Type: "number" } => new("double"),
                { Type: "string", Format: "byte" } => new("string"), // TODO - is there a way to automate base64 decoding without custom code?
                { Type: "string", Format: "binary" } => new("string"), // TODO - is there a way to automate octet decoding without custom code? Or should this be a Stream?
                { Type: "string", Format: "date" } => new("string"), // TODO - make DateOnly available if target is .NET 6
                { Type: "string", Format: "date-time" } => new("global::System.DateTimeOffset"),
                { Type: "string", Format: "uuid" or "guid" } => new("global::System.Guid"),
                { Type: "string" } => new("string"),
                { Type: "boolean" } => new("bool"),
                { Type: "array", Items: OpenApiSchema items } => new($"global::System.Collections.Generic.IEnumerable<{ToInlineDataType(items, true).text}>", isEnumerable: true),
                _ => throw new NotSupportedException("Unkonwn schema"),
            };
            return (schema is { Nullable: true } || !required)
                ? result.MakeNullable()
                : result;
        }

        public string UseReferenceName(OpenApiSchema schema)
        {
            return CSharpNaming.ToClassName(schema.Reference.Id);
        }

        public SourceEntry TransformSchema(OpenApiSchema schema)
        {
            var targetNamespace = baseNamespace;
            var className = CSharpNaming.ToClassName(schema.Reference.Id);

            var header = new templates.PartialHeader(
                appName: document.Info.Title,
                appDescription: document.Info.Description,
                version: document.Info.Version,
                infoEmail: document.Info.Contact?.Email
            );
            var entry = HandlebarsTemplateProcess.ProcessModel(
                header: header,
                packageName: targetNamespace,
                model: schema switch
                {
                    { Enum: { Count: > 0 }, Type: "string" } => ToEnumModel(className, schema),
                    _ => BuildObjectModel(schema) switch
                    {
                        ObjectModel model => ToObjectModel(className, schema, model),
                        _ => throw new NotSupportedException()
                    }
                },
                handlebars.Value
            );
            return new SourceEntry
            {
                Key = $"{targetNamespace}.{className}.cs",
                SourceText = entry,
            };
        }

        private templates.ObjectModel ToObjectModel(string className, OpenApiSchema schema, ObjectModel objectModel)
        {
            if (objectModel == null)
                throw new ArgumentNullException(nameof(objectModel));
            var properties = objectModel.properties();
            var required = new HashSet<string>(objectModel.required());

            return new templates.ObjectModel(
                description: schema.Description,
                className: className,
                parent: null, // TODO
                vars: (from entry in properties
                       let req = required.Contains(entry.Key)
                       let dataType = ToInlineDataType(entry.Value, req)
                       select new templates.ModelVar(
                           baseName: entry.Key,
                           dataType: dataType.text,
                           nullable: dataType.nullable,
                           isContainer: dataType.isEnumerable,
                           name: CSharpNaming.ToPropertyName(entry.Key),
                           required: req
                        )).ToArray()
            );
        }

        private templates.EnumModel ToEnumModel(string className, OpenApiSchema schema)
        {
            return new templates.EnumModel(
                schema.Description,
                className,
                isString: schema.Type == "string",
                enumVars: (from entry in schema.Enum
                           select entry switch
                           {
                               Microsoft.OpenApi.Any.OpenApiPrimitive<string> { Value: string name } => new templates.EnumVar(CSharpNaming.ToPropertyName(name), name),
                               _ => throw new NotSupportedException()
                           }).ToArray()
            );
        }

        record ObjectModel(Func<IDictionary<string, OpenApiSchema>> properties, Func<IEnumerable<string>> required);

        private ObjectModel? BuildObjectModel(OpenApiSchema schema) =>
            schema switch
            {
                { AllOf: { Count: > 0 } } => schema.AllOf.Select(BuildObjectModel).ToArray() switch
                {
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
    }
}