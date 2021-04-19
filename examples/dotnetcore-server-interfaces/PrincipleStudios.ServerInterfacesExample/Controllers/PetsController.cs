using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#nullable disable warnings

namespace PrincipleStudios.ServerInterfacesExample.Controllers
{
    public class PetsController : PrincipleStudios.ServerInterfacesExample.Controllers.PetsControllerBase
    {

        protected override async Task<TypeSafeAddPetResult> AddPetTypeSafe(NewPet newPet)
        {
            await Task.Yield();
            var newId = Interlocked.Increment(ref Data.lastId);
            var result = new Pet(newPet.Name, newPet.Tag, newId);
            if (Data.pets.TryAdd(newId, (result.Name, result.Tag)))
            {
                return TypeSafeAddPetResult.ApplicationJsonStatusCode200(result);
            }
            else
            {
                return TypeSafeAddPetResult.ApplicationJsonOtherStatusCode(500, new Error(0, "Unable to add pet"));
            }
        }

        protected override async Task<TypeSafeFindPetsResult> FindPetsTypeSafe(IEnumerable<string> tags, int limit)
        {
            await Task.Yield();
            var result = Data.pets.Where(p => tags.Contains(p.Value.tag));
            //if (limit != null)
            //{
                result = result.Take(limit);
            //}
            return TypeSafeFindPetsResult.ApplicationJsonStatusCode200(result.Select(kvp => new Pet(kvp.Value.name, kvp.Value.tag, kvp.Key)).ToArray());
        }

    }
}
