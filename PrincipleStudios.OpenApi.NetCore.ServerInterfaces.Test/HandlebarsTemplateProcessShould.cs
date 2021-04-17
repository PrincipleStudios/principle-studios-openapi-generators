using System;
using Xunit;

namespace PrincipleStudios.OpenApi.NetCore.ServerInterfaces.Test
{
    public class HandlebarsTemplateProcessShould
    {
        [Fact]
        public void RegisterAllHandlebarsTemplates()
        {
            var handlebars = HandlebarsTemplateProcess.CreateHandlebars();

            Assert.True(handlebars.Configuration.RegisteredTemplates.Count >= 10);
        }
    }
}
