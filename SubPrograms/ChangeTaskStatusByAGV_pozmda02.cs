using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AGV_BackgroundTask.SubPrograms
{
    public class ChangeTaskStatusByAGV_pozmda02
    {
        public static async Task Update(int status, string duniTaskDetails)
        {
            var url = $"https://pozmda02.duni.org/api/DuniTasks/changeTaskStatusByAGV/{status}/{duniTaskDetails}";
            // LINK:
            //pozmda01.duni.org:81//api/DuniTasks/changeTaskStatusByAGV/{status}/{duniTaskDetails}"

            using (var client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);


                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine(response.StatusCode + " | " + "Zadanie: " + duniTaskDetails + " zaktualizowane o status: " + status + ".");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error:  Błąd podczas aktualizacji statusu zadania "+duniTaskDetails + "na status "+ status );
                    Console.WriteLine(e.Message);
                    throw;
                }
            }
        }
    }
}
