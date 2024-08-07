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
    class GetMissions_pozagv02
    {
        public static async Task<List<GetMissions>> Get()
        {
            var url = "https://pozagv02.duni.org:1234/api/GetMissions";


            using (var client = new HttpClient())
            {
                try
                {
                    var jsonserialize = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                    client.DefaultRequestHeaders.Add("ApiKey", "C1XUN3agvZ9P2ER");
                    client.DefaultRequestHeaders.Add("Content", "application/json");


                    return await client.GetFromJsonAsync<List<GetMissions>>(url);


                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }
    }
}
