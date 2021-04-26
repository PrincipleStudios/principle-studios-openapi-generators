using System.Threading.Tasks;

namespace PrincipleStudios.ServerInterfacesExample.Oauth.Controllers
{
    public class InfoController : InfoControllerBase
    {
        protected override Task<TypeSafeGetInfoResult> GetInfoTypeSafe()
        {
            return Task.FromResult(TypeSafeGetInfoResult.ApplicationJsonStatusCode200(User.Identity!.IsAuthenticated ? $"success as {User.Identity.Name}" : "success"));
        }
    }
}