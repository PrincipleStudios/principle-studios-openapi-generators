using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp;

using static Utilities;

public class ControllerExtensionsYamlShould
{
	[Fact]
	public Task DecodeBase64EncodedQueryData() =>
		TestSingleRequest<ControllerExtensions.InformationControllerBase.GetInfoActionResult>(new(
			ControllerExtensions.InformationControllerBase.GetInfoActionResult.Ok("SomeData"),
			client => client.GetAsync("/controller-extensions/api/info")
		)
		{
			AssertResponseMessage = VerifyResponse(200, "SomeData"),
		});

}
