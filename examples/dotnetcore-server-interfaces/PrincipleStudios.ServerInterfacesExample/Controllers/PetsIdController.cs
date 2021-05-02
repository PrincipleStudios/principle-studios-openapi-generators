using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PrincipleStudios.ServerInterfacesExample.Controllers
{
    public class PetsIdController : PetsIdControllerBase
    {
        protected override async Task<TypeSafeDeletePetResult> DeletePetTypeSafe(long id)
        {
            await Task.Yield();
            if (Data.pets.Remove(id, out var _))
            {
                return TypeSafeDeletePetResult.StatusCode204();
            }
            else
            {
                return TypeSafeDeletePetResult.OtherStatusCode(404, new Error(404, "Could not find pet"));
            }
        }

        protected override async Task<TypeSafeFindPetByIdResult> FindPetByIdTypeSafe(long id)
        {
            await Task.Yield();
            if (Data.pets.TryGetValue(id, out var tuple))
            {
                return TypeSafeFindPetByIdResult.StatusCode200(new Pet(tuple.name, tuple.tag, id));
            }
            else
            {
                return TypeSafeFindPetByIdResult.OtherStatusCode(404, new Error(404, "Could not find pet"));
            }
        }

    }
}
