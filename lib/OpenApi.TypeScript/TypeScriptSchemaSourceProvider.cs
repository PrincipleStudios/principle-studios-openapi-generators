
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using PrincipleStudios.OpenApi.Transformations;
using PrincipleStudios.OpenApi.Transformations.Abstractions;
using PrincipleStudios.OpenApi.Transformations.Specifications;
using PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Applicator;
using PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Metadata;
using PrincipleStudios.OpenApi.Transformations.Specifications.Keywords.Draft2020_12Validation;
using PrincipleStudios.OpenApi.TypeScript.Templates;
using PrincipleStudios.OpenApiCodegen;

namespace PrincipleStudios.OpenApi.TypeScript;

public class TypeScriptSchemaSourceProvider : SchemaSourceProvider
{
	private readonly DocumentRegistry documentRegistry;
	private readonly ISchemaRegistry schemaRegistry;
	private readonly TypeScriptInlineSchemas inlineSchemas;
	private readonly string baseNamespace;
	private readonly TypeScriptSchemaOptions options;
	private readonly HandlebarsFactory handlebarsFactory;
	private readonly PartialHeader header;

	public TypeScriptSchemaSourceProvider(DocumentRegistry documentRegistry, ISchemaRegistry schemaRegistry, string baseNamespace, TypeScriptSchemaOptions options, HandlebarsFactory handlebarsFactory, Templates.PartialHeader header) : base(schemaRegistry)
	{
		this.documentRegistry = documentRegistry;
		this.schemaRegistry = schemaRegistry;
		this.inlineSchemas = new TypeScriptInlineSchemas(options, documentRegistry);
		this.baseNamespace = baseNamespace;
		this.options = options;
		this.handlebarsFactory = handlebarsFactory;
		this.header = header;
	}

	protected override SourceEntry? GetSourceEntry(JsonSchema entry, OpenApiTransformDiagnostic diagnostic)
	{
		if (!inlineSchemas.ProduceSourceEntry(entry)) return null;
		return TransformSchema(entry, diagnostic);
	}

