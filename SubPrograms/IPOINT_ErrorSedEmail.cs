using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AGV_BackgroundTask.SubPrograms
{
    class SendEmail_pozmda02
    {
        public static async Task<HttpResponseMessage> ToWarehouse()
        {
            string HttpSerwerURI = "https://pozmda02.duni.org/api/SendEmail/IpointNOTworkEmail";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    return  await client.GetAsync(HttpSerwerURI);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
