using PrincipleStudios.OpenApi.CSharp;
using System;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.Test
{
    public class HandlebarsTemplateProcessShould
    {
        [Fact]
        public void RegisterAllHandlebarsTemplates()
        {
            var handlebars = ControllerHandlebarsTemplateProcess.CreateHandlebars();

            Assert.True(handlebars.Configuration.RegisteredTemplates.Count >= 4);
        }
    }
}
