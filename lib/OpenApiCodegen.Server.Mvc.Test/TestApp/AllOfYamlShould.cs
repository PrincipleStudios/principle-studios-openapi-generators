using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp;

using static Utilities;

public class AllOfYamlShould
{

	[Fact]
	public Task HandleAllOfResponses() =>
		TestSingleRequest<AllOf.ContactControllerBase.GetContactActionResult>(new(
			AllOf.ContactControllerBase.GetContactActionResult.Ok(new(FirstName: "John", LastName: "Doe", Id: "john-doe-123")),
			client => client.GetAsync("/all-of/contact")
		)
		{
			AssertResponseMessage = VerifyResponse(200, new { firstName = "John", lastName = "Doe", id = "john-doe-123" }),
		});
}
