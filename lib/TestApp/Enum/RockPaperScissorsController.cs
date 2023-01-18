using static PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp.ControllerExtensions.InformationControllerBase;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp.Enum
{
    public class RockPaperScissorsController : RockPaperScissorsControllerBase
    {
        protected override Task<PlayRockPaperScissorsActionResult> PlayRockPaperScissors(PlayRockPaperScissorsRequest playRockPaperScissorsBody)
        {
            this.DelegateRequest(playRockPaperScissorsBody);
            return this.DelegateResponse<PlayRockPaperScissorsActionResult>();
        }
    }
}
