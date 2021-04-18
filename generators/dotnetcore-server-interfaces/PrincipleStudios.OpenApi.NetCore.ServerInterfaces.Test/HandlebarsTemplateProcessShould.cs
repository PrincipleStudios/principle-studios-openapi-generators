using System;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.Test
{
    public class HandlebarsTemplateProcessShould
    {
        [Fact]
        public void RegisterAllHandlebarsTemplates()
        {
            var handlebars = HandlebarsTemplateProcess.CreateHandlebars();

            Assert.True(handlebars.Configuration.RegisteredTemplates.Count >= 5);
        }
    }
}
