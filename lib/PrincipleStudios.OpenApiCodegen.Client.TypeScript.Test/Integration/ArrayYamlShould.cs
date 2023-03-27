using PrincipleStudios.OpenApiCodegen.Client.TypeScript.Utils;
using static PrincipleStudios.OpenApiCodegen.Client.TypeScript.Utils.GenerationUtilities;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript.Integration;

[Collection(CommonDirectoryFixture.CollectionName)]
public class ArrayYamlShould
{
    private readonly CommonDirectoryFixture commonDirectory;

    public ArrayYamlShould(CommonDirectoryFixture commonDirectory)
    {
        this.commonDirectory = commonDirectory;
    }

    [Fact]
    public async Task Support_array_types()
    {
        var body = new[] { "red", "green", "blue" };

        var result = await commonDirectory.CheckModel("array.yaml", "Colors", body);

        Assert.Equal(0, result.ExitCode);
    }

    [Fact(Skip = "Aliases get flattened currently")]
    public async Task Support_aliases_of_array_types()
    {
        var body = new[] { "red", "green", "blue" };

        var result = await commonDirectory.CheckModel("array.yaml", "Palette", body);

        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task Support_arrays_of_array_types()
    {
        var body = new[] { new[] { "red", "green", "blue" }, new[] { "cyan", "yellow", "magenta" } };

        var result = await commonDirectory.CheckModel("array.yaml", "Palettes", body);

        Assert.Equal(0, result.ExitCode);
    }

}
