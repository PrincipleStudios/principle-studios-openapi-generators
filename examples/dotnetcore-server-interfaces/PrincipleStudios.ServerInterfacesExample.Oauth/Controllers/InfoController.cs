using PrincipleStudios.OpenApiCodegen.Json.Extensions;
using System.Threading.Tasks;

namespace PrincipleStudios.ServerInterfacesExample.Oauth.Controllers
{
    public class InfoController : InfoControllerBase
    {
        protected override Task<GetInfoActionResult> GetInfo(Optional<byte[]>? data)
        {
            return Task.FromResult(GetInfoActionResult.Ok(User.Identity!.IsAuthenticated ? $"success as {User.Identity.Name}" : "success"));
        }
    }
}