using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApiCodegen;
using System;
using System.Collections.Generic;

namespace PrincipleStudios.OpenApi.Transformations
{
	public class OperationSourceTransformer : ISourceProvider
	{
		private readonly OpenApiDocument document;
		private readonly IOpenApiOperationTransformer operationTransformer;
		private static readonly OperationVisitor visitor = new();

		public OperationSourceTransformer(OpenApiDocument document, IOpenApiOperationTransformer operationTransformer)
		{
			this.document = document;
			this.operationTransformer = operationTransformer;
		}

		public IEnumerable<SourceEntry> GetSources(OpenApiTransformDiagnostic diagnostic)
		{
			List<(OpenApiOperation operation, OpenApiContext context)> operations = GetOperations(diagnostic);

			foreach (var (operation, context) in operations)
			{
				yield return operationTransformer.TransformOperation(operation, context, diagnostic);
			}
		}

		public List<(OpenApiOperation operation, OpenApiContext context)> GetOperations(OpenApiTransformDiagnostic diagnostic)
		{
			var operations = new List<(OpenApiOperation operation, OpenApiContext context)>();
			visitor.VisitAny(document, OpenApiContext.From(document), new OperationVisitor.Argument((operation, context) =>
			{
				operations.Add((operation, context));
			}, diagnostic));
			return operations;
		}

		class OperationVisitor : OpenApiDocumentVisitor<OperationVisitor.Argument>
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
