using PrincipleStudios.OpenApi.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static PrincipleStudios.OpenApiCodegen.TestUtils.DocumentHelpers;

namespace PrincipleStudios.OpenApiCodegen.Client.CSharp
{
	using static OptionsHelpers;
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

		[MemberData(nameof(WindowsPaths))]
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

		public static IEnumerable<object?[]> WindowsPaths()
		{
			if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
				yield break;

			yield return ["PrincipleStudios.Project", @"C:\users\user\source\project\", @"C:\users\user\source\project\controllers\api.yaml", null, "PrincipleStudios.Project.Controllers"];
			yield return ["PrincipleStudios.Project", @"C:\users\user\source\project\", @"C:\users\user\source\api.yaml", @"controllers\api.yaml", "PrincipleStudios.Project.Controllers"];
			yield return ["PrincipleStudios.Project", @"C:\users\user\source\project\", @"C:\users\user\source\project\api.yaml", null, "PrincipleStudios.Project"];
			yield return ["PrincipleStudios.Project", @"C:\users\user\source\project\", @"C:\users\user\source\api.yaml", @"api.yaml", "PrincipleStudios.Project"];
			yield return ["", @"C:\users\user\source\project\", @"C:\users\user\source\project\controllers\api.yaml", null, "Controllers"];
			yield return ["", @"C:\users\user\source\project\", @"C:\users\user\source\api.yaml", @"controllers\api.yaml", "Controllers"];
			yield return ["", @"C:\users\user\source\project\", @"C:\users\user\source\project\api.yaml", null, ""];
			yield return ["", @"C:\users\user\source\project\", @"C:\users\user\source\api.yaml", @"api.yaml", ""];
		}
	}
}
