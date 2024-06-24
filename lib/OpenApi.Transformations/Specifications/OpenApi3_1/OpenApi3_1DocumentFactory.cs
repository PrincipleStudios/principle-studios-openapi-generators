using Json.More;
using Json.Pointer;
using PrincipleStudios.OpenApi.Transformations.Abstractions;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace PrincipleStudios.OpenApi.Transformations.Specifications.OpenApi3_1;

public class OpenApi3_1DocumentFactory
{
	private readonly DocumentRegistry documentRegistry;

	public List<DiagnosticBase> Diagnostics { get; }

	public OpenApi3_1DocumentFactory(DocumentRegistry documentRegistry, IEnumerable<DiagnosticBase> initialDiagnostics)
	{
		this.documentRegistry = documentRegistry;
		this.Diagnostics = initialDiagnostics.ToList();
	}

	internal OpenApiDocument? ConstructDocument(Uri baseUri, JsonNode jsonNode)
	{
		throw new NotImplementedException();
	}
}