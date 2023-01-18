using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp;

using static Utilities;

public class EnumYamlShould
{
    [Fact]
    public Task HandleEnumRequestsAndResponses() =>
        TestSingleRequest<Enum.RockPaperScissorsControllerBase.PlayRockPaperScissorsActionResult>(new(
            Enum.RockPaperScissorsControllerBase.PlayRockPaperScissorsActionResult.Ok(Enum.PlayRockPaperScissorsResponse.Player1),
            client => client.PostAsync("/rock-paper-scissors", JsonContent.Create(new { player1 = "rock", player2 = "scissors" }))
        )
        {
            AssertResponseMessage = VerifyResponse(200, "player1"),
        });
}
