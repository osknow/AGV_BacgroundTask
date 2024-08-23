using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AGV_BackgroundTask.SubPrograms
{
    class CreateMission_pozagv02
    {
        public static MissionsPozagv02_sBodyResponse responseJSON;
        public static async Task<HttpResponseMessage> POST (Object body)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("ApiKey", "C1XUN3agvZ9P2ER");
                    client.DefaultRequestHeaders.Add("Content", "application/json");
                    string url = "https://pozagv02.duni.org:1234/api/MissionCreate/";
                    var response = await client.PostAsJsonAsync<Object>(url, body);
                    var outbody = response.Content.ReadAsStringAsync().Result;
                    responseJSON = JsonConvert.DeserializeObject<MissionsPozagv02_sBodyResponse>(outbody);
                    return response;
                }
                catch (HttpRequestException e)
                {
                    throw;
                }
            }
        }

    }
}

