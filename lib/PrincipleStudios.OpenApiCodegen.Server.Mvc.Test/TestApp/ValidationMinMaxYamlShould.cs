using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp;

using static Utilities;

public class ValidationMinMaxYamlShould
{
	[Theory]
	[InlineData(false, -1)]
	[InlineData(true, 1)]
	[InlineData(true, 7654L)]
	public Task Handle_validation_of_integer_parameters(bool expectedIsValid, long value) =>
		TestSingleRequest<ValidationMinMax.ColorsControllerBase.GetColorActionResult, long>(new(
			ValidationMinMax.ColorsControllerBase.GetColorActionResult.Ok("red"),
			client => client.GetAsync($"/validation-min-max/colors?id={value}")
		)
		{
			AssertRequest = (controller, request) =>
			{
				Assert.Equal(expectedIsValid, controller.ModelState.IsValid);
			},
			AssertResponseMessage = VerifyResponse(200, "red"),
		});

}
