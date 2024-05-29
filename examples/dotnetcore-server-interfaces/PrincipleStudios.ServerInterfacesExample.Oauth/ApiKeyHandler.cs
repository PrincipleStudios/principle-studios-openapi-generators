using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace PrincipleStudios.ServerInterfacesExample.Oauth
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes - referenced by services
	internal class ApiKeyHandler : AuthenticationHandler<ApiKeyOptions>
#pragma warning restore CA1812
	{
		public ApiKeyHandler(IOptionsMonitor<ApiKeyOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
		{
		}

		protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			await Task.Yield();
			if (!Request.Headers.TryGetValue(Options.Header, out var apiKeyHeaderValues))
			{
				return AuthenticateResult.NoResult();
			}

			if (!apiKeyHeaderValues.All(v => v == Options.Value) || apiKeyHeaderValues.Count == 0)
				return AuthenticateResult.NoResult();


			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, Options.ClaimName)
			};

			claims.AddRange(Options.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

			var identity = new ClaimsIdentity(claims, Scheme.Name);
			var identities = new List<ClaimsIdentity> { identity };
			var principal = new ClaimsPrincipal(identities);
			var ticket = new AuthenticationTicket(principal, Scheme.Name);

			return AuthenticateResult.Success(ticket);
		}
	}
}