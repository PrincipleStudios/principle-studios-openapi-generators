using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations
{
	public class TagControllerSourceTransformer : ISourceProvider
	{
		private readonly OpenApiDocument document;
		private readonly IOpenApiTagControllerTransformer tagControllerTransformer;
		private readonly string defaultTagName;

		public TagControllerSourceTransformer(OpenApiDocument document, IOpenApiTagControllerTransformer tagControllerTransformer, string defaultTagName = "default")
		{
			this.document = document;
			this.tagControllerTransformer = tagControllerTransformer;
			this.defaultTagName = defaultTagName;
		}

		public IEnumerable<SourceEntry> GetSources(OpenApiTransformDiagnostic diagnostic)
		{
			var controllers = from path in document.Paths
							  from operation in path.Value.Operations
							  let firstTag = operation.Value.Tags.Select(t => t.Name).FirstOrDefault() ?? defaultTagName
							  group new OpenApiFullOperation(path, operation) by firstTag;

			foreach (var controller in controllers)
			{
				foreach (var entry in tagControllerTransformer.TransformController(controller.Key, controller, diagnostic))
				{
					yield return entry;
				}
			}
		}
	}
}
