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
        // Assumes C#8, since it's standard in VS2019+, which is when nullable reference types were introduced
        public InlineDataType MakeNullable() =>
            nullable ? this : new(text + "?", nullable: true, isEnumerable: isEnumerable);
    }

    public class CSharpSchemaSourceResolver : SchemaSourceResolver<InlineDataType>
    {
        private readonly string baseNamespace;
        private readonly CSharpSchemaOptions options;
        private readonly HandlebarsFactory handlebarsFactory;
        private readonly string versionInfo;


        public CSharpSchemaSourceResolver(string baseNamespace, CSharpSchemaOptions options, HandlebarsFactory handlebarsFactory, string versionInfo)
        {
            this.baseNamespace = baseNamespace;
            this.options = options;
            this.handlebarsFactory = handlebarsFactory;
            this.versionInfo = versionInfo;
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
                { Type: "object" } => true,
                _ => throw new NotSupportedException("Unknown schema"),
            };
        }

        public bool UseInline(OpenApiSchema schema)
        {
            // C# can't inline things that must be referenced, and vice versa.
            // (Except with tuples, but those don't serialize/deserialize reliably yet.)
            return schema switch
            {
                { Type: "object", Properties: { Count: 0 }, AdditionalProperties: OpenApiSchema _ } => true,
                { UnresolvedReference: true } => throw new ArgumentException("Unable to resolve reference"),
                { AllOf: { Count: > 1 } } => false,
                { AnyOf: { Count: > 1 } } => false,
                { Type: "string", Enum: { Count: > 1 } } => false,
                { Type: "object" } => false,
                { Properties: { Count: > 1 } } => false,
                { Type: "string" or "number" or "integer" or "boolean" } => true,
                { Type: "array", Items: OpenApiSchema inner } => UseInline(inner),
                _ => throw new NotSupportedException("Unknown schema"),
            };
        }

        protected InlineDataType ToInlineDataType(OpenApiSchema schema, OpenApiTransformDiagnostic diagnostic)
        {
            // TODO: Allow configuration of this
            // from https://github.com/OAI/OpenAPI-Specification/blob/main/versions/3.0.3.md#data-types
            InlineDataType result = schema switch
            {
                { Reference: not null } => new(UseReferenceName(schema)),
                { Type: "object", Properties: { Count: 0 }, AdditionalProperties: OpenApiSchema dictionaryValueSchema } => new(options.ToMapType(ToInlineDataType(dictionaryValueSchema, diagnostic).text), isEnumerable: true),
                { Type: "array", Items: OpenApiSchema items } => new(options.ToArrayType(ToInlineDataType(items, diagnostic).text), isEnumerable: true),
                { Type: string type, Format: var format } => new(options.Find(type, format)),
                _ => throw new NotSupportedException("Unknown schema"),
            };
            return schema is { Nullable: true }
                ? result.MakeNullable()
                : result;
        }

        public string UseReferenceName(OpenApiSchema schema)
        {
            return CSharpNaming.ToClassName(schema.Reference.Id, options.ReservedIdentifiers);
        }

        public SourceEntry? TransformSchema(OpenApiSchema schema, OpenApiContext context, OpenApiTransformDiagnostic diagnostic)
        {
            var targetNamespace = baseNamespace;
            var info = context.Select(v => v.Element).OfType<OpenApiDocument>().Last().Info;
            // TODO - use context if Reference.Id is null
            var className = CSharpNaming.ToClassName(schema.Reference?.Id ?? ContextToIdentifier(context), options.ReservedIdentifiers);

            var header = new templates.PartialHeader(
                appName: info.Title,
                appDescription: info.Description,
                version: info.Version,
                infoEmail: info.Contact?.Email,
                codeGeneratorVersionInfo: versionInfo
            );
            var model = BuildObjectModel(schema);
            if (model == null)
                return null;
            var entry = HandlebarsTemplateProcess.ProcessModel(
                header: header,
                packageName: targetNamespace,
                model: schema switch
                {
                    { Enum: { Count: > 0 }, Type: "string" } => ToEnumModel(className, schema),
                    _ => model switch
                    {
                        ObjectModel objectModel => ToObjectModel(className, schema, objectModel, diagnostic),
                        _ => throw new NotSupportedException()
                    }
                },
                handlebarsFactory.Handlebars
            );
            return new SourceEntry
            {
                Key = $"{targetNamespace}.{className}.cs",
                SourceText = entry,
            };
        }

        private readonly Regex _2xxRegex = new Regex("2[0-9]{2}");
        private string ContextToIdentifier(OpenApiContext context)
        {
            var (parts, remaining) = Simplify(context.Entries);
            while (remaining.Length > 0)
            {
                string[] newParts;
                (newParts, remaining) = Simplify(remaining);
                parts = parts.Concat(newParts).ToArray();
            }

            return string.Join(" ", parts);

            (string[] parts, OpenApiContextEntry[] remaining) Simplify(IReadOnlyList<OpenApiContextEntry> context)
            {
                if (context.Skip(1).FirstOrDefault(e => e.Element is OpenApiOperation) is { Element: OpenApiOperation newOperation })
                {
                    return (new[] { newOperation.OperationId }, context.SkipWhile(e => e.Element != newOperation).ToArray());
                }
                if (context[0] is { Element: OpenApiOperation operation })
                {
                    if (context[1].Key == "Responses" && context[2] is { Element: OpenApiResponse response, Key: string responseKey } && context[3].Key == "Content" && context[4] is { Key: string mimeType, Element: OpenApiMediaType _ })
                        return (
                            new[] {
                                operation.Responses.Count == 1 ? ""
                                    : _2xxRegex.IsMatch(responseKey) && operation.Responses.Keys.Count(_2xxRegex.IsMatch) == 1 ? ""
                                    : responseKey == "default" && !operation.Responses.ContainsKey("other") ? "other"
                                    : responseKey,
                                response.Content.Count == 1 ? ""
                                    : mimeType,
                                "response"
                            },
                            context.Skip(6).ToArray()
                        );
                    if (context[1].Key == "RequestBody" && context[3] is { Key: string requestType, Element: OpenApiMediaType _ })
                        return (
                            new[] { (operation.RequestBody?.Content.Count ?? 0) == 1 ? "" : requestType, "request" },
                            context.Skip(5).ToArray()
                        );
                    if (context[1].Key == "Parameters" && context[2] is { Element: OpenApiParameter { Name: string paramName } })
                        return (
                            new[] { paramName },
                            context.Skip(4).ToArray()
                        );
                }

                throw new NotImplementedException();
            }
        }

        private templates.ObjectModel ToObjectModel(string className, OpenApiSchema schema, ObjectModel objectModel, OpenApiTransformDiagnostic diagnostic)
        {
            if (objectModel == null)
                throw new ArgumentNullException(nameof(objectModel));
            var properties = objectModel.properties();
            var required = new HashSet<string>(objectModel.required());

            return new templates.ObjectModel(
                description: schema.Description,
                className: className,
                parent: null, // TODO - if "all of" and only one was a reference, we should be able to use inheritance.
                vars: (from entry in properties
                       let req = required.Contains(entry.Key)
                       let dataTypeBase = ToInlineDataType(entry.Value, diagnostic)
                       let dataType = req ? dataTypeBase : dataTypeBase.MakeNullable()
                       select new templates.ModelVar(
                           baseName: entry.Key,
                           dataType: dataType.text,
                           nullable: dataType.nullable,
                           isContainer: dataType.isEnumerable,
                           name: CSharpNaming.ToPropertyName(entry.Key, options.ReservedIdentifiers),
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
                               Microsoft.OpenApi.Any.OpenApiPrimitive<string> { Value: string name } => new templates.EnumVar(CSharpNaming.ToPropertyName(name, options.ReservedIdentifiers), name),
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
                { Type: "object" } or { Properties: { Count: > 0 } } => new ObjectModel(properties: () => schema.Properties, required: () => schema.Required),
                _ => null,
            };

        protected override SchemaSourceEntry ToInlineDataTypeWithReference(OpenApiSchema schema, OpenApiContext context, OpenApiTransformDiagnostic diagnostic)
        {
            // TODO - should probably defer the inline data type for the shortest context
            return new SchemaSourceEntry
            {
                Inline = ToInlineDataType(schema, diagnostic),
                SourceEntry = !UseInline(schema) ? TransformSchema(schema, context, diagnostic) : null,
            };
        }

        protected override InlineDataType UnresolvedReferencePlaceholder()
        {
            return new InlineDataType("object", false, false);
        }
    }
}