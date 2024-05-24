using System.Threading.Tasks;

namespace PrincipleStudios.ServerInterfacesExample.Oauth.Controllers
{
	public class AddressController : AddressControllerBase
	{
		protected override Task<GetAddressActionResult> GetAddress()
		{
			// TODO - this should probably move to code generation
			if (User.Identity?.AuthenticationType == "OAuth2" && (User.FindFirst("scope")?.Value ?? User.FindFirst("scp")?.Value ?? "").Split(' ', ',') switch
			{
				var scopes when global::System.Linq.Enumerable.All(scopes, scope => global::System.Linq.Enumerable.Contains(scopes, scope)) => false,
				_ => true
			})
			{
				return Task.FromResult(GetAddressActionResult.Unsafe(Forbid()));
			}

			return Task.FromResult(GetAddressActionResult.Ok(User.Identity!.IsAuthenticated ? $"success as {User.Identity.Name}" : "success"));
		}
	}
}