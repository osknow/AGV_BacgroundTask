using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AGV_BackgroundTask.SubPrograms
{
    class ReadMachines
    {
        public static async Task<List<MachineModel>> GetMachinesFromPOZMDA()
        {


            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = "https://pozmda02.duni.org/api/Machine";
                    return await client.GetFromJsonAsync<List<MachineModel>>(url);
                }
                catch (HttpRequestException e)
                {
                    throw;
                }
            }
        }
    }
}
