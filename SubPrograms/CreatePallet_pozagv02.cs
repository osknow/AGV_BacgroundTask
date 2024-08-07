using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AGV_BackgroundTask.SubPrograms
{
    class CreatePallet_pozagv02
    {
        public static async Task SetResourses(ResourceAtLocation data)
        {
            using (var client = new HttpClient())
            {
                var url_ResourcesAtLocation = "https://pozagv02.duni.org:1234/api/ResourceAtLocation";
                //var url_LoadAtLocation = "https://pozagv02.duni.org:1234/api/LoadAtLocation";

                var jsonserialize = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                client.DefaultRequestHeaders.Add("ApiKey", "C1XUN3agvZ9P2ER");
                client.DefaultRequestHeaders.Add("Content", "application/json");


                var response = await client.PostAsJsonAsync(url_ResourcesAtLocation, data);

                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();


                    Console.WriteLine($"{response.StatusCode} | Żądanie paleta  wysłana poprawnie...");
                }
                else
                {
                    Console.WriteLine($"{response.StatusCode} , {response.RequestMessage}");
                }

            }
        }
    }
}
