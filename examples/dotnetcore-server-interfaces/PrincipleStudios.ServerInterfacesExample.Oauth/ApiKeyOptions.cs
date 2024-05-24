using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;

namespace PrincipleStudios.ServerInterfacesExample.Oauth
{
	internal class ApiKeyOptions : AuthenticationSchemeOptions
	{
		public string Header { get; set; } = "X-API-Key";
		public string Value { get; set; } = "hard-coded-value";
		public string ClaimName { get; set; } = "API_KEY";
		public List<string> Roles { get; set; } = new() { "apikey" };
	}
}