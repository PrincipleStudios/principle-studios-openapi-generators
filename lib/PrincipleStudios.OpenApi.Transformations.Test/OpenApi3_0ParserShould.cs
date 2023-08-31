using Bogus.DataSets;
using PrincipleStudios.OpenApi.Transformations.DocumentTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static PrincipleStudios.OpenApiCodegen.TestUtils.DocumentHelpers;

namespace PrincipleStudios.OpenApi.Transformations;

public class OpenApi3_0ParserShould
{
	[Fact]
	public void Reports_diagnostics_for_bad_yaml()
	{
		var result = GetOpenApiDocument("bad.yaml");
		// TODO - check more diagnostics here
		Assert.Contains(result.Diagnostics, (d) => d is UnableToParseSchema);
	}
}
