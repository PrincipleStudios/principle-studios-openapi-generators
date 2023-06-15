using PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp.Enum;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp;

using static Utilities;

public class EnumYamlShould
{
    [Fact]
    public Task Handle_enum_requests_and_responses() =>
        TestSingleRequest<Enum.RockPaperScissorsControllerBase.PlayRockPaperScissorsActionResult, Enum.PlayRockPaperScissorsRequest>(new(
            Enum.RockPaperScissorsControllerBase.PlayRockPaperScissorsActionResult.Ok(Enum.PlayRockPaperScissorsResponse.Player1),
            client => client.PostAsync("/enum/rock-paper-scissors", JsonContent.Create(new { player1 = "rock", player2 = "scissors" }))
        )
        {
            AssertRequest = (controller, request) =>
            {
                Assert.Equal(Enum.Option.Rock, request.Player1);
                Assert.Equal(Enum.Option.Scissors, request.Player2);
            },
            AssertResponseMessage = VerifyResponse(200, "player1"),
        });

    [Fact]
    public Task Handle_invalid_enum_requests_and_give_400_response() =>
        TestSingleRequest<Enum.RockPaperScissorsControllerBase.PlayRockPaperScissorsActionResult, Enum.PlayRockPaperScissorsRequest>(new(
            Enum.RockPaperScissorsControllerBase.PlayRockPaperScissorsActionResult.Unsafe(new Microsoft.AspNetCore.Mvc.BadRequestResult()),
            client => client.PostAsync("/enum/rock-paper-scissors", JsonContent.Create(new { player1 = "spock", player2 = "lizard" }))
        )
        {
            AssertRequest = (controller, request) =>
            {
                // it's okay if it either doesn't get here or this passes
                Assert.False(controller.ModelState.IsValid);
            },
            AssertResponseMessage = VerifyResponse(400),
        });

    [Fact]
    public Task Handle_enum_query_strings() =>
        TestSingleRequest<Enum.RockPaperScissorsQueryControllerBase.PlayRockPaperScissorsQueryActionResult, System.Tuple<Enum.Option, Enum.Option>>(new(
            Enum.RockPaperScissorsQueryControllerBase.PlayRockPaperScissorsQueryActionResult.Ok(Enum.PlayRockPaperScissorsQueryResponse.Player1),
            client => client.GetAsync("/enum/rock-paper-scissors-query?player1=rock&player2=scissors")
        )
        {
            AssertRequest = (controller, request) =>
            {
                Assert.Equal(Enum.Option.Rock, request.Item1);
                Assert.Equal(Enum.Option.Scissors, request.Item2);
            },
            AssertResponseMessage = VerifyResponse(200, "player1"),
        });


    [Theory]
    [InlineData(DifficultQueryStringEnumEnum.PlayerOne, "player+one")]
    [InlineData(DifficultQueryStringEnumEnum._2, "2")]
    [InlineData(DifficultQueryStringEnumEnum.Three, "three!")]
    [InlineData(DifficultQueryStringEnumEnum.F0u2, "f0u%7c2")]
    [InlineData(DifficultQueryStringEnumEnum.FiE, "fi%25e")]
    public Task SerializeEnumerationsWithSpacesInQuery(DifficultQueryStringEnumEnum enumValue, string queryValue) =>
        TestSingleRequest<Enum.DifficultEnumControllerBase.DifficultQueryStringEnumActionResult, DifficultQueryStringEnumEnum>(new(
            Enum.DifficultEnumControllerBase.DifficultQueryStringEnumActionResult.Ok("OK"),
            client => client.GetAsync($"/enum/difficult-enum?enum={queryValue}")
        )
        {
            AssertRequest = (controller, request) =>
            {
                Assert.Equal(enumValue, request);
            },
            AssertResponseMessage = VerifyResponse(200, "OK"),
        });

}
