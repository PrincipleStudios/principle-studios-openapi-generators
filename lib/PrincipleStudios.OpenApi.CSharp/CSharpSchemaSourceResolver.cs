using HandlebarsDotNet;
using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.CSharp.Templates;
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
				{ Type: "object", Properties: { Count: 0 }, AdditionalProperties: OpenApiSchema _ } => false,
				{ UnresolvedReference: true, Reference: { IsExternal: false } } => ProduceSourceEntry((OpenApiSchema)GetApiContexts(schema).First().Reverse().Select(e => e.Element).OfType<OpenApiDocument>().Last().ResolveReference(schema.Reference)),
				{ UnresolvedReference: true } => throw new ArgumentException("Unable to resolve reference"),
				{ AllOf: { Count: > 1 } } => true,
				{ AnyOf: { Count: > 1 } } => true,
				{ OneOf: { Count: > 1 } } => true,
				{ Type: "string", Enum: { Count: > 0 } } => true,
				{ Type: "array", Items: OpenApiSchema inner } => false,
				{ Type: string type, Format: var format, Properties: { Count: 0 }, Enum: { Count: 0 } } => options.Find(type, format) == "object",
				{ Type: "object", Format: null } => true,
				{ Properties: { Count: > 1 } } => true,
				{ Type: "string" or "number" or "integer" or "boolean" } => false,
				_ => throw new NotSupportedException("Unknown schema"),
			};
		}

		public string UseReferenceName(OpenApiSchema schema)
		{
			return CSharpNaming.ToClassName(schema.Reference.Id, options.ReservedIdentifiers());
		}

		public SourceEntry? TransformSchema(OpenApiSchema schema, OpenApiTransformDiagnostic diagnostic)
		{
			var targetNamespace = baseNamespace;
			var context = GetBestContext(GetApiContexts(schema));
			var info = context.Select(v => v.Element).OfType<OpenApiDocument>().Last().Info;
			var className = GetClassName(schema);

			var header = new Templates.PartialHeader(
				AppName: info.Title,
				AppDescription: info.Description,
				Version: info.Version,
				InfoEmail: info.Contact?.Email,
				CodeGeneratorVersionInfo: versionInfo
			);
			Templates.Model? model = schema switch
			{
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

		private string GetClassName(OpenApiSchema schema)
		{
			var context = GetBestContext(GetApiContexts(schema));
			return CSharpNaming.ToClassName(schema.Reference?.Id ?? ContextToIdentifier(context), options.ReservedIdentifiers());
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
												   Name: CSharpNaming.ToPropertyName(entry.Key, options.ReservedIdentifiers("object", className)),
												   Required: req,
												   Optional: !req && !objectModel.LegacyOptionalBehavior,
												   Pattern: entry.Value.Pattern,
												   MinLength: entry.Value.MinLength,
												   MaxLength: entry.Value.MaxLength,
												   Minimum: entry.Value.Minimum,
												   Maximum: entry.Value.Maximum
												))).ToArray();

			return () => new Templates.ObjectModel(
				Description: schema.Description,
				ClassName: className,
				Parent: null, // TODO - if "all of" and only one was a reference, we should be able to use inheritance.
				Vars: vars.Select(v => v()).ToArray()
			);
		}

		private Templates.EnumModel ToEnumModel(string className, OpenApiSchema schema)
		{
			return new Templates.EnumModel(
				schema.Description,
				className,
				IsString: schema.Type == "string",
				EnumVars: (from entry in schema.Enum
						   select entry switch
						   {
							   Microsoft.OpenApi.Any.OpenApiPrimitive<string> { Value: string name } => new Templates.EnumVar(CSharpNaming.ToPropertyName(name, options.ReservedIdentifiers("enum", className)), name),
							   _ => throw new NotSupportedException()
						   }).ToArray()
			);
		}

		private Templates.TypeUnionModel ToOneOfModel(string className, OpenApiSchema schema)
		{
			return new Templates.TypeUnionModel(
				schema.Description,
				className,
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
						return new TypeUnionEntry(
							TypeName: ToInlineDataType(e)().text,
							Identifier: CSharpNaming.ToPropertyName(id, options.ReservedIdentifiers("object", className)),
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
				{ Type: "object" } or { Properties: { Count: > 0 } } =>
					new ObjectModel(
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
					? TransformSchema(schema, diagnostic)
					: null;
		}

		protected override InlineDataType GetInlineDataType(OpenApiSchema schema)
		{
			InlineDataType result = schema switch
			{
				{ Type: "object", Properties: { Count: 0 }, AdditionalProperties: OpenApiSchema dictionaryValueSchema } =>
					new(options.ToMapType(ToInlineDataType(dictionaryValueSchema)().text), isEnumerable: true),
				{ Type: "array", Items: OpenApiSchema items } =>
					new(options.ToArrayType(ToInlineDataType(items)().text), isEnumerable: true),
				{ Type: string type, Format: var format } when !ProduceSourceEntry(schema) && options.Find(type, format) != "object" =>
					new(options.Find(type, format)),
				{ Reference: not null } =>
					new(UseReferenceName(schema)),
				_ when ProduceSourceEntry(schema) =>
					new(GetClassName(schema)),
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