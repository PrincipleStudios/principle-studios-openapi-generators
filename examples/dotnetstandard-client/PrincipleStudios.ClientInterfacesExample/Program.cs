using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrincipleStudios.ClientInterfacesExample
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var client = new Clients.DefaultApiClient(null, null);
            var response = await client.AddPetAsync(null);
            var result = await response.StatusCode200Async();
        }
    }
}
