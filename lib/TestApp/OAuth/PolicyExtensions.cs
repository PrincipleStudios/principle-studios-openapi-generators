using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Microsoft.Extensions.DependencyInjection;

public static class OuthYamlPolicyExtensions
{
    // The following constants must match what is in oauth.yaml
    const string ApiKeyAuthName = "ApiKeyAuth";
    const string AuthorizationHeaderName = "X-API-Key";
    public const string OAuthAuthName = "OAuth2";

    public static AuthenticationBuilder AddOauthYamlPolicies(this AuthenticationBuilder builder)
    {
        return builder
            .AddScheme<OauthYamlApiKeyOptions, OauthYamlApiKeyHandler>(ApiKeyAuthName, null)
            .AddOAuth(OAuthAuthName, opt =>
            {
                // Note that this isn't a real oauth flow, just supposed to look that way for our unit tests
                opt.AuthorizationEndpoint = "https://oauth.example.com/";
                opt.TokenEndpoint = "https://oauth.example.com/oauth/token";
                opt.ClientId = "<clientID>";
                opt.ClientSecret = "<secret>";
                opt.CallbackPath = "/cb_oauth";
                opt.SaveTokens = true;
            });
    }

    public static AuthorizationBuilder AddOauthYamlPolicies(this AuthorizationBuilder builder)
    {
        return builder
            .AddPolicy(ApiKeyAuthName, policy =>
            {
                policy.AddAuthenticationSchemes(ApiKeyAuthName);
                policy.RequireAuthenticatedUser();
            })
            .AddPolicy(OAuthAuthName, policy =>
            {
                policy.AddAuthenticationSchemes(OAuthAuthName);
                policy.RequireAuthenticatedUser();
            });
    }

    public class OauthYamlApiKeyOptions : AuthenticationSchemeOptions
    {
    }

    private class OauthYamlApiKeyHandler : AuthenticationHandler<OauthYamlApiKeyOptions>
    {
        public OauthYamlApiKeyHandler(IOptionsMonitor<OauthYamlApiKeyOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // This is just a dummy api key handler
            await Task.Yield();

            if (!Request.Headers.ContainsKey(AuthorizationHeaderName))
            {
                //Authorization header not in request
                return AuthenticateResult.NoResult();
            }

            var headerValue = Request.Headers[AuthorizationHeaderName];

            var identity = new ClaimsIdentity(Scheme.Name);
            identity.AddClaim(new Claim(ClaimTypes.Name, $"Key:{headerValue}"));
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }
}
