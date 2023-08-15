namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp.RegexEscape;

public class RegexEscapeRootController : TestApp.RegexEscape.ControllerBase
{
	protected override Task<TestForRegexActionResult> TestForRegex(string testForRegexBody)
	{
		this.DelegateRequest(testForRegexBody);
		return this.DelegateResponse<TestForRegexActionResult>();
	}
}
