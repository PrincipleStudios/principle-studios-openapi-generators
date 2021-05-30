using PrincipleStudios.OpenApi.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static PrincipleStudios.OpenApiCodegen.Server.Mvc.DocumentHelpers;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc
{
    public class CSharpNamingShould
    {
        [InlineData("CSharpNamingShould", "CSharpNamingShould")]
        [InlineData("404 not found", "_404NotFound")]
        [InlineData("camelCase", "CamelCase")]
        [InlineData("kebab-case", "KebabCase")]
        [InlineData("SCREAMING_CAPS", "ScreamingCaps")]
        [InlineData("underscore_case", "UnderscoreCase")]
        [Theory]
        public void ConvertStringsToValidClassNames(string input, string expected)
        {
            var options = LoadOptions();
            var actual = CSharpNaming.ToClassName(input, options.ReservedIdentifiers());
            Assert.Equal(expected, actual);
        }

        [InlineData("CSharpNamingShould", "cSharpNamingShould")]
        [InlineData("404 not found", "_404NotFound")]
        [InlineData("camelCase", "camelCase")]
        [InlineData("kebab-case", "kebabCase")]
        [InlineData("SCREAMING_CAPS", "screamingCaps")]
        [InlineData("underscore_case", "underscoreCase")]
        [InlineData("if", "_if")]
        [InlineData("for", "_for")]
        [Theory]
        public void ConvertStringsToValidParameterNames(string input, string expected)
        {
            var options = LoadOptions();
            var actual = CSharpNaming.ToParameterName(input, options.ReservedIdentifiers());
            Assert.Equal(expected, actual);
        }

        [InlineData("PrincipleStudios.Project", @"C:\users\user\source\project\", @"C:\users\user\source\project\controllers\api.yaml", null, "PrincipleStudios.Project.Controllers")]
        [InlineData("PrincipleStudios.Project", @"C:\users\user\source\project\", @"C:\users\user\source\api.yaml", @"controllers\api.yaml", "PrincipleStudios.Project.Controllers")]
        [InlineData("PrincipleStudios.Project", @"C:\users\user\source\project\", @"C:\users\user\source\project\api.yaml", null, "PrincipleStudios.Project")]
        [InlineData("PrincipleStudios.Project", @"C:\users\user\source\project\", @"C:\users\user\source\api.yaml", @"api.yaml", "PrincipleStudios.Project")]
        [InlineData("", @"C:\users\user\source\project\", @"C:\users\user\source\project\controllers\api.yaml", null, "Controllers")]
        [InlineData("", @"C:\users\user\source\project\", @"C:\users\user\source\api.yaml", @"controllers\api.yaml", "Controllers")]
        [InlineData("", @"C:\users\user\source\project\", @"C:\users\user\source\project\api.yaml", null, "")]
        [InlineData("", @"C:\users\user\source\project\", @"C:\users\user\source\api.yaml", @"api.yaml", "")]
        [InlineData("PrincipleStudios.Project", @"/users/user/source/project/", @"/users/user/source/project/controllers/api.yaml", null, "PrincipleStudios.Project.Controllers")]
        [InlineData("PrincipleStudios.Project", @"/users/user/source/project/", @"/users/user/source/api.yaml", @"controllers/api.yaml", "PrincipleStudios.Project.Controllers")]
        [InlineData("PrincipleStudios.Project", @"/users/user/source/project/", @"/users/user/source/project/api.yaml", null, "PrincipleStudios.Project")]
        [InlineData("PrincipleStudios.Project", @"/users/user/source/project/", @"/users/user/source/api.yaml", @"api.yaml", "PrincipleStudios.Project")]
        [InlineData("", @"/users/user/source/project/", @"/users/user/source/project/controllers/api.yaml", null, "Controllers")]
        [InlineData("", @"/users/user/source/project/", @"/users/user/source/api.yaml", @"controllers/api.yaml", "Controllers")]
        [InlineData("", @"/users/user/source/project/", @"/users/user/source/project/api.yaml", null, "")]
        [InlineData("", @"/users/user/source/project/", @"/users/user/source/api.yaml", @"api.yaml", "")]
        [Theory]
        public void ConvertPathsToNamespace(string? rootNamespace, string? projectDir, string? identity, string? link, string expected)
        {
            var options = LoadOptions();
            var actual = CSharpNaming.ToNamespace(rootNamespace, projectDir, identity, link, options.ReservedIdentifiers());
            Assert.Equal(expected, actual);
        }
    }
}
