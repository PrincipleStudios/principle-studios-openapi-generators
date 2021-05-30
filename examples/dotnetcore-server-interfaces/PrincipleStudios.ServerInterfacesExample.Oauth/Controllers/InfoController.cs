using System.Threading.Tasks;

namespace PrincipleStudios.ServerInterfacesExample.Oauth.Controllers
{
    public class InfoController : InfoControllerBase
    {
        protected override Task<TypeSafeGetInfoResult> GetInfoTypeSafe(byte[]? data)
        {
            return Task.FromResult(TypeSafeGetInfoResult.Ok(User.Identity!.IsAuthenticated ? $"success as {User.Identity.Name}" : "success"));
        }
    }
}