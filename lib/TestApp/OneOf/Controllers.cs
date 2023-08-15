using static PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp.Enum.RockPaperScissorsControllerBase;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp.OneOf;

public class PetController : PetControllerBase
{
	protected override Task<AddPetActionResult> AddPet(SpecifiedPet addPetBody)
	{
		this.DelegateRequest(addPetBody);
		return this.DelegateResponse<AddPetActionResult>();
	}

	protected override Task<GetRandomPetActionResult> GetRandomPet()
	{
		this.DelegateRequest();
		return this.DelegateResponse<GetRandomPetActionResult>();
	}
}
