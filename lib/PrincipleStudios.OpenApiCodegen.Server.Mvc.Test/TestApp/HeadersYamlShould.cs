using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp;

using static Utilities;

public class HeadersYamlShould
{
    [Fact]
    public Task HandleHeaderRequestsAndResponses() =>
        TestSingleRequest<Headers.InfoControllerBase.GetInfoActionResult, byte[]>(new(
            Headers.InfoControllerBase.GetInfoActionResult.Ok(new() { ["foo"] = "bar" }, "some-header-data"),
            client => client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/headers/info")
            {
                Headers =
                {
                    { "X-Data", "Zm9vYmFy" },
                }
            })
        )
        {
            AssertRequest = (controller, request) =>
            {
                Assert.Collection(
                    request, 
                    Encoding.ASCII.GetBytes("foobar")
                        .Select(expected => (Action<byte>)(actual => Assert.Equal(expected, actual)))
                        .ToArray()
                );
            },
            AssertResponseMessage = async (responseMessage) => {
                await VerifyResponse(200, new { foo = "bar" })(responseMessage);
                Assert.Equal("some-header-data", responseMessage.Headers.GetValues("X-Data").Single());
            },
        });

    [Fact]
    public Task HandleRedirectionResponses() =>
        TestSingleRequest<Headers.RedirectControllerBase._RedirectActionResult, byte[]>(new(
            Headers.RedirectControllerBase._RedirectActionResult.Redirect("https://google.com"),
            client => client.GetAsync("/headers/redirect")
        )
        {
            AssertResponseMessage = async (responseMessage) => {
                await VerifyResponse(302)(responseMessage);
                Assert.Equal("https://google.com", responseMessage.Headers.Location?.OriginalString);
            },
        });
}
