using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.Transformations;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApi.CSharp
{
	public class DotNetMvcAddServicesHelperTransformer : ISourceProvider
	{
		private CSharpControllerTransformer schemaTransformer;
		private readonly OperationGroupingSourceTransformer operationGrouping;

		public DotNetMvcAddServicesHelperTransformer(CSharpControllerTransformer schemaTransformer, OperationGroupingSourceTransformer operationGrouping)
		{
			this.schemaTransformer = schemaTransformer;
			this.operationGrouping = operationGrouping;
		}

		public IEnumerable<SourceEntry> GetSources(OpenApiTransformDiagnostic diagnostic)
		{
			yield return schemaTransformer.TransformAddServicesHelper(operationGrouping.GetGroupNames(diagnostic), diagnostic);
		}
	}
}
