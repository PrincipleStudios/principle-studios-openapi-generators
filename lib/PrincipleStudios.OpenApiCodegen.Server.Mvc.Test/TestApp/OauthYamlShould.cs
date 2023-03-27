using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp;

using static Utilities;

public class OauthYamlShould
{
    [Fact]
    public Task Handle_anonymous_requests() =>
        TestSingleRequest<OAuth.InfoControllerBase.GetInfoActionResult>(new(
            OAuth.InfoControllerBase.GetInfoActionResult.Ok("baadf00d"),
            client => client.GetAsync("/oauth/info")
        )
        {
            AssertRequest = (controller, request) =>
            {
                Assert.False(controller.User.Identity?.IsAuthenticated);
            },
            AssertResponseMessage = VerifyResponse(200, "baadf00d"),
        });


    [Fact]
    public Task Handle_api_key_requests() =>
        TestSingleRequest<OAuth.InfoControllerBase.GetInfoActionResult>(new(
            OAuth.InfoControllerBase.GetInfoActionResult.Ok("baadf00d"),
            client => client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/oauth/info")
            {
                Headers =
                {
                    { "X-API-Key", "xunit-test-user" }
                }
            })
        )
        {
            AssertRequest = (controller, request) =>
            {
                Assert.True(controller.User.Identity?.IsAuthenticated);
                var claim = controller.User.FindFirst(ClaimTypes.Name);
                Assert.NotNull(claim);
                Assert.Equal("Key:xunit-test-user", claim?.Value);
            },
            AssertResponseMessage = VerifyResponse(200, "baadf00d"),
        });

    [Fact]
    public Task Block_anonymous_requests() =>
        TestRequestBlockedByFramework(
            client => client.GetAsync("/oauth/address"),
            async (response) =>
            {
                await VerifyResponse(302)(response);
                Assert.Equal("oauth.example.com", response.Headers.Location?.Host);
            });

    [Fact]
    public Task Handles_multiple_authorization_schemes() =>

        TestSingleRequest<OAuth.AddressControllerBase.GetAddressActionResult>(new(
            OAuth.AddressControllerBase.GetAddressActionResult.Ok("baadf00d"),
            client => client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/oauth/address")
            {
                Headers =
                {
                    { "X-API-Key", "xunit-test-user" }
                }
            })
        )
        {
            AssertRequest = (controller, request) =>
            {
                Assert.True(controller.User.Identity?.IsAuthenticated);
                var claim = controller.User.FindFirst(ClaimTypes.Name);
                Assert.NotNull(claim);
                Assert.Equal("Key:xunit-test-user", claim?.Value);
            },
            AssertResponseMessage = VerifyResponse(200, "baadf00d"),
        });

}
