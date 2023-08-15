using static PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp.AllOf.ContactControllerBase;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp.ControllerExtensions
{
	public class InfoController : ControllerExtensions.InformationControllerBase
	{
		protected override Task<GetInfoActionResult> GetInfo(byte[]? data)
		{
			this.DelegateRequest(data);
			return this.DelegateResponse<GetInfoActionResult>();
		}
	}
}
