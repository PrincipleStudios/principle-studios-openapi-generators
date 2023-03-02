using PrincipleStudios.OpenApiCodegen.Client.TypeScript.Utils;
using static PrincipleStudios.OpenApiCodegen.Client.TypeScript.Utils.GenerationUtilities;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript.Integration;

[Collection(CommonDirectoryFixture.CollectionName)]
public class AllOfYamlShould
{
    private readonly CommonDirectoryFixture commonDirectory;

    public AllOfYamlShould(CommonDirectoryFixture commonDirectory)
    {
        this.commonDirectory = commonDirectory;
    }

    [Fact]
    public async Task Be_able_to_generate_the_request()
    {
        var result = await commonDirectory.ConvertRequest("all-of.yaml", "getContact", new { });

        AssertRequestSuccess(result, "GET", "/contact");
    }

    [Fact]
    public async Task Combine_types_for_an_allOf()
    {
        var body = new { firstName = "John", lastName = "Smith", id = "a2611248-3e8e-46e1-98f5-46eb6195101f" };

        var result = await commonDirectory.CheckModel("all-of.yaml", "ContactWithId", body);

        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task Be_able_to_catch_type_errors()
    {
        var body = new { firstName = "John", lastName = "Smith", };

        var result = await commonDirectory.CheckModel("all-of.yaml", "ContactWithId", body);

        Assert.NotEqual(0, result.ExitCode);
    }

    [Fact]
    public async Task Transform_responses()
    {
        var statusCode = 200;
        var responseBody = new { firstName = "John", lastName = "Smith", id = "a2611248-3e8e-46e1-98f5-46eb6195101f" };

        var result = await commonDirectory.ConvertResponse("all-of.yaml", "getContact", statusCode, responseBody);
        AssertResponseSuccess(result, statusCode, responseBody);
    }
}
