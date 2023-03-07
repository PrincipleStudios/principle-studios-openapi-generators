using Microsoft.Extensions.Configuration;
using PrincipleStudios.OpenApi.CSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc
{
    public static class OptionsHelpers
    {
        public static CSharpServerSchemaOptions LoadOptions(Action<IConfigurationBuilder>? configureBuilder = null)
        {
            using var defaultJsonStream = CSharpSchemaOptions.GetDefaultOptionsJson();
            var builder = new ConfigurationBuilder();
            builder.AddYamlStream(defaultJsonStream);
            configureBuilder?.Invoke(builder);
            var result = builder.Build().Get<CSharpServerSchemaOptions>();
            if (result == null) throw new InvalidOperationException("Could not load default test");
            return result;
        }
    }
}
