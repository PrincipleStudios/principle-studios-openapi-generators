using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PrincipleStudios.ClientInterfacesExample
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await Petstore();
        }

        private static async Task Petstore()
        {
            using var httpClient = new HttpClient();
            var client = new Clients.Petstore.DefaultApiClient(httpClient, new Clients.Petstore.DefaultApiClientConfiguration
            {
                BaseUrl = "https://localhost:5001/api",
                Settings = new Newtonsoft.Json.JsonSerializerSettings()
            });
            var createdPetResponse = await client.AddPetAsync(new Clients.Petstore.NewPet(Name: "Fido", Tag: "Dog"));
            var pet = await createdPetResponse.StatusCode200Async();
            using var foundPetResponse = await client.FindPetByIdAsync(pet.Id);
            var foundPet = await foundPetResponse.StatusCode200Async();
            System.Diagnostics.Debug.Assert(pet.Name == foundPet.Name);
            using var foundPetsResponse = await client.FindPetsAsync(new[] { "Dog" }, 1);
            var pets = await foundPetsResponse.StatusCode200Async();
            System.Diagnostics.Debug.Assert(pets.Count == 1);
            System.Diagnostics.Debug.Assert(pet.Name == pets[0].Name);
            using var deletePetResponse = await client.DeletePetAsync(pet.Id);
            await deletePetResponse.StatusCode204Async();
            using var finalFindResponse = await client.FindPetsAsync(new[] { "Dog " }, 1);
            var finalPets = await finalFindResponse.StatusCode200Async();
            System.Diagnostics.Debug.Assert(finalPets.Count == 0);
        }

    }
}
