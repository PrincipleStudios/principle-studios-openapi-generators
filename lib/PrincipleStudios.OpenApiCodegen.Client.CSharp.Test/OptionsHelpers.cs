using Microsoft.Extensions.Configuration;
using PrincipleStudios.OpenApi.CSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApiCodegen.Client.CSharp
{
	public static class OptionsHelpers
	{
		public static CSharpSchemaOptions LoadOptions(Action<IConfigurationBuilder>? configureBuilder = null)
		{
			using var defaultJsonStream = CSharpSchemaOptions.GetDefaultOptionsJson();
			var builder = new ConfigurationBuilder();
			builder.AddYamlStream(defaultJsonStream);
			configureBuilder?.Invoke(builder);
			var result = builder.Build().Get<CSharpSchemaOptions>()
					?? throw new InvalidOperationException("Could not construct options");
			return result;
		}
	}
}
