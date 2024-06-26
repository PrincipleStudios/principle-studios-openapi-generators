using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApiCodegen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations
{
	public class OperationGroupingSourceTransformer : ISourceProvider
	{
		public delegate (string groupName, string? groupSummary, string? groupDescription) OperationToGroup(OpenApiOperation operation, OpenApiContext context);
		private readonly IOpenApiElement openApiElement;
		private readonly OperationToGroup operationToGroup;
		private readonly IOpenApiOperationControllerTransformer operationControllerTransformer;
		private readonly OperationGroupingVisitor visitor = new();
		private OpenApiContext openApiContext;

		public OperationGroupingSourceTransformer(OpenApiDocument document, OperationToGroup operationToGroup, IOpenApiOperationControllerTransformer operationControllerTransformer)
			: this(document, OpenApiContext.From(document), operationToGroup, operationControllerTransformer)
		{
		}

		public OperationGroupingSourceTransformer(IOpenApiElement openApiElement, OpenApiContext openApiContext, OperationToGroup operationToGroup, IOpenApiOperationControllerTransformer operationControllerTransformer)
		{
			this.openApiElement = openApiElement;
			this.openApiContext = openApiContext;
			this.operationToGroup = operationToGroup;
			this.operationControllerTransformer = operationControllerTransformer;
		}

		private Dictionary<string, OperationGroupData> GetGroups(OpenApiTransformDiagnostic diagnostic)
		{
			var result = new Dictionary<string, OperationGroupData>();
			visitor.VisitAny(openApiElement, openApiContext, new OperationGroupingVisitor.Argument((operation, context) =>
			{
				var (group, summary, description) = operationToGroup(operation, context);
				group = operationControllerTransformer.SanitizeGroupName(group);
				var resultList = result[group] = result.TryGetValue(group, out var list) ? list : new()
				{
					Summary = summary,
					Description = description,
				};
				if (resultList.Summary != summary)
					resultList.Summary = null;
				if (resultList.Description != description)
					resultList.Description = null;
				resultList.Operations.Add((operation, context));
			}, diagnostic));
			return result;
		}

		public IEnumerable<string> GetGroupNames(OpenApiTransformDiagnostic diagnostic)
		{
			return GetGroups(diagnostic).Keys;
		}

		public IEnumerable<SourceEntry> GetSources(OpenApiTransformDiagnostic diagnostic)
		{
			return GetGroups(diagnostic).Select(kvp => operationControllerTransformer.TransformController(kvp.Key, kvp.Value, diagnostic)).ToArray();
		}

		class OperationGroupingVisitor : OpenApiDocumentVisitor<OperationGroupingVisitor.Argument>
		{
			public record Argument(RegisterOperationEntry RegisterSourceEntry, OpenApiTransformDiagnostic Diagnostic);
			public delegate void RegisterOperationEntry(OpenApiOperation operation, OpenApiContext context);

			public override void Visit(OpenApiOperation operation, OpenApiContext context, Argument argument)
			{
				try
				{
					argument.RegisterSourceEntry(operation, context);
				}
#pragma warning disable CA1031 // Do not catch general exception types
				catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
				{
					argument.Diagnostic.Errors.Add(new(context, $"Unhandled exception: {ex.Message}"));
				}
			}

			public override void Visit(OpenApiExternalDocs ignored, OpenApiContext context, Argument argument) { }
			public override void Visit(OpenApiServer ignored, OpenApiContext context, Argument argument) { }
			public override void Visit(OpenApiComponents ignored, OpenApiContext context, Argument argument) { }
			public override void Visit(OpenApiInfo ignored, OpenApiContext context, Argument argument) { }
			public override void Visit(OpenApiSecurityRequirement ignored, OpenApiContext context, Argument argument) { }
			public override void Visit(OpenApiTag ignored, OpenApiContext context, Argument argument) { }


		}
	}
}
