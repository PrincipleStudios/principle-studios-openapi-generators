using PrincipleStudios.OpenApiCodegen.Json.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PrincipleStudios.ServerInterfacesExample.Controllers
{
    public class PetsController : PrincipleStudios.ServerInterfacesExample.Controllers.PetsControllerBase
    {

        protected override async Task<AddPetActionResult> AddPet(NewPet newPet)
        {
            await Task.Yield();
            var newId = Interlocked.Increment(ref Data.lastId);
            var result = new Pet(newPet.Name, newPet.Tag, newId);
            if (Data.pets.TryAdd(newId, (result.Name, result.Tag.GetValueOrDefault())))
            {
                return AddPetActionResult.Ok(result);
            }
            else
            {
                return AddPetActionResult.OtherStatusCode(500, new Error(0, "Unable to add pet"));
            }
        }

        protected override async Task<FindPetsActionResult> FindPets(Optional<IEnumerable<string>>? tags, Optional<int>? limit)
        {
            await Task.Yield();
            var result = Data.pets.AsEnumerable();
            if (tags.TryGet(out var actualTags) && actualTags.Any())
            {
                result = result.Where(p => actualTags.Contains(p.Value.tag));
            }
            if (limit.TryGet(out var actualLimit))
            {
                result = result.Take(actualLimit);
            }
            return FindPetsActionResult.Ok(result.Select(kvp => new Pet(kvp.Value.name, kvp.Value.tag is string ? (Optional<string>)kvp.Value.tag : null, kvp.Key)).ToArray());
        }

        protected override async Task<DeletePetActionResult> DeletePet(long id)
        {
            await Task.Yield();
            if (Data.pets.Remove(id, out var _))
            {
                return DeletePetActionResult.NoContent();
            }
            else
            {
                return DeletePetActionResult.OtherStatusCode(404, new Error(404, "Could not find pet"));
            }
        }

        protected override async Task<FindPetByIdActionResult> FindPetById(long id)
        {
            await Task.Yield();
            if (Data.pets.TryGetValue(id, out var tuple))
            {
                return FindPetByIdActionResult.Ok(new Pet(tuple.name, tuple.tag is string ? (Optional<string>)tuple.tag : null, id));
            }
            else
            {
                return FindPetByIdActionResult.OtherStatusCode(404, new Error(404, "Could not find pet"));
            }
        }

    }
}
