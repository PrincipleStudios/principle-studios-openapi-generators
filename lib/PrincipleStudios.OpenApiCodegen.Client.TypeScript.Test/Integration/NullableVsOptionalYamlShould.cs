using PrincipleStudios.OpenApiCodegen.Client.TypeScript.Utils;
using static PrincipleStudios.OpenApiCodegen.Client.TypeScript.Utils.GenerationUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript.Integration;

[Collection(CommonDirectoryFixture.CollectionName)]
public class NullableVsOptionalYamlShould
{
	private readonly CommonDirectoryFixture commonDirectory;

	public NullableVsOptionalYamlShould(CommonDirectoryFixture commonDirectory)
	{
		this.commonDirectory = commonDirectory;
	}

	[Fact]
	public async Task Support_providing_all_values_for_inline_objects()
	{
		var body = new { nullableOnly = 2, optionalOnly = 3, optionalOrNullable = 5 };

		var result = await commonDirectory.ConvertRequest("nullable-vs-optional.yaml", "contrived", new { }, body);

		AssertRequestSuccess(result, "POST", "/contrived", body);
	}

	[Fact]
	public async Task Support_providing_values_as_intended_by_yaml_for_inline_objects()
	{
		var body = new { nullableOnly = (object?)null };

		var result = await commonDirectory.ConvertRequest("nullable-vs-optional.yaml", "contrived", new { }, body);

		AssertRequestSuccess(result, "POST", "/contrived", body);
	}

	[Fact]
	public async Task Not_support_providing_extra_null_values_for_inline_objects()
	{
		var body = new { nullableOnly = (object?)null, optionalOnly = (object?)null, optionalOrNullable = (object?)null };

		var result = await commonDirectory.ConvertRequest("nullable-vs-optional.yaml", "contrived", new { }, body);

		Assert.NotEqual(0, result.ExitCode);
	}

	[Fact]
	public async Task Not_support_excluding_required_parameters_for_inline_objects()
	{
		var body = new { };

		var result = await commonDirectory.ConvertRequest("nullable-vs-optional.yaml", "contrived", new { }, body);

		Assert.NotEqual(0, result.ExitCode);
	}

	[Fact]
	public async Task Support_providing_all_values_for_models()
	{
		var body = new { errorCode = "foo:bar", errorMessage = "The foo did not bar", referenceCode = "12345" };

		var result = await commonDirectory.CheckModel("nullable-vs-optional.yaml", "_Error", body);

		Assert.Equal(0, result.ExitCode);
	}

	[Fact]
	public async Task Support_providing_values_as_intended_by_yaml_for_models()
	{
		var body = new { errorCode = "foo:bar", errorMessage = (object?)null };

		var result = await commonDirectory.CheckModel("nullable-vs-optional.yaml", "_Error", body);

		Assert.Equal(0, result.ExitCode);
	}

	[Fact]
	public async Task Not_support_providing_extra_null_values_for_models()
	{
		var body = new { errorCode = "foo:bar", errorMessage = (object?)null, referenceCode = (object?)null };

		var result = await commonDirectory.CheckModel("nullable-vs-optional.yaml", "_Error", body);

		Assert.NotEqual(0, result.ExitCode);
	}

	[Fact]
	public async Task Not_support_excluding_required_parameters_for_models()
	{
		var body = new { errorCode = "foo:bar" };

		var result = await commonDirectory.CheckModel("nullable-vs-optional.yaml", "_Error", body);

		Assert.NotEqual(0, result.ExitCode);
	}
}
