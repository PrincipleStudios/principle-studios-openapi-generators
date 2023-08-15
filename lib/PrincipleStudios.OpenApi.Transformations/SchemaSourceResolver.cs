using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations
{
	public delegate void SchemaCallback(OpenApiSchema schema, OpenApiContext context);
	public abstract class SchemaSourceResolver<TInlineDataType> : ISchemaSourceResolver<TInlineDataType>
	{
		protected SchemaSourceResolver()
			: this(new DefaultSchemaVisitor())
		{
		}

		protected SchemaSourceResolver(IOpenApiDocumentVisitor<SchemaCallback> schemaVisitor)
		{
			this.schemaVisitor = schemaVisitor;
		}

		private readonly Dictionary<OpenApiSchema, SchemaSourceEntry> referencedSchemas = new Dictionary<OpenApiSchema, SchemaSourceEntry>();
		private readonly IOpenApiDocumentVisitor<SchemaCallback> schemaVisitor;

		public InlineDataTypeResolver<TInlineDataType> ToInlineDataType(OpenApiSchema schema)
		{
			if (!referencedSchemas.ContainsKey(schema))
				throw new InvalidOperationException("Attempt to use unregistered schema");

			return referencedSchemas[schema].Inline;
		}

		public IEnumerable<SourceEntry> GetSources(OpenApiTransformDiagnostic diagnostic) =>
			(from entry in referencedSchemas.Values
			 let sourceEntry = entry.GetSourceEntry?.Invoke()
			 where sourceEntry != null
			 select sourceEntry.Value)
			.Concat(GetAdditionalSources(referencedSchemas.Values.Select(v => v.Schema), diagnostic));

		protected virtual IEnumerable<SourceEntry> GetAdditionalSources(IEnumerable<OpenApiSchema> referencedSchemas, OpenApiTransformDiagnostic diagnostic) => Enumerable.Empty<SourceEntry>();

		public virtual void EnsureSchemasRegistered(Microsoft.OpenApi.Interfaces.IOpenApiElement element, OpenApiContext context, OpenApiTransformDiagnostic diagnostic)
		{
			var newSchemas = new Dictionary<OpenApiSchema, List<OpenApiContext>>();
			this.schemaVisitor.VisitAny(element, context, (nestedSchema, nestedContext) =>
			{
				if (!newSchemas.ContainsKey(nestedSchema))
					newSchemas.Add(nestedSchema, new());
				newSchemas[nestedSchema].Add(nestedContext);
			});

			foreach (var s in newSchemas)
			{
				if (referencedSchemas.TryGetValue(s.Key, out var schema))
				{
					schema.AllContext.AddRange(s.Value);
					continue;
				}

				if (s.Key.UnresolvedReference)
				{
					diagnostic.Errors.Add(new OpenApiTransformError(s.Value.First(), $"Unresolved external reference: {s.Key.Reference.Id} @ {s.Key.Reference.ExternalResource}"));
					referencedSchemas[s.Key] = new SchemaSourceEntry
					{
						Schema = s.Key,
						Inline = UnresolvedReferencePlaceholder,
						AllContext = new List<OpenApiContext>(s.Value),
						GetSourceEntry = null,
					};
					continue;
				}

				var allContext = new List<OpenApiContext>(s.Value);
				referencedSchemas[s.Key] = new SchemaSourceEntry
				{
					Schema = s.Key,
					Inline = () => GetInlineDataType(s.Key),
					AllContext = allContext,
					GetSourceEntry = () => GetSourceEntry(s.Key, allContext, diagnostic),
				};
			}
		}

		protected abstract TInlineDataType GetInlineDataType(OpenApiSchema schema);
		protected abstract SourceEntry? GetSourceEntry(OpenApiSchema schema, IEnumerable<OpenApiContext> allContexts, OpenApiTransformDiagnostic diagnostic);
		protected IEnumerable<OpenApiContext> GetApiContexts(OpenApiSchema schema) =>
			referencedSchemas[schema].AllContext.AsEnumerable();

		protected abstract TInlineDataType UnresolvedReferencePlaceholder();

		public delegate SourceEntry? SourceEntryResolver();

		private struct SchemaSourceEntry
		{
			public OpenApiSchema Schema { get; set; }
			public InlineDataTypeResolver<TInlineDataType> Inline { get; set; }
			public List<OpenApiContext> AllContext { get; set; }
			public SourceEntryResolver? GetSourceEntry { get; set; }
		}
	}

	public class DefaultSchemaVisitor : OpenApiDocumentVisitor<SchemaCallback>
	{
		public override void Visit(OpenApiSchema schema, OpenApiContext context, SchemaCallback argument)
		{
			argument(schema, context);
			base.Visit(schema, context, argument);
		}
	}
}
