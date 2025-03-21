using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AGV_BackgroundTask.SubPrograms
{
    class PostSubMachines_pozmda02
    {
        public static async Task<HttpResponseMessage> PostMachinesToPOZMDA(AGV_SubMachine data)
        {
            string HttpSerwerURI = "https://pozmda02.duni.org/api/Agv/AGV_IPOINTStatusUpdate";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.PostAsJsonAsync($"{HttpSerwerURI}", data);

                    return response;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: Błąd podzas aktualizacji danych o IPOINCIE. ");
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}
