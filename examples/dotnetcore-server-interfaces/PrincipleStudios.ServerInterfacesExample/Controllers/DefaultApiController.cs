using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
#nullable disable warnings

namespace PrincipleStudios.ServerInterfacesExample.Controllers
{
    public class DefaultApiController : PrincipleStudios.ServerInterfacesExample.Controllers.PetsControllerBase
    {
        // Obviously, this is a demo. You should use a different class for storage, and only use your controller for mapping.
        private readonly static System.Collections.Concurrent.ConcurrentDictionary<long, (string name, string tag)> pets = new System.Collections.Concurrent.ConcurrentDictionary<long, (string name, string tag)>();
        private static long lastId = 0;

        protected override async Task<TypeSafeAddPetResult> AddPetTypeSafe(NewPet newPet)
        {
            await Task.Yield();
            var newId = Interlocked.Increment(ref lastId);
            var result = new Pet(newPet.Name, newPet.Tag, newId);
            if (pets.TryAdd(newId, (result.Name, result.Tag)))
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
            var result = pets.Where(p => tags.Contains(p.Value.tag));
            //if (limit != null)
            //{
                result = result.Take(limit);
            //}
            return TypeSafeFindPetsResult.ApplicationJsonStatusCode200(result.Select(kvp => new Pet(kvp.Value.name, kvp.Value.tag, kvp.Key)).ToArray());
        }

    }

    public class PetsIdController : PetsIdControllerBase
    {
        // Obviously, this is a demo. You should use a different class for storage, and only use your controller for mapping.
        private readonly static System.Collections.Concurrent.ConcurrentDictionary<long, (string name, string tag)> pets = new System.Collections.Concurrent.ConcurrentDictionary<long, (string name, string tag)>();
        private static long lastId = 0;

        protected override async Task<TypeSafeDeletePetResult> DeletePetTypeSafe(long id)
        {
            await Task.Yield();
            if (pets.Remove(id, out var _))
            {
                return TypeSafeDeletePetResult.Unsafe(NoContent());
            }
            else
            {
                return TypeSafeDeletePetResult.ApplicationJsonOtherStatusCode(404, new Error(404, "Could not find pet"));
            }
        }

        protected override async Task<TypeSafeFindPetByIdResult> FindPetByIdTypeSafe(long id)
        {
            await Task.Yield();
            if (pets.TryGetValue(id, out var tuple))
            {
                return TypeSafeFindPetByIdResult.ApplicationJsonStatusCode200(new Pet(tuple.name, tuple.tag, id));
            }
            else
            {
                return TypeSafeFindPetByIdResult.ApplicationJsonOtherStatusCode(404, new Error(404, "Could not find pet"));
            }
        }

    }
}
