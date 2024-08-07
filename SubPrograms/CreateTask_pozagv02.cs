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
    class CreateTask_pozagv02
    {
        public static TaskPozagv02_sBodyResponse responseJSON;
        public static async Task<HttpResponseMessage> POST(CreateTaskPozagv02_sBody body)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("ApiKey", "C1XUN3agvZ9P2ER");
                    client.DefaultRequestHeaders.Add("Content", "application/json");
                    string url = "https://pozagv02.duni.org:1234/api/AddProductionOrder/";
                    var response =  await client.PostAsJsonAsync<CreateTaskPozagv02_sBody>(url, body);
                    var outbody = response.Content.ReadAsStringAsync().Result;
                    responseJSON = JsonConvert.DeserializeObject<TaskPozagv02_sBodyResponse>(outbody);
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
