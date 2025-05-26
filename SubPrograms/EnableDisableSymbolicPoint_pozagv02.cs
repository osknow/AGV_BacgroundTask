using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using AGV_BackgroundTask;

namespace StackPaletsFunction.REQUESTS
{
    class EnableDisableSymbolicPoint_pozagv02
    {
        public static async Task<HttpResponseMessage> POST (EnableDisableSymbolicPoint body)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("ApiKey", "C1XUN3agvZ9P2ER");
                    client.DefaultRequestHeaders.Add("Content", "application/json");
                    string url = "https://pozagv02.duni.org:1234/api/EnableDisableSymbolicPoint/";


                    var response = await client.PostAsJsonAsync(url, body);
                    return response;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error during Enable/Disable SymbolicPoint. Message: "+ e);
                    throw;
                }
            }
        }
    }
}