	public SourceEntry? TransformSchema(JsonSchema schema, OpenApiTransformDiagnostic diagnostic)
	{
		var className = UseReferenceName(schema);

		var typeInfo = TypeScriptTypeInfo.From(schema);
		Templates.Model? model = typeInfo switch
		{
			{ Items: JsonSchema arrayItem, Type: "array" } => ToArrayModel(className, typeInfo),
			{ Enum: { Count: > 0 }, Type: "string" } => ToEnumModel(className, typeInfo),
			{ OneOf: { Count: > 0 } } => ToOneOfModel(className, typeInfo),
			_ => BuildObjectModel(schema) switch
			{
				ObjectModel objectModel => ToObjectModel(className, schema, objectModel, diagnostic)(),
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
		return new SourceEntry(
			Key: ToSourceEntryKey(schema),
			SourceText: entry
		);
	}

	public string UseReferenceName(JsonSchema schema)
	{
		return TypeScriptNaming.ToClassName(inlineSchemas.UriToClassIdentifier(schema.Metadata.Id), options.ReservedIdentifiers());
	}

	public string ToSourceEntryKey(JsonSchema schema)
	{
		var className = UseReferenceName(schema);
		return $"models/{className}.ts";
	}

	private Templates.ArrayModel ToArrayModel(string className, TypeScriptTypeInfo schema)
	{
		var dataType = inlineSchemas.ToInlineDataType(schema.Items);
		return new Templates.ArrayModel(
			schema.Description,
			className,
			Item: dataType.Text,
			Imports: inlineSchemas.GetImportStatements([schema.Items], Enumerable.Empty<JsonSchema>(), "./models/").ToArray()
		);
	}

	private Templates.EnumModel ToEnumModel(string className, TypeScriptTypeInfo schema)
	{
		return new Templates.EnumModel(
			schema.Description,
			className,
			TypeScriptNaming.ToPropertyName(className, options.ReservedIdentifiers()),
			IsString: schema.Type == "string",
			EnumVars: (from entry in schema.Enum
					   select new Templates.EnumVar(PrimitiveToJsonValue.GetPrimitiveValue(entry))).ToArray()
		);
	}

	private Templates.TypeUnionModel ToOneOfModel(string className, TypeScriptTypeInfo schema)
	{
		var discriminator = schema.Schema?.TryGetAnnotation<Transformations.Specifications.OpenApi3_0.DiscriminatorKeyword>();
		return new Templates.TypeUnionModel(
			Imports: inlineSchemas.GetImportStatements(schema.OneOf ?? Enumerable.Empty<JsonSchema>(), Enumerable.Empty<JsonSchema>(), "./models/").ToArray(),
			Description: schema.Description,
			ClassName: className,
			AllowAnyOf: false,
			DiscriminatorProperty: discriminator?.PropertyName,
			TypeEntries: schema.OneOf
				.Select((e, index) =>
				{
					var id = e.Metadata.Id;
					string? discriminatorValue = e.GetLastContextPart();
					if (discriminator?.Mapping?.FirstOrDefault(kvp => kvp.Value.OriginalString == id.OriginalString) is { Key: string key, Value: var relativeId })
					{
						discriminatorValue = key;
						id = new Uri(id, relativeId);
					}

					return new Templates.TypeUnionEntry(
						TypeName: inlineSchemas.ToInlineDataType(e).Text,
						DiscriminatorValue: discriminatorValue
					);
				}).ToArray()
		);
	}

	record ObjectModel(Func<IReadOnlyDictionary<string, JsonSchema>> Properties, Func<IEnumerable<string>> Required, bool LegacyOptionalBehavior);

	private ObjectModel? BuildObjectModel(JsonSchema schema) =>
		TypeScriptTypeInfo.From(schema) switch
		{
			{ AllOf: { Count: > 0 } allOf } => allOf.Select(BuildObjectModel).ToArray() switch
			{
				ObjectModel[] models when models.All(v => v != null) =>
					new ObjectModel(
						Properties: () => models.SelectMany(m => m!.Properties()).Aggregate(new Dictionary<string, JsonSchema>(), (prev, kvp) =>
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
				Properties: () => schema.TryGetAnnotation<PropertiesKeyword>()?.Properties ?? new Dictionary<string, JsonSchema>(),
				Required: () => schema.TryGetAnnotation<RequiredKeyword>()?.RequiredProperties ?? Enumerable.Empty<string>(),
				LegacyOptionalBehavior: schema.UseOptionalAsNullable()
			),
			_ => null,
		};

	private Func<Templates.ObjectModel> ToObjectModel(string className, JsonSchema schema, ObjectModel objectModel, OpenApiTransformDiagnostic diagnostic)
	{
		if (objectModel == null)
			throw new ArgumentNullException(nameof(objectModel));
		var properties = objectModel.Properties();
		var required = new HashSet<string>(objectModel.Required());

		Func<Templates.ModelVar>[] vars = (from entry in properties
										   let req = required.Contains(entry.Key)
										   let dataType = inlineSchemas.ToInlineDataType(entry.Value)
										   let resolved = objectModel.LegacyOptionalBehavior && !req ? dataType.MakeNullable() : dataType
										   select (Func<Templates.ModelVar>)(() => new Templates.ModelVar(
											   BaseName: entry.Key,
											   DataType: resolved.Text,
											   Nullable: resolved.Nullable,
											   IsContainer: resolved.IsEnumerable,
											   Name: entry.Key,
											   Required: req,
											   Optional: !req
											))).ToArray();

		return () => new Templates.ObjectModel(
			Imports: inlineSchemas.GetImportStatements(properties.Values, new[] { schema }, "./models/").ToArray(),
			Description: schema.TryGetAnnotation<DescriptionKeyword>()?.Description,
			ClassName: className,
			Parent: null, // TODO - if "all of" and only one was a reference, we should be able to use inheritance.
			Vars: vars.Select(v => v()).ToArray()
		);
	}

}


public static class TypeScriptInlineSchemasExtensions
{
	// public static IEnumerable<Templates.ExportStatement> GetExportStatements(this TypeScriptInlineSchemas inlineSchemas, IEnumerable<OpenApiSchema> schemasReferenced, TypeScriptSchemaOptions options, string path)
	// {
	// 	// FIXME: this is very hacked together; this accesses the "inline" data type to determine what should be exported
	// 	return from entry in schemasReferenced
	// 		   let t = inlineSchemas.ToInlineDataType(entry)()
	// 		   from import in t.Imports
	// 		   from refName in new[] { new Templates.ExportMember(import.Member, IsType: true) }.Concat(GetAdditionalModuleMembers(t, entry, options))
	// 		   let fileName = import.File
	// 		   group refName by fileName into imports
	// 		   let nodePath = imports.Key.ToNodePath(path)
	// 		   orderby nodePath
	// 		   select new Templates.ExportStatement(imports.Distinct().OrderBy(a => a.MemberName).ToArray(), nodePath);
	// }


	public static IEnumerable<Templates.ImportStatement> GetImportStatements(this TypeScriptInlineSchemas inlineSchemas, IEnumerable<JsonSchema?> schemasReferenced, IEnumerable<JsonSchema?> excludedSchemas, string path)
	{
		return from entry in schemasReferenced.Except(excludedSchemas)
			   let t = inlineSchemas.ToInlineDataType(entry)
			   from import in t.Imports
			   where !excludedSchemas.Contains(import.Schema)
			   let refName = import.Member
			   let fileName = import.File
			   group refName by fileName into imports
			   let nodePath = imports.Key.ToNodePath(path)
			   orderby nodePath
			   select new Templates.ImportStatement(imports.Distinct().OrderBy(a => a).ToArray(), nodePath);
	}

	private static IEnumerable<Templates.ExportMember> GetAdditionalModuleMembers(TypeScriptInlineDefinition t, TypeScriptTypeInfo schema, TypeScriptSchemaOptions options)
	{
		switch (schema)
		{
			case { Enum: { Count: > 0 } }:
				yield return new Templates.ExportMember(
					TypeScriptNaming.ToPropertyName(t.Text, options.ReservedIdentifiers()),
					IsType: false
				);
				break;
		}
	}

	public static string ToNodePath(this string path, string fromPath)
	{
		if (path.StartsWith("..")) throw new ArgumentException("Cannot start with ..", nameof(path));
		if (fromPath.StartsWith("..")) throw new ArgumentException("Cannot start with ..", nameof(fromPath));
		path = Normalize(path);
		fromPath = Normalize(fromPath);
		var pathParts = path.Split('/');
		pathParts[pathParts.Length - 1] = System.IO.Path.GetFileNameWithoutExtension(pathParts[pathParts.Length - 1]);
		var fromPathParts = System.IO.Path.GetDirectoryName(fromPath).Split('/');
		var ignored = pathParts.TakeWhile((p, i) => i < fromPathParts.Length && p == fromPathParts[i]).Count();
		pathParts = pathParts.Skip(ignored).ToArray();
		fromPathParts = fromPathParts.Skip(ignored).ToArray();
		return string.Join("/", Enumerable.Repeat(".", 1).Concat(Enumerable.Repeat("..", fromPathParts.Length).Concat(pathParts)));

		string Normalize(string p)
		{
			p = p.Replace('\\', '/');
			if (p.StartsWith("./")) p = p.Substring(2);
			return p;
		}
	}
}