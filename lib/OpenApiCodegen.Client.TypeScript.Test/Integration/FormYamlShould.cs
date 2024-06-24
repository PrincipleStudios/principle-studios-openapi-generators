using PrincipleStudios.OpenApiCodegen.Client.TypeScript.Utils;
using static PrincipleStudios.OpenApiCodegen.Client.TypeScript.Utils.GenerationUtilities;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript.Integration;

[Collection(CommonDirectoryFixture.CollectionName)]
public class FormYamlShould
{
	private readonly CommonDirectoryFixture commonDirectory;

	public FormYamlShould(CommonDirectoryFixture commonDirectory)
	{
		this.commonDirectory = commonDirectory;
	}

	[Fact(Skip = "Form support is currenlty very weak; the generated types do not pass.")]
	public async Task Be_able_to_generate_the_request()
	{
		var body = new { name = "Fido", tag = "dog", hasIdTag = true };

		var result = await commonDirectory.ConvertRequest("form.yaml", "postBasicForm", new { }, body, contentType: "application/x-www-form-urlencoded");

		AssertRequestSuccess(result, "POST", "/form/basic", body);
	}

}
