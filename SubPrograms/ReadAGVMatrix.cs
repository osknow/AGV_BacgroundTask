using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AGV_BackgroundTask.SubPrograms
{
    class ReadAGVMatrix
    {

        public static async Task<List<AGV_Matrix>> GetMachineMatrixFromPOZMDA()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = "https://pozmda02.duni.org/api/AGV/AGV_MachineActiveMatrixListAll";
                    return await client.GetFromJsonAsync<List<AGV_Matrix>>(url);
                    //To POST Deserialization.
                    //HttpResponseMessage  = await client.GetAsync($"{subtaskContext.Subtask.BaseUrl}/api/RareBackgroundTask/{(int)subtaskContext.Subtask.Type}");
                    //response.EnsureSuccessStatusCode(); 
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("Error: Błąd podczas odczytu MachineMatrixa z pozmda02.");
                    Console.WriteLine(e.Message);
                    throw;
                }
            }
        }
    }
}
