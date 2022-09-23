using HandlebarsDotNet;
using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PrincipleStudios.OpenApi.TypeScript
{
    public record ImportReference(OpenApiSchema Schema, string Member, string File);
    public record InlineDataType(string text, IReadOnlyList<ImportReference> Imports, bool nullable = false, bool isEnumerable = false)
    {
        public InlineDataType MakeNullable() =>
            nullable ? this : new(text + " | null", Imports, nullable: true, isEnumerable: isEnumerable);
    }

    public class TypeScriptSchemaSourceResolver : SchemaSourceResolver<InlineDataType>
    {
        private readonly TypeScriptSchemaOptions options;
        private readonly HandlebarsFactory handlebarsFactory;
        private readonly string versionInfo;


        public TypeScriptSchemaSourceResolver(TypeScriptSchemaOptions options, HandlebarsFactory handlebarsFactory, string versionInfo)
        {
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

        public bool ProduceSourceEntry(OpenApiSchema schema)
        {
            // C# can't inline things that must be referenced, and vice versa.
            // (Except with tuples, but those don't serialize/deserialize reliably yet.)
            return schema switch
            {
                { Reference: null } => false,
                { UnresolvedReference: false } => true,
                { UnresolvedReference: true, Reference: { IsExternal: false } } => ProduceSourceEntry((OpenApiSchema)GetApiContexts(schema).First().Reverse().Select(e => e.Element).OfType<OpenApiDocument>().Last().ResolveReference(schema.Reference)),
                { UnresolvedReference: true } => throw new ArgumentException("Unable to resolve reference"),
            };
        }

        public string UseReferenceName(OpenApiSchema schema)
        {
            return TypeScriptNaming.ToClassName(schema.Reference.Id, options.ReservedIdentifiers());
        }

        public string ToSourceEntryKey(OpenApiSchema schema)
        {
            var className = UseReferenceName(schema);
            return $"models/{className}.ts";
        }

        public ImportReference ToImportReference(OpenApiSchema schema)
        {
            return new ImportReference(schema, UseReferenceName(schema), ToSourceEntryKey(schema));
        }

        public SourceEntry? TransformSchema(OpenApiSchema schema, OpenApiContext context, OpenApiTransformDiagnostic diagnostic)
        {
            var info = context.Select(v => v.Element).OfType<OpenApiDocument>().Last().Info;
            var className = UseReferenceName(schema);

            var header = new templates.PartialHeader(
                appName: info.Title,
                appDescription: info.Description,
                version: info.Version,
                infoEmail: info.Contact?.Email,
                codeGeneratorVersionInfo: versionInfo
            );
            templates.Model? model = schema switch
            {
                { Items: OpenApiSchema arrayItem, Type: "array" } => ToArrayModel(className, schema),
                { Enum: { Count: > 0 }, Type: "string" } => ToEnumModel(className, schema),
                _ => BuildObjectModel(schema) switch
                {
                    ObjectModel objectModel => ToObjectModel(className, schema, context, objectModel, diagnostic)(),
                    _ => null
                }
            };
            if (model == null)
                return null;
            var entry = HandlebarsTemplateProcess.ProcessModel(
                header: header,
                packageName: "",
                model: model,
                handlebarsFactory.Handlebars
            );
            return new SourceEntry
            {
                Key = ToSourceEntryKey(schema),
                SourceText = entry,
            };
        }

        protected readonly Regex _2xxRegex = new Regex("2[0-9]{2}");
        protected virtual string ContextToIdentifier(OpenApiContext context)
        {
            var (parts, remaining) = Simplify(context.Entries);
            while (remaining.Length > 0)
            {
                IEnumerable<string> newParts;
                (newParts, remaining) = Simplify(remaining);
                parts = parts.Concat(newParts).ToArray();
            }

            return string.Join(" ", parts);

            (IEnumerable<string> parts, OpenApiContextEntry[] remaining) Simplify(IReadOnlyList<OpenApiContextEntry> context)
            {
                return context[0] switch
                {
                    { Element: OpenApiDocument _ or OpenApiPaths _ or OpenApiPathItem _ or OpenApiComponents _ } => (Enumerable.Empty<string>(), context.Skip(1).ToArray()),
                    { Element: OpenApiOperation operation } => (new[] { operation.OperationId }, context.Skip(1).ToArray()),
                    { Property: "Responses", Element: OpenApiResponses responses } =>
                        (context[1], context[2], context[3]) switch
                        {
                            ({ Key: string statusCode, Element: OpenApiResponse response }, { Property: "Content", Key: string mimeType }, { Property: "Schema" }) =>
                                (
                                    new[] {
                                        responses.Count == 1 ? ""
                                            : _2xxRegex.IsMatch(statusCode) && responses.Keys.Count(_2xxRegex.IsMatch) == 1 ? ""
                                            : statusCode == "default" && !responses.ContainsKey("other") ? "other"
                                            : int.TryParse(statusCode, out var numeric) && HttpStatusCodes.StatusCodeNames.TryGetValue(numeric, out var statusCodeName) ? statusCodeName
                                            : statusCode,
                                        response.Content.Count == 1 ? ""
                                            : mimeType,
                                        "response"
                                    },
                                    context.Skip(4).ToArray()
                                ),
                            ({ Key: string statusCode, Element: OpenApiResponse response }, { Property: "Headers", Key: string headerName }, { Property: "Schema" }) =>
                                (
                                    new[] {
                                        responses.Count == 1 ? ""
                                            : _2xxRegex.IsMatch(statusCode) && responses.Keys.Count(_2xxRegex.IsMatch) == 1 ? ""
                                            : statusCode == "default" && !responses.ContainsKey("other") ? "other"
                                            : statusCode,
                                        headerName,
                                        "response header"
                                    },
                                    context.Skip(4).ToArray()
                                ),
                            _ => throw new NotImplementedException(),
                        },
                    { Property: "RequestBody" or "RequestBodies", Key: var namedRequest, Element: OpenApiRequestBody requestBody } =>
                         (context[1], context[2]) switch
                         {
                             ({ Property: "Content", Key: string mimeType }, { Property: "Schema" }) =>
                                 (
                                     new[] {
                                         namedRequest ?? "",
                                        requestBody.Content.Count == 1 ? ""
                                            : mimeType,
                                        "request"
                                     },
                                     context.Skip(3).ToArray()
                                 ),
                             _ => throw new NotImplementedException(),
                         },
                    { Property: "Parameters", Key: string, Element: OpenApiParameter parameter } =>
                         context[1] switch
                         {
                             { Property: "Schema" } =>
                                 (
                                     new[] { parameter.Name },
                                     context.Skip(2).ToArray()
                                 ),
                             _ => throw new NotImplementedException(),
                         },
                    { Property: "Items", Element: OpenApiSchema _ } => (new[] { "Item" }, context.Skip(1).ToArray()),
                    { Property: "AdditionalProperties", Element: OpenApiSchema _ } => (new[] { "AdditionalProperty" }, context.Skip(1).ToArray()),
                    { Key: string key, Element: OpenApiSchema _ } => (new[] { key }, context.Skip(1).ToArray()),
                    _ => throw new NotImplementedException(),
                };
            }
        }

        private Func<templates.ObjectModel> ToObjectModel(string className, OpenApiSchema schema, OpenApiContext context, ObjectModel objectModel, OpenApiTransformDiagnostic diagnostic)
        {
            if (objectModel == null)
                throw new ArgumentNullException(nameof(objectModel));
            var properties = objectModel.properties();
            var required = new HashSet<string>(objectModel.required());

            Func<templates.ModelVar>[] vars = (from entry in properties
                                               let req = required.Contains(entry.Key)
                                               let dataTypeBase = ToInlineDataType(entry.Value)
                                               let dataType = req ? dataTypeBase : () => dataTypeBase().MakeNullable()
                                               select (Func<templates.ModelVar>)(() => new templates.ModelVar(
                                                   baseName: entry.Key,
                                                   dataType: dataType().text,
                                                   nullable: dataType().nullable,
                                                   isContainer: dataType().isEnumerable,
                                                   name: entry.Key,
                                                   required: req
                                                ))).ToArray();


            return () => new templates.ObjectModel(
                imports: this.GetImportStatements(properties.Values, new[] { schema }, "./models/").ToArray(),
                description: schema.Description,
                className: className,
                parent: null, // TODO - if "all of" and only one was a reference, we should be able to use inheritance.
                vars: vars.Select(v => v()).ToArray()
            );
        }

        private templates.ArrayModel ToArrayModel(string className, OpenApiSchema schema)
        {
            var dataType = ToInlineDataType(schema.Items)();
            return new templates.ArrayModel(
                schema.Description,
                className,
                Item: dataType.text,
                Imports: this.GetImportStatements(new[] { schema.Items }, Enumerable.Empty<OpenApiSchema>(), "./models/").ToArray()
            );
        }

        private templates.EnumModel ToEnumModel(string className, OpenApiSchema schema)
        {
            return new templates.EnumModel(
                schema.Description,
                className,
                isString: schema.Type == "string",
                enumVars: (from entry in schema.Enum.OfType<Microsoft.OpenApi.Any.IOpenApiPrimitive>()
                           select new templates.EnumVar(PrimitiveToJsonValue.GetPrimitiveValue(entry))).ToArray()
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
                            properties: () => models.SelectMany(m => m!.properties()).Aggregate(new Dictionary<string, OpenApiSchema>(), (prev, kvp) =>
                            {
                                prev[kvp.Key] = kvp.Value;
                                return prev;
                            }),
                            required: () => models.SelectMany(m => m!.required()).Distinct()
                        ),
                    _ => null
                },
                { Type: "object" } or { Properties: { Count: > 0 } } => new ObjectModel(properties: () => schema.Properties, required: () => schema.Required),
                _ => null,
            };

        protected override SourceEntry? GetSourceEntry(OpenApiSchema schema, IEnumerable<OpenApiContext> allContexts, OpenApiTransformDiagnostic diagnostic)
        {
            var bestContext = GetBestContext(allContexts);
            if (bestContext.Any(c => c.Property is "AllOf"))
                return null;
            return ProduceSourceEntry(schema)
                    ? TransformSchema(schema, GetBestContext(allContexts), diagnostic)
                    : null;
        }

        protected override InlineDataType GetInlineDataType(OpenApiSchema schema)
        {
            InlineDataType result = schema switch
            {
                { Reference: not null } =>
                    new(UseReferenceName(schema), new[] { ToImportReference(schema) }),
                { Type: "object", Properties: { Count: 0 }, AdditionalProperties: OpenApiSchema dictionaryValueSchema } =>
                    DictionaryToInline(dictionaryValueSchema),
                { Type: "array", Items: OpenApiSchema items } =>
                    ArrayToInline(items),
                // TODO - better inline types
                _ when ProduceSourceEntry(schema) =>
                    new(UseReferenceName(schema), new[] { ToImportReference(schema) }),
                { Type: "object", Format: null, Properties: IDictionary<string, OpenApiSchema> properties, AdditionalProperties: null } =>
                    ObjectToInline(properties),
                { Enum: { Count: > 0 } and IList<Microsoft.OpenApi.Any.IOpenApiAny> enumValues } =>
                    EnumToInline(enumValues),
                { Type: string type, Format: var format } =>
                    TypeWithFormatToInline(type, format),
                _ => throw new NotSupportedException("Unknown schema"),
            };
            return schema is { Nullable: true }
                ? result.MakeNullable()
                : result;

            InlineDataType DictionaryToInline(OpenApiSchema dictionaryValueSchema)
            {
                var inline = ToInlineDataType(dictionaryValueSchema)();
                return new(options.ToMapType(inline.text), inline.Imports, isEnumerable: true);
            }
            InlineDataType ArrayToInline(OpenApiSchema items)
            {
                var inline = ToInlineDataType(items)();
                return new(options.ToArrayType(inline.text), inline.Imports, isEnumerable: true);
            }
            InlineDataType ObjectToInline(IDictionary<string, OpenApiSchema> properties)
            {
                var props = (from prop in properties
                             let inline = ToInlineDataType(prop.Value)()
                             select (
                                 text: $"\"{prop.Key}\": {inline.text}",
                                 imports: inline.Imports
                             )).ToArray();
                return new($"{{ { string.Join("; ", props.Select(p => p.text))} }}", props.SelectMany(p => p.imports).ToArray());
            }
            InlineDataType TypeWithFormatToInline(string type, string? format)
            {
                return new(options.Find(type, format), Array.Empty<ImportReference>());
            }
            InlineDataType EnumToInline(IEnumerable<Microsoft.OpenApi.Any.IOpenApiAny> enumValues)
            {
                return new(string.Join(" | ", enumValues.Select(PrimitiveToJsonValue.GetPrimitiveValue)), Array.Empty<ImportReference>());
            }
        }

        protected override InlineDataType UnresolvedReferencePlaceholder()
        {
            return new InlineDataType("unknown", Array.Empty<ImportReference>(), false, false);
        }

        protected virtual OpenApiContext GetBestContext(IEnumerable<OpenApiContext> allContexts)
        {
            return allContexts.OrderBy(c => c.Entries.Count).First();
        }
    }
}