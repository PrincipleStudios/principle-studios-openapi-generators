using PrincipleStudios.OpenApi.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript.Diagnostics;

using static OptionsHelpers;
using static PrincipleStudios.OpenApiCodegen.TestUtils.DocumentHelpers;

public class DiagnosticsReportingShould
{
	[Fact]
	public static void Report_unresolved_external_references()
	{
		var diagnostic = GetDocumentDiagnostics("bad.yaml");
		Assert.True(
			diagnostic.Errors.Any(err => err.Message.Contains("Unresolved external reference")),
			"Diagnostic did not report the actual issue: the reference to Pet was unresolved."
		);
	}

	private static OpenApiTransformDiagnostic GetDocumentDiagnostics(string name)
	{
		var document = GetDocument(name);
		var options = LoadOptions();

		var transformer = document.BuildTypeScriptOperationSourceProvider("", options);
		OpenApiTransformDiagnostic diagnostic = new();

		transformer.GetSources(diagnostic).ToArray(); // force all sources to load to get diagnostics
		return diagnostic;
	}

}
