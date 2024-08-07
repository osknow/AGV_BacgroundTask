using AGV_BackgroundTask.SubPrograms;
using Opc.UaFx.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace AGV_BackgroundTask
{
    class Program
    {
        public static List<GetMissions> tasks_pozagv02;
        public static AGV_SubMachine IpointStatus = new AGV_SubMachine();
        static async Task Main(string[] args)
        {
#if !DEBUG
      
            Console.SetOut(new MyLoger("W:\\BackgroundTasks\\AGV\\logs"));
#endif
            //Status mówi o tym czy działa komunikacja z serwerem pozagv02.
            bool ststusPozagv02 = await IPOINT_Sequencer();
            if (ststusPozagv02)
            {
                        
            await SetResourses();
            // Funkcja aktualizująca zadania przetwarzane przez system AGV.
            await DuniTaskAGV();
            }
            await Main_OpcPaletyzer.SubMain_AGV_Tasks();
            Console.WriteLine("End...");
        }
 
        //**************
        // Miejsce zarządzania sekwencją na podstawie zajętośći miejca IPOINT
        //
        //**************
        static async Task<bool> IPOINT_Sequencer()
        {
            bool status_out = false;
            bool status_E_Stop = false;
            bool status_Fault = false;
            bool status_SafetyRelay = false;
            bool status_EndOfMaterial = false;
            Int32 status_TimeOfPalletOnEntrance = 0;
            //
            try
            {
                //
                var client = new OpcClient("opc.tcp://POZOPC01:5013/POZOPC_IPOINT_AGV");
                client.Connect();
                //
                var Place_status = client.ReadNode("ns=3;s=IPOINT_001_AGV.DB_AGV.Place_1 Free");
                var E_Stop = client.ReadNode("ns=3;s=IPOINT_001_AGV.DB_AGV.OWIJARKA E-STOP");
                var Alarm = client.ReadNode("ns=3;s=IPOINT_001_AGV.DB_AGV.OWIJARKA BLAD");
                var SafetyRelay = client.ReadNode("ns=3;s=IPOINT_001_AGV.DB_AGV.OWIJARKA Obwod Bezpieczenstwa");
                var EndOfMaterial = client.ReadNode("ns=3;s=IPOINT_001_AGV.DB_AGV.OWIJARKA koniec foli");
                var TimeOfPalletOnEntrance = client.ReadNode("ns=3;s=IPOINT_001_AGV.DB_AGV.Place_1_CzasPostoju");
                // FREE     : TRUE
                // OCUPATED : FALSE
                bool status = Convert.ToBoolean(Place_status.Value);

                status_out = status;
                status_E_Stop = Convert.ToBoolean(E_Stop.Value);
                status_Fault = Convert.ToBoolean(Alarm.Value);
                status_SafetyRelay = Convert.ToBoolean(SafetyRelay.Value);
                status_EndOfMaterial = Convert.ToBoolean(EndOfMaterial.Value);
                status_TimeOfPalletOnEntrance = Convert.ToInt32(TimeOfPalletOnEntrance.Value);
                client.Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error:  Błąd odczytania bazy danych OPC.");
                throw;
            }
            if (status_out == true)
            {
                //ResourceAtLocation_Euro();
                //Thread.Sleep(100);
                try { 
                HttpResponseMessage response = await IPOINT_Resourse_pozagv02.ResetPallet();
                }
                catch(Exception e)
                {
                    // Błąd komunikacji.
                    return false;
                }
                      
            }
            else
            {
                //Miejsce zajęte - zablokować oba miejca dla czytelnośći że zrobił to bacground Task.
                try
                {
                    await IPOINT_Resourse_pozagv02.SetPallet();
                }
                catch
                {
                    return false;
                }
            }
            //
            //Sprawdzenie czasu postoju palety na miejscu odkładczym IPOINT i ustwaiie alarmuw moemncie przekroczenia czasu z założonym.
            //
            List<AGV_SubMachine> SubMachines = await GetSubMachines_pozmda02.GetSUBMachinesFromPOZMDA();
            //
            //IPOINT
            IpointStatus = SubMachines[0];
            //
            if (IpointStatus.Name == "IPOINT")
            {
                if(status_TimeOfPalletOnEntrance >= IpointStatus.Setup_PaletNotPickedTime)
                {
                    IpointStatus.Error_PaletNotPicked = true;
                }
                else
                {
                    IpointStatus.Error_PaletNotPicked = false;
                }
            }
            //
            // Wysłanie emaila do Warehousu w celu powiadomienia gdy czas postoju palety jest większy niż zadany czas. 
            //
            if (IpointStatus.Setup_IPOINT_EmailPaletNotPickedTime == 0)
            {
                // Nic nie rób - blokada działania funkcji
            }
            else if(status_TimeOfPalletOnEntrance >= IpointStatus.Setup_IPOINT_EmailPaletNotPickedTime && (IpointStatus.Email_Sended == false))
            {
                    SendEmail_pozmda02.ToWarehouse();
                    IpointStatus.Email_Sended = true;
            }
            else if ((status_TimeOfPalletOnEntrance < IpointStatus.Setup_IPOINT_EmailPaletNotPickedTime) && (IpointStatus.Email_Sended == true))
            {
                IpointStatus.Email_Sended = false;
            }
            //
            IpointStatus.E_Stop = status_E_Stop;
            IpointStatus.Fault = status_Fault;
            IpointStatus.EndOfMaterial = status_EndOfMaterial;
            IpointStatus.SafetyRelay = status_SafetyRelay;
            IpointStatus.UpdatedTime = DateTime.Now;
            IpointStatus.UpdatedTime = DateTime.Now;
            IpointStatus.Real_PaletNotPickedTime = status_TimeOfPalletOnEntrance;

            try { 
            await PostSubMachines_pozmda02.PostMachinesToPOZMDA(SubMachines[0]);
            }
            catch
            {
                return false;
            }
            return true;
        }

 
        static async Task SetResourses()
        {
            using (var client = new HttpClient())
            {
                var url_ResourcesAtLocation = "https://pozagv02.duni.org:1234/api/ResourceAtLocation";
                var url_LoadAtLocation = "https://pozagv02.duni.org:1234/api/LoadAtLocation";

                var jsonserialize = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                client.DefaultRequestHeaders.Add("ApiKey", "C1XUN3agvZ9P2ER");
                client.DefaultRequestHeaders.Add("Content", "application/json");

                var PSM_038_ENG = new ResourceAtLocation()
                {
                    symbolicPointId = 3001,
                    resourceType = 2,
                    amount = 8,
                    shelfId = -1
                };
                var PSM_054_PALL = new ResourceAtLocation()
                {
                    symbolicPointId = 2001,
                    resourceType = 4,
                    amount = 8,
                    shelfId = -1
                }; ;

                var response_2 = await client.PostAsJsonAsync(url_ResourcesAtLocation, PSM_038_ENG);


                if (response_2.IsSuccessStatusCode)
                {
                    string responseString = await response_2.Content.ReadAsStringAsync();
                }
                else
                {
                    Console.WriteLine($"{response_2.StatusCode} , {response_2.RequestMessage}");
                }

                var response_3 = await client.PostAsJsonAsync(url_ResourcesAtLocation, PSM_054_PALL);

                
                if (response_3.IsSuccessStatusCode)
                {
                    string responseString = await response_3.Content.ReadAsStringAsync();
                }
                else
                {
                    Console.WriteLine($"{response_3.StatusCode} , {response_3.RequestMessage}");
                }

            }
        }
        
        
        //
        //DUNI TAKS AGV
        //
  
        
        static async Task ChangeTaskStatusByAGV(int status , string duniTaskDetails)
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
                        Console.WriteLine(response.StatusCode+" | "+"Zadanie: "+ duniTaskDetails + " zaktualizowane o status: " + status+".");
                    }
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }
        static async Task DuniTaskAGV()
        {
            // Lista zadań AGV z pozagv02
            tasks_pozagv02 = await GetMissions_pozagv02.Get();
            //Lista zadań  AGV z pozmda01
            List<GetCurrentTask> tasksNew_pozmda01 = await GetMissions_pozmda02.AGV();
            List<GetCurrentTask> tasksOpen_pozmda01 = new List<GetCurrentTask> { };
            //   
            //Zmiana statusu zadań aktualnie wykonywanych i otrzymanych przez serwer pozagv02

            foreach (GetMissions item_pozagv02 in tasks_pozagv02)
            {
                string item_pozagv02_Id_String = Convert.ToString(item_pozagv02.id);
                foreach (GetCurrentTask item_pozmda01 in tasksNew_pozmda01)
                {
                    //
                    //Lista zadań otwartych / przetwarzanych (Krok 1 lub Krok 2)
                    //
                    if (!(item_pozmda01.statusText=="newTask"))
                    {
                        tasksOpen_pozmda01.Add(item_pozmda01);
                        //
                        bool output_status = false;
                        foreach(var obj in tasks_pozagv02)
                        {
                            string obj_Id_String = Convert.ToString(obj.id);
                            if (((obj.State == "Executing") || (obj.State == "Interrupted")) && (obj_Id_String == item_pozmda01.details))
                            {
                                output_status = true;
                            }
                        }
                        //Kończenie zadań otwartych 
                        if (output_status==false)
                        {
                            await ChangeTaskStatusByAGV(3, item_pozmda01.details);
                            DateTime Time = DateTime.Now;
                            Console.WriteLine($"Zadanie: {item_pozmda01.details} skasowane poprawnie.");
                        }

                        
                    }

                    if (item_pozagv02_Id_String == item_pozmda01.details)
                    {
                        //
                        //Do testów bez przetwarzania zadania niezbędne jest zanegowanie tego warunku
                        //
                        if(item_pozagv02.State== "Executing")
                        {
                            //
                            // Aktualizacja zadania o status "W trakcie"
                            //
                            await ChangeTaskStatusByAGV(1, item_pozagv02_Id_String);
                            //Sprawdzenie ilośći kroków do wykonania w danym zadaniu
                            int length = item_pozagv02.Steps.Count;
                            // 
                            for(int i =0; i<=length-1; i++)
                            {
                                //*  Kroki:
                                //i=0 Pickup
                                //i=1 Dropoff
                                // Narazie na stan 22,09,2023 nie posiadamy więcej kroków przy zadaniu
                                //
                                //
                                //Do testów bez przetwarzania zadania niezbędne jest zanegowanie tego warunku
                                //
                                if (item_pozagv02.Steps[i].StepStatus == "Complete")
                                {
                                    if (i==0)
                                    {
                                        //
                                        //Krok 2 nie zostanie zasygnalizoway ponieważ zadanie znika z listy gdy wykona się osatni krok zadania.
                                        //Tak więc gdy kroków w zadaniu będzie 3 lub więcej wtedy wszystkie do ostatniego będą sygnalizowane.
                                        //
                                        await ChangeTaskStatusByAGV(2, item_pozagv02_Id_String);
                                    }

                                }

                                
                            }

                        }
                    }
                }
            }
        }


        // *************************************************************
        // Funkcja do kasowania WSZYSTKICH zadań 
        // UWAGA
        // NIE UŻYWAĆ !!
        static async Task ClearAllPos()
        {
            List<GetCurrentTask> tasksNew_pozmda01 = await GetMissions_pozmda02.AGV();
            foreach(var obj in tasksNew_pozmda01)
            {
                await ChangeTaskStatusByAGV(3, obj.details);
            }
        }
        // *************************************************************
    }
}
