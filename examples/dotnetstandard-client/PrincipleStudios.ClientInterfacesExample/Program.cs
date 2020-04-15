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
            var header = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "Boo").ToString();

            await Petstore();
            await Petstore3();
        }

        private static async Task Petstore()
        {
            var test = new HttpRequestMessage(HttpMethod.Post,
                    new UriBuilder("http://localhost/pets")
                    {
                    }.Uri
                )
            {
                // TODO
                Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "Boo") }
            };

            var client = new Clients.Petstore.DefaultApiClient(null, null);
            var response = await client.AddPetAsync(null);
            var result = await response.StatusCode200Async();
        }

        private static async Task Petstore3()
        {
            var petClient = new Clients.Petstore3.PetApiClient(null, null);
            var response = await petClient.DeletePetAsync(null, null);

            var userClient = new Clients.Petstore3.UserApiClient(null, null);
            await userClient.LoginUserAsync(null, null);
        }
    }
}
