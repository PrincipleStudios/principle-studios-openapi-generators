using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
                return TypeSafeAddPetResult.Ok(result);
            }
            else
            {
                return TypeSafeAddPetResult.OtherStatusCode(500, new Error(0, "Unable to add pet"));
            }
        }

        protected override async Task<TypeSafeFindPetsResult> FindPetsTypeSafe(IEnumerable<string>? tags, int? limit)
        {
            await Task.Yield();
            var result = Data.pets.AsEnumerable();
            if (tags != null && tags.Any())
            {
                result = result.Where(p => tags.Contains(p.Value.tag));
            }
            if (limit != null)
            {
                result = result.Take(limit.Value);
            }
            return TypeSafeFindPetsResult.Ok(result.Select(kvp => new Pet(kvp.Value.name, kvp.Value.tag, kvp.Key)).ToArray());
        }

        protected override async Task<TypeSafeDeletePetResult> DeletePetTypeSafe(long id)
        {
            await Task.Yield();
            if (Data.pets.Remove(id, out var _))
            {
                return TypeSafeDeletePetResult.NoContent();
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
                return TypeSafeFindPetByIdResult.Ok(new Pet(tuple.name, tuple.tag, id));
            }
            else
            {
                return TypeSafeFindPetByIdResult.OtherStatusCode(404, new Error(404, "Could not find pet"));
            }
        }

    }
}
