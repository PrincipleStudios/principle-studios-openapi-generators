using PrincipleStudios.OpenApiCodegen.Client.TypeScript.Utils;
using static PrincipleStudios.OpenApiCodegen.Client.TypeScript.Utils.GenerationUtilities;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript.Integration;

[Collection(CommonDirectoryFixture.CollectionName)]
public class HeadersYamlShould
{
    private readonly CommonDirectoryFixture commonDirectory;

    public HeadersYamlShould(CommonDirectoryFixture commonDirectory)
    {
        this.commonDirectory = commonDirectory;
    }

    [Fact]
    public async Task Pass_headers_as_params()
    {
        var headerData = "foobar";

        var result = await commonDirectory.ConvertRequest("headers.yaml", "getInfo", new { xData = headerData });

        var token = AssertRequestSuccess(result, "GET", "/info");
        Assert.Equal("foobar", token["headers"]?["X-Data"]?.ToObject<string>());
    }

}
