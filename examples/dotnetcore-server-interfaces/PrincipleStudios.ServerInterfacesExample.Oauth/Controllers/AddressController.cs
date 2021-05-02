using System.Threading.Tasks;

namespace PrincipleStudios.ServerInterfacesExample.Oauth.Controllers
{
    public class AddressController : AddressControllerBase
    {
        public AddressController()
        {

        }

        protected override Task<TypeSafeGetAddressResult> GetAddressTypeSafe()
        {
            // TODO - this should probably move to code generation
            if (User.Identity?.AuthenticationType == "OAuth2" && (User.FindFirst("scope")?.Value ?? User.FindFirst("scp")?.Value ?? "").Split(' ', ',') switch
                {
                    var scopes when global::System.Linq.Enumerable.All(new[] { "read:user", "user:email" }, scope => global::System.Linq.Enumerable.Contains(scopes, scope)) => false,
                    _ => true
                })
            {
                return Task.FromResult(TypeSafeGetAddressResult.Unsafe(Forbid()));
            }

            return Task.FromResult(TypeSafeGetAddressResult.StatusCode200(User.Identity!.IsAuthenticated ? $"success as {User.Identity.Name}" : "success"));
        }
    }
}