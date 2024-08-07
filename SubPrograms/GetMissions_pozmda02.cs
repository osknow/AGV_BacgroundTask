using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AGV_BackgroundTask.SubPrograms
{
    class GetMissions_pozmda02
    {
        public static async Task<List<GetCurrentTask>> AGV()
        {
            var url = "https://pozmda02.duni.org/api/DuniTasks/GetCurrentTasks/service?duniTaskRecipient=agv";


            using (var client = new HttpClient())
            {
                try
                {
                    return await client.GetFromJsonAsync<List<GetCurrentTask>>(url);
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }
        public static async Task<List<GetCurrentTask>> SERVICE()
        {
            var url = "https://pozmda02.duni.org/api/DuniTasks/GetCurrentTasks/service?duniTaskRecipient=service";


            using (var client = new HttpClient())
            {
                try
                {
                    return await client.GetFromJsonAsync<List<GetCurrentTask>>(url);
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }
    }
}
