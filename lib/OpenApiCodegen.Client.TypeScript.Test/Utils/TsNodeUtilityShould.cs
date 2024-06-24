using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript.Utils;

using static NodeUtility;

public class TsNodeUtilityShould
{
	[Fact]
	public async Task Run_TsNode()
	{
		var result = await TsNode("console.log('hello world' as string)");
		Assert.Equal("hello world", result.Output.Trim());
	}
}

[Collection(CommonDirectoryFixture.CollectionName)]
public class TsNodeWithCommonDirectoryShould
{
	private readonly CommonDirectoryFixture commonDirectory;

	public TsNodeWithCommonDirectoryShould(CommonDirectoryFixture commonDirectory)
	{
		this.commonDirectory = commonDirectory;
	}

	[Fact]
	public async Task Be_able_to_import_common_npm_package()
	{
		var result = await commonDirectory.TsNode(@"
            import type { HttpHeaders } from '@principlestudios/openapi-codegen-typescript';
            const headers: HttpHeaders = {};
            console.log(JSON.stringify(headers));
        ");

		Assert.Equal(0, result.ExitCode);
		Assert.Equal("{}", result.Output.Trim());
	}
}