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
    public class DefaultApiController : DefaultApiControllerBase
    {
        // Obviously, this is a demo. You should use a different class for storage, and only use your controller for mapping.
        private readonly static System.Collections.Concurrent.ConcurrentDictionary<long, (string name, string tag)> pets = new System.Collections.Concurrent.ConcurrentDictionary<long, (string name, string tag)>();
        private static long lastId = 0;

        public override async Task<TypeSafeAddPetResult> AddPetTypeSafe(NewPet newPet)
        {
            await Task.Yield();
            var newId = Interlocked.Increment(ref lastId);
            var result = new Pet(newPet.Name, newPet.Tag, newId);
            if (pets.TryAdd(newId, (result.Name, result.Tag)))
            {
                return TypeSafeAddPetResult.StatusCode200(result);
            }
            else
            {
                return TypeSafeAddPetResult.OtherStatusCode(500, new Error(0, "Unable to add pet"));
            }
        }

        public override async Task<TypeSafeDeletePetResult> DeletePetTypeSafe(long? id)
        {
            await Task.Yield();
            if (id != null && pets.Remove(id.Value, out var _))
            {
                return TypeSafeDeletePetResult.StatusCode204();
            }
            else
            {
                return TypeSafeDeletePetResult.OtherStatusCode(404, new Error(404, "Could not find pet"));
            }
        }

        public override async Task<TypeSafeFindPetByIdResult> FindPetByIdTypeSafe(long? id)
        {
            await Task.Yield();
            if (id != null && pets.TryGetValue(id.Value, out var tuple))
            {
                return TypeSafeFindPetByIdResult.StatusCode200(new Pet(tuple.name, tuple.tag, id.Value));
            }
            else
            {
                return TypeSafeFindPetByIdResult.OtherStatusCode(404, new Error(404, "Could not find pet"));
            }
        }

        public override async Task<TypeSafeFindPetsResult> FindPetsTypeSafe(IReadOnlyList<string> tags, int? limit)
        {
            await Task.Yield();
            var result = pets.Where(p => tags.Contains(p.Value.tag));
            if (limit != null)
            {
                result = result.Take(limit.Value);
            }
            return TypeSafeFindPetsResult.StatusCode200(result.Select(kvp => new Pet(kvp.Value.name, kvp.Value.tag, kvp.Key)).ToArray());
        }

    }
}
