using PrincipleStudios.OpenApiCodegen.Client.TypeScript.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript.Integration;

[Collection(CommonDirectoryFixture.CollectionName)]
public class OneOfYamlShould
{
    private readonly CommonDirectoryFixture commonDirectory;

    public OneOfYamlShould(CommonDirectoryFixture commonDirectory)
    {
        this.commonDirectory = commonDirectory;
    }


    [Theory]
    [MemberData(nameof(ModelsWithoutDiscriminator))]
    public async Task Allow_a_model_without_discriminator(object body)
    {
        var result = await commonDirectory.CheckModel("one-of.yaml", "Pet", body);

        Assert.Equal(0, result.ExitCode);
    }
    public static IEnumerable<object[]> ModelsWithoutDiscriminator()
    {
        yield return new object[] { new { bark = true, breed = "Shiba Inu" } };
        yield return new object[] { new { hunts = false, age = 12 } };
    }

    [Theory]
    [MemberData(nameof(ModelsWithDiscriminator))]
    public async Task Allow_a_model_with_a_discriminator(object body)
    {
        var result = await commonDirectory.CheckModel("one-of.yaml", "SpecifiedPet", body);

        Assert.Equal(0, result.ExitCode);
    }
    public static IEnumerable<object[]> ModelsWithDiscriminator()
    {
        yield return new object[] { new { petType = "dog", bark = true, breed = "Shiba Inu" } };
        yield return new object[] { new { petType = "cat", hunts = false, age = 12 } };
    }

    [Fact]
    public async Task Be_able_to_catch_type_errors_without_discriminator()
    {
        var body = new { bark = true, age = 9, };

        var result = await commonDirectory.CheckModel("one-of.yaml", "Pet", body);

        Assert.NotEqual(0, result.ExitCode);
    }

    [Fact]
    public async Task Be_able_to_catch_type_errors_with_discriminator()
    {
        var body = new { petType = "cat", bark = true, breed = "Shiba Inu" };

        var result = await commonDirectory.CheckModel("one-of.yaml", "SpecifiedPet", body);

        Assert.NotEqual(0, result.ExitCode);
    }
}
