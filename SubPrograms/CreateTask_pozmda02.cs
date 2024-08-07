using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AGV_BackgroundTask.SubPrograms
{
    class CreateTask_pozmda02
    {
        public static async Task<HttpResponseMessage> POST(CreateTaskPozmda01_sBody body)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = "https://pozmda02.duni.org/api/DuniTasks/service";
                    return await client.PostAsJsonAsync<CreateTaskPozmda01_sBody>(url,body);

                    //return response;
                }
                catch (HttpRequestException e)
                {
                    throw;
                }
            }
        }
    }
}
