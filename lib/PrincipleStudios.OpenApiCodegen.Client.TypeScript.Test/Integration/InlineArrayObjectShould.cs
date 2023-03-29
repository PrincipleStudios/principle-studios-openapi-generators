using PrincipleStudios.OpenApiCodegen.Client.TypeScript.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript.Integration;

[Collection(CommonDirectoryFixture.CollectionName)]
public class InlineArrayObjectShould
{
    private readonly CommonDirectoryFixture commonDirectory;

    public InlineArrayObjectShould(CommonDirectoryFixture commonDirectory)
    {
        this.commonDirectory = commonDirectory;
    }

    [Fact]
    public async Task Allow_objects_inside_inline_arrays()
    {
        var body = new[] { new { id = "baadf00d" } };

        var result = await commonDirectory.CheckModel("github-issue-42.yaml", "SomeArray", body);

        Assert.Equal(0, result.ExitCode);
    }
}
