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
    class IPOINT_Resourse_pozagv02
    {
        public static async Task SetPallet()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var url = "https://pozagv02.duni.org:1234/api/ResourceAtLocation";
                    client.DefaultRequestHeaders.Add("ApiKey", "C1XUN3agvZ9P2ER");
                    client.DefaultRequestHeaders.Add("Content", "application/json");
                    //
                    var data_palletEuro = new ResourceAtLocation()
                    {
                        symbolicPointId = 4001,
                        resourceType = 3,
                        amount = 1,
                        shelfId = 1
                    };
                    var data_palletAng = new ResourceAtLocation()
                    {
                        symbolicPointId = 4001,
                        resourceType = 1,
                        amount = 1,
                        shelfId = 2
                    };

                    DateTime localDate = DateTime.Now;

                    var response = await client.PostAsJsonAsync(url, data_palletEuro);
                    if (response.IsSuccessStatusCode)
                    {
                        var response_2 = await client.PostAsJsonAsync(url, data_palletAng);
                    }
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }
        public static async Task<HttpResponseMessage> ResetPallet()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("ApiKey", "C1XUN3agvZ9P2ER");
                    client.DefaultRequestHeaders.Add("Content", "application/json");
                    var url = "https://pozagv02.duni.org:1234/api/ResourceAtLocation";
                    var data_Euro = new ResourceAtLocation()
                    {
                        symbolicPointId = 4001,
                        resourceType = 3,
                        amount = 0,
                        shelfId = 1
                    };

                    var data_Ang = new ResourceAtLocation()
                    {
                        symbolicPointId = 4001,
                        resourceType = 1,
                        amount = 0,
                        shelfId = 2
                    };

                    //var dataJson = JsonSerializer.Serialize(data);

                    DateTime localDate = DateTime.Now;

                    var response = await client.PostAsJsonAsync(url, data_Euro);
                    if ((response.IsSuccessStatusCode) || !(response.IsSuccessStatusCode))
                    {
                        var response_2 = await client.PostAsJsonAsync(url, data_Ang);
                        return response_2;
                    }
                    else
                    {
                        return response;
                    }
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

    }
}
