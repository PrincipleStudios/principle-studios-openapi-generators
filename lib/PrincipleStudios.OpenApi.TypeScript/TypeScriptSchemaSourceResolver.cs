using HandlebarsDotNet;
using HandlebarsDotNet.Runtime;
using Microsoft.OpenApi.Interfaces;
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
		private Templates.PartialHeader? header;

		public TypeScriptSchemaSourceResolver(TypeScriptSchemaOptions options, HandlebarsFactory handlebarsFactory, string versionInfo)
		{
			this.options = options;
			this.handlebarsFactory = handlebarsFactory;
			this.versionInfo = versionInfo;
		}

		protected override IEnumerable<SourceEntry> GetAdditionalSources(IEnumerable<OpenApiSchema> referencedSchemas, OpenApiTransformDiagnostic diagnostic)
		{
			if (header == null) yield break;
			var exportStatements = this.GetExportStatements(referencedSchemas, options, "./models/").ToArray();
			if (exportStatements.Length > 0)
				yield return new SourceEntry()
				{
					Key = "models/index.ts",
					SourceText = HandlebarsTemplateProcess.ProcessModelBarrelFile(
						new Templates.ModelBarrelFile(header, exportStatements),
						handlebarsFactory.Handlebars
					),
				};
		}

		public override void EnsureSchemasRegistered(IOpenApiElement element, OpenApiContext context, OpenApiTransformDiagnostic diagnostic)
		{
			var info = context.Select(v => v.Element).OfType<OpenApiDocument>().Last().Info;

			header = new Templates.PartialHeader(
				AppName: info.Title,
				AppDescription: info.Description,
				Version: info.Version,
				InfoEmail: info.Contact?.Email,
				CodeGeneratorVersionInfo: versionInfo
			);

			base.EnsureSchemasRegistered(element, context, diagnostic);
		}

		public static bool MakeReference(OpenApiSchema schema)
		{
			return schema switch
			{
				{ Reference: not null, UnresolvedReference: false } => false,
				{ Type: "string", Enum: { Count: > 0 } } => true,
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

			var header = new Templates.PartialHeader(
				AppName: info.Title,
				AppDescription: info.Description,
				Version: info.Version,
				InfoEmail: info.Contact?.Email,
				CodeGeneratorVersionInfo: versionInfo
			);
			Templates.Model? model = schema switch
			{
				{ Items: OpenApiSchema arrayItem, Type: "array" } => ToArrayModel(className, schema),
				{ Enum: { Count: > 0 }, Type: "string" } => ToEnumModel(className, schema),
				{ OneOf: { Count: > 0 } } => ToOneOfModel(className, schema),
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

#pragma warning disable CA1707 // Identifiers should not contain underscores
		protected static readonly Regex _2xxRegex = new Regex("2[0-9]{2}");
#pragma warning restore CA1707 // Identifiers should not contain underscores

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

		private Func<Templates.ObjectModel> ToObjectModel(string className, OpenApiSchema schema, OpenApiContext context, ObjectModel objectModel, OpenApiTransformDiagnostic diagnostic)
		{
			if (objectModel == null)
				throw new ArgumentNullException(nameof(objectModel));
			var properties = objectModel.Properties();
			var required = new HashSet<string>(objectModel.Required());

			Func<Templates.ModelVar>[] vars = (from entry in properties
											   let req = required.Contains(entry.Key)
											   let dataType = ToInlineDataType(entry.Value)
											   let resolved = objectModel.LegacyOptionalBehavior && !req ? dataType().MakeNullable() : dataType()
											   select (Func<Templates.ModelVar>)(() => new Templates.ModelVar(
												   BaseName: entry.Key,
												   DataType: resolved.text,
												   Nullable: resolved.nullable,
												   IsContainer: resolved.isEnumerable,
												   Name: entry.Key,
												   Required: req,
												   Optional: !req
												))).ToArray();

			return () => new Templates.ObjectModel(
				Imports: this.GetImportStatements(properties.Values, new[] { schema }, "./models/").ToArray(),
				Description: schema.Description,
				ClassName: className,
				Parent: null, // TODO - if "all of" and only one was a reference, we should be able to use inheritance.
				Vars: vars.Select(v => v()).ToArray()
			);
		}

		private Templates.ArrayModel ToArrayModel(string className, OpenApiSchema schema)
		{
			var dataType = ToInlineDataType(schema.Items)();
			return new Templates.ArrayModel(
				schema.Description,
				className,
				Item: dataType.text,
				Imports: this.GetImportStatements(new[] { schema.Items }, Enumerable.Empty<OpenApiSchema>(), "./models/").ToArray()
			);
		}

		private Templates.EnumModel ToEnumModel(string className, OpenApiSchema schema)
		{
			return new Templates.EnumModel(
				schema.Description,
				className,
				TypeScriptNaming.ToPropertyName(className, options.ReservedIdentifiers()),
				IsString: schema.Type == "string",
				EnumVars: (from entry in schema.Enum.OfType<Microsoft.OpenApi.Any.IOpenApiPrimitive>()
						   select new Templates.EnumVar(PrimitiveToJsonValue.GetPrimitiveValue(entry))).ToArray()
			);
		}

		private Templates.TypeUnionModel ToOneOfModel(string className, OpenApiSchema schema)
		{
			return new Templates.TypeUnionModel(
				Imports: this.GetImportStatements(schema.OneOf, Enumerable.Empty<OpenApiSchema>(), "./models/").ToArray(),
				Description: schema.Description,
				ClassName: className,
				AllowAnyOf: false,
				DiscriminatorProperty: schema.Discriminator?.PropertyName,
				TypeEntries: schema.OneOf
					.Select((e, index) =>
					{
						var id = e.Reference?.Id.Split('/').Last() ?? throw new NotSupportedException("When using the discriminator, inline schemas will not be considered.") { HelpLink = "https://github.com/OAI/OpenAPI-Specification/blob/main/versions/3.0.2.md#discriminator-object" };
						if (schema.Discriminator?.Mapping?.FirstOrDefault(kvp => kvp.Value == id) is { Key: var mapped })
						{
							id = mapped;
						}
						return new Templates.TypeUnionEntry(
							TypeName: ToInlineDataType(e)().text,
							DiscriminatorValue: schema.Discriminator == null ? null : id
						);
					}).ToArray()
			);
		}

		record ObjectModel(Func<IDictionary<string, OpenApiSchema>> Properties, Func<IEnumerable<string>> Required, bool LegacyOptionalBehavior);

		private ObjectModel? BuildObjectModel(OpenApiSchema schema) =>
			schema switch
			{
				{ AllOf: { Count: > 0 } } => schema.AllOf.Select(BuildObjectModel).ToArray() switch
				{
					ObjectModel[] models when models.All(v => v != null) =>
						new ObjectModel(
							Properties: () => models.SelectMany(m => m!.Properties()).Aggregate(new Dictionary<string, OpenApiSchema>(), (prev, kvp) =>
							{
								prev[kvp.Key] = kvp.Value;
								return prev;
							}),
							Required: () => models.SelectMany(m => m!.Required()).Distinct(),
							LegacyOptionalBehavior: models.Any(m => m!.LegacyOptionalBehavior)
						),
					_ => null
				},
				{ Type: "object" } or { Properties: { Count: > 0 } } => new ObjectModel(
					Properties: () => schema.Properties,
					Required: () => schema.Required,
					LegacyOptionalBehavior: schema.UseOptionalAsNullable()
				),
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
				{ Type: "object", Format: null, Properties: IDictionary<string, OpenApiSchema> properties, AdditionalProperties: null, Required: var requiredProperties } =>
					ObjectToInline(properties, requiredProperties, schema.UseOptionalAsNullable()),
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
			InlineDataType ObjectToInline(IDictionary<string, OpenApiSchema> properties, ISet<string> requiredProperties, bool useOptionalAsNullable)
			{
				var props = (from prop in properties
							 let inline = ToInlineDataType(prop.Value)()
							 let resolved = useOptionalAsNullable && !requiredProperties.Contains(prop.Key) ? inline.MakeNullable() : inline
							 let optional = !requiredProperties.Contains(prop.Key)
							 select (
								 text: $"\"{prop.Key}\"{(optional ? "?" : "")}: {resolved.text}",
								 imports: resolved.Imports
							 )).ToArray();
				return new($"{{ {string.Join("; ", props.Select(p => p.text))} }}", props.SelectMany(p => p.imports).ToArray());
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