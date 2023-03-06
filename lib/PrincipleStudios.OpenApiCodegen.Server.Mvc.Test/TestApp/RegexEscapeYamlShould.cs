using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp;

using static Utilities;

public class RegexEscapeYamlShould
{
    [Theory]
    [InlineData(false, "nope")]
    [InlineData(true, "\"foo\"")]
    [InlineData(true, "prefix \"foo\" suffix")] 
    public Task Handle_validation_of_regex_parameters(bool expectedIsValid, string content) =>
        TestSingleRequest<RegexEscape.ControllerBase.TestForRegexActionResult, string>(new(
            RegexEscape.ControllerBase.TestForRegexActionResult.Unsafe(new Microsoft.AspNetCore.Mvc.BadRequestResult()),
            client => client.PostAsync("/regex-escape/", JsonContent.Create(content))
        )
        {
            AssertRequest = (controller, request) =>
            {
                Assert.Equal(expectedIsValid, controller.ModelState.IsValid);
            },
            AssertResponseMessage = VerifyResponse(400),
        });

}
