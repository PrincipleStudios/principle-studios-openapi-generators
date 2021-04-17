using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApi.NetCore.ServerInterfaces
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
            var actual = CSharpNaming.ToClassName(input);
            Assert.Equal(expected, actual);
        }
    }
}
