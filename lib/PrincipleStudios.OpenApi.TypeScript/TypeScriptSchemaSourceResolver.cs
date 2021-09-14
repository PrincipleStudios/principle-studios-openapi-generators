using HandlebarsDotNet;
using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PrincipleStudios.OpenApi.TypeScript
{
    public record InlineDataType(string text, bool nullable = false, bool isEnumerable = false)
    {
        // Assumes C#8, since it's standard in VS2019+, which is when nullable reference types were introduced
        public InlineDataType MakeNullable() =>
            nullable ? this : new(text + "?", nullable: true, isEnumerable: isEnumerable);
    }

    public class TypeScriptSchemaSourceResolver : SchemaSourceResolver<InlineDataType>
    {
        private readonly string baseNamespace;
        private readonly TypeScriptSchemaOptions options;
        private readonly HandlebarsFactory handlebarsFactory;
        private readonly string versionInfo;


        public TypeScriptSchemaSourceResolver(string baseNamespace, TypeScriptSchemaOptions options, HandlebarsFactory handlebarsFactory, string versionInfo)
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

        public SourceEntry? TransformSchema(OpenApiSchema schema, OpenApiContext context, OpenApiTransformDiagnostic diagnostic)
        {
            var targetNamespace = baseNamespace;
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
                packageName: targetNamespace,
                model: model,
                handlebarsFactory.Handlebars
            );
            return new SourceEntry
            {
                Key = $"{targetNamespace}.{className}.cs",
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
                                            : int.TryParse(statusCode, out var numeric) ? ((System.Net.HttpStatusCode)numeric).ToString("g")
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
                description: schema.Description,
                className: className,
                parent: null, // TODO - if "all of" and only one was a reference, we should be able to use inheritance.
                vars: vars.Select(v => v()).ToArray()
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
                               Microsoft.OpenApi.Any.OpenApiPrimitive<string> { Value: string name } => new templates.EnumVar(TypeScriptNaming.ToPropertyName(name, options.ReservedIdentifiers("enum", className)), name),
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
                    new(UseReferenceName(schema)),
                { Type: "object", Properties: { Count: 0 }, AdditionalProperties: OpenApiSchema dictionaryValueSchema } =>
                    new(options.ToMapType(ToInlineDataType(dictionaryValueSchema)().text), isEnumerable: true),
                { Type: "array", Items: OpenApiSchema items } =>
                    new(options.ToArrayType(ToInlineDataType(items)().text), isEnumerable: true),
                // TODO - better inline types
                _ when ProduceSourceEntry(schema) =>
                    new(UseReferenceName(schema)),
                { Type: "object", Properties: IDictionary<string, OpenApiSchema> properties, AdditionalProperties: null } =>
                    new($"{{ { string.Join("; ", properties.Select(p => $"\"{p.Key}\": {ToInlineDataType(p.Value)().text}"))} }}"),
                { Type: string type, Format: var format } =>
                    new(options.Find(type, format)),
                _ => throw new NotSupportedException("Unknown schema"),
            };
            return schema is { Nullable: true }
                ? result.MakeNullable()
                : result;
        }

        protected override InlineDataType UnresolvedReferencePlaceholder()
        {
            return new InlineDataType("object", false, false);
        }

        protected virtual OpenApiContext GetBestContext(IEnumerable<OpenApiContext> allContexts)
        {
            return allContexts.OrderBy(c => c.Entries.Count).First();
        }
    }
}