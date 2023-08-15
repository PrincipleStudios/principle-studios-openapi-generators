namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp.Headers;

public class InfoController : InfoControllerBase
{
	protected override Task<GetInfoActionResult> GetInfo(byte[] xData)
	{
		this.DelegateRequest(xData);
		return this.DelegateResponse<GetInfoActionResult>();
	}
}

public class RedirectController : RedirectControllerBase
{
	protected override Task<_RedirectActionResult> Redirect()
	{
		this.DelegateRequest();
		return this.DelegateResponse<_RedirectActionResult>();
	}
}