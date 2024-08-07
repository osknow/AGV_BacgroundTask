using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AGV_BackgroundTask.SubPrograms
{
    class GetSubMachines_pozmda02
    {
        public static async Task<List<AGV_SubMachine>> GetSUBMachinesFromPOZMDA()
        {
            string HttpSerwerURI = "https://pozmda02.duni.org/api/Agv/AGV_SubMachines";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    return  await client.GetFromJsonAsync<List<AGV_SubMachine>>(HttpSerwerURI);

                     
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
