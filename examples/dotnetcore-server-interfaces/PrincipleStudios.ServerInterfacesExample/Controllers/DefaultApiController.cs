using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PrincipleStudios.ServerInterfacesExample.Controllers
{
    public class DefaultApiController : DefaultApiControllerBase
    {
        // Obviously, this is a demo. You should use a different class for storage, and only use your controller for mapping.
        private readonly static System.Collections.Concurrent.ConcurrentDictionary<long, (string name, string tag)> pets = new System.Collections.Concurrent.ConcurrentDictionary<long, (string name, string tag)>();
        private static long lastId = 0;

        public override async Task<IActionResult> AddPet([FromBody] NewPet newPet)
        {
            await Task.Yield();
            var newId = Interlocked.Increment(ref lastId);
            var result = new Pet(newPet.Name, newPet.Tag, newId);
            if (pets.TryAdd(newId, (result.Name, result.Tag)))
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(500, new Error(0, "Unable to add pet"));
            }
        }

        public override async Task<IActionResult> DeletePet([FromRoute, Required] long? id)
        {
            await Task.Yield();
            if (id != null && pets.Remove(id.Value, out var _))
            {
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }

        public override async Task<IActionResult> FindPetById([FromRoute, Required] long? id)
        {
            await Task.Yield();
            if (id != null && pets.TryGetValue(id.Value, out var tuple))
            {
                return Ok(new Pet(tuple.name, tuple.tag, id.Value));
            }
            else
            {
                return NotFound();
            }
        }

        public override async Task<IActionResult> FindPets([FromQuery] List<string> tags, [FromQuery] int? limit)
        {
            await Task.Yield();
            var result = pets.Where(p => tags.Contains(p.Value.tag));
            if (limit != null)
            {
                result = result.Take(limit.Value);
            }
            return Ok(result.Select(kvp => new Pet(kvp.Value.name, kvp.Value.tag, kvp.Key)).ToArray());
        }
    }
}
