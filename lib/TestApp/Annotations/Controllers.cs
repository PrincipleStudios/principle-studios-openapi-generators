using static PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp.OneOf.PetControllerBase;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp.Annotations
{
	public class DogController : DogControllerBase
	{
		protected override Task<AddDogActionResult> AddDog(Dog addDogBody)
		{
			this.DelegateRequest(addDogBody);
			return this.DelegateResponse<AddDogActionResult>();
		}
	}
}
