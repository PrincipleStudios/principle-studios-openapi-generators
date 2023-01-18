using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp;

using static Utilities;

public class MvcBindingShould
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


    [Fact]
    public Task HandleAllOfResponses() =>
        TestSingleRequest<AllOf.ContactControllerBase.GetContactActionResult>(new(
            AllOf.ContactControllerBase.GetContactActionResult.Ok(new(FirstName: "John", LastName: "Doe", Id: "john-doe-123")),
            client => client.GetAsync("/contact")
        )
        {
            AssertResponseMessage = VerifyResponse(200, new { firstName = "John", lastName = "Doe", id = "john-doe-123" }),
        });

    [Fact]
    public Task DecodeBase64EncodedQueryData() =>
        TestSingleRequest<ControllerExtensions.InformationControllerBase.GetInfoActionResult>(new(
            ControllerExtensions.InformationControllerBase.GetInfoActionResult.Ok("SomeData"),
            client => client.GetAsync("/api/info")
        )
        {
            AssertResponseMessage = VerifyResponse(200, "SomeData"),
        });

}
