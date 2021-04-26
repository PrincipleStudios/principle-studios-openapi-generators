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
            return Task.FromResult(TypeSafeGetAddressResult.ApplicationJsonStatusCode200(User.Identity!.IsAuthenticated ? $"success as {User.Identity.Name}" : "success"));
        }
    }
}