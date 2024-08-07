using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AGV_BackgroundTask.SubPrograms;
using Newtonsoft.Json;
using Opc.UaFx.Client;

namespace AGV_BackgroundTask
{
    class Main_OpcPaletyzer

    //_________________________________________________________
    //
    //OpcNode for all Paletyzers
    //
    //_________________________________________________________


    #region PaletyzersObjectOPCDescriptions
    {
        static bool AGV_TaskExist = false;
        static bool SERVICE_TaskExist = false;
        public static Thread myNewThread;

        static OPCNode_Paletyzer OPC_PSM003 = new OPCNode_Paletyzer
        {
            MachineName = "PSM003",
            OpcNode_FullPaletPick = "ns=3;s=PSM003.PSM003_MainMachine.AGV.REQ_PelnaOdbior",
            OpcNode_EmptyPaletsDrop = "ns=3;s=PSM003.PSM003_MainMachine.AGV.REQ_PustaDostarczenia",
        };
        static OPCNode_Paletyzer OPC_PSM004 = new OPCNode_Paletyzer
        {
            MachineName = "PSM004",
            OpcNode_FullPaletPick = "ns=4;s=PSM004.PSM004_SOCO.AGV.REQ_PelnaOdbior",
            OpcNode_EmptyPaletsDrop = "ns=4;s=PSM004.PSM004_SOCO.AGV.REQ_PustaDostarczenia",
        };
        static OPCNode_Paletyzer OPC_PSM017 = new OPCNode_Paletyzer
        {
            MachineName = "PSM017",
            OpcNode_FullPaletPick = "ns=5;s=PSM017_MainMachine.AGV.PaletaPelnaDoOdbioru",
            OpcNode_EmptyPaletsDrop = "ns=5;s=PSM017_MainMachine.AGV.PotrzebaPustychPalet",
        };

        static OPCNode_Paletyzer OPC_PSM054 = new OPCNode_Paletyzer
        {
            MachineName = "PSM054",
            OpcNode_FullPaletPick = "ns=6;s=PSM054.PSM054_MainMachine.AGV.REQ_PelnaOdbior",
            OpcNode_EmptyPaletsDrop = "ns=6;s=PSM054.PSM054_MainMachine.AGV.REQ_PustaDostarczenia",
        };
        static OPCNode_Paletyzer OPC_PSM067 = new OPCNode_Paletyzer
        {
            MachineName = "PSM067",
            OpcNode_FullPaletPick = "ns=5;s=PSM006.PSM006_MainMachine.AGV.REQ_PelnaOdbior",
            OpcNode_EmptyPaletsDrop = "ns=5;s=PSM006.PSM006_MainMachine.AGV.REQ_PustaDostarczenia",
        };
        //
        //
        //
        //
        static List<OPCNode_Paletyzer> OPCNode = new List<OPCNode_Paletyzer> { OPC_PSM003, OPC_PSM004, OPC_PSM017, OPC_PSM054,OPC_PSM067};
        #endregion
        //
        //
       public static async Task SubMain_AGV_Tasks()
        {
            //Read AGV_MatrixConfiguration
            var AGV_MatrixModel =  await ReadAGVMatrix.GetMachineMatrixFromPOZMDA();
            //Read OPC signals from Paletizers
            OPC_ReadData();
            //Read Service tasks on pozmda01 server
            List<GetCurrentTask> ServiceTasks = await GetMissions_pozmda02.SERVICE();
            //Read PaletType on Machines
            var Machines = await ReadMachines.GetMachinesFromPOZMDA();
            //Current tasks from pozagv02
            var  tasks = Program.tasks_pozagv02;
            //
            foreach (var item in OPCNode )
            {
                foreach (var agv_machine in AGV_MatrixModel)
                {
                    //__________________________________________________________________________________________
                    //
                    // IPOINT Awaria - edycja sBody i edycja na pusty string pola targetLocation  
                    //
                    //__________________________________________________________________________________________
                    #region IPOINT Awaria
                    //Jeśli wystąpi błąd IPOINTa kasujemy "miejsce" IPOINT żeby zadanie  trafiło do SERWISU
                    //
                    if (Program.IpointStatus.EndOfMaterial == true || Program.IpointStatus.Fault == true || Program.IpointStatus.SafetyRelay == false || Program.IpointStatus.E_Stop == false)
                    {
                        agv_machine.ipoint = null;
                    }
                    #endregion
                    //
                    foreach (var machine in Machines)
                    {
                        if ((item.MachineName == agv_machine.name && agv_machine.name == machine.Name) || (item.MachineName == agv_machine.name && agv_machine.name == "PSM067" && machine.Name=="PSM006"))
                        {
                            // Konieczność stworzenia zadania dla AGV lub Serwisu w zależnośći od ustawień.
                            //AGV
                            if (item.REQ_FullPaletPick && agv_machine.pickActive)
                            {
                                // Zadanie dla AGV
                                #region sBody
                                var sBodySerwiceAGV = new CreateTaskPozagv02_sBody() { machineType ="", startTime="", priority=4,};
                                //
                                if (machine.PalletType == EnumPalletType.Euro || machine.PalletType == EnumPalletType.NewEuro || machine.PalletType == EnumPalletType.EuroChep || machine.PalletType == EnumPalletType.EuroJYSK)
                                {
                                    sBodySerwiceAGV.pickupLocation = agv_machine.pick;
                                    sBodySerwiceAGV.targetLocation = agv_machine.ipoint;
                                    sBodySerwiceAGV.resourceTypes = 3;
                                    sBodySerwiceAGV.targetShelfId = 1;
                                    //
                                    if (agv_machine.shelf)
                                    {
                                        sBodySerwiceAGV.pickupShelfId = 1;
                                    }
                                    else
                                    {
                                        sBodySerwiceAGV.pickupShelfId = -1;
                                    }
                                }
                                else if(machine.PalletType == EnumPalletType.Ang || machine.PalletType == EnumPalletType.AngChep )
                                {
                                    sBodySerwiceAGV.pickupLocation = agv_machine.pick;
                                    sBodySerwiceAGV.targetLocation = agv_machine.ipoint;
                                    sBodySerwiceAGV.resourceTypes = 1;
                                    sBodySerwiceAGV.targetShelfId = 2;
                                    //
                                    sBodySerwiceAGV.pickupShelfId = 2;
          
                                }
                                #endregion
                                // Sprawdzenie czy komórka w MachineMatrix NIE jest pusta: jeśli tak to zadanie z automatu do seriwsu. 
                                if (!(sBodySerwiceAGV.targetLocation == null || sBodySerwiceAGV.pickupLocation == null))
                                {
                                    // Sprawdzenie czy zadanie już nie występuje na liście zadań do wykonania dla AGV.
                                    foreach (var task in Program.tasks_pozagv02)
                                    {
                                        if(! (task.MissionType == "Wait" || task.MissionType == "Manual"))
                                        { 
                                            var finalTargetId = task.FinalTarget.Split(" ");
                                            if (finalTargetId[1] == "4001" && task.Steps[0].CurrentTarget == agv_machine.pick)
                                            {
                                                AGV_TaskExist = true;   
                                            }
                                        }
                                    }
                                    if (AGV_TaskExist == false)
                                    {
                                        //Tworzebnie palety dla systemu AGV
                                        if (sBodySerwiceAGV.targetLocation.Contains("I-POINT") && SERVICE_TaskExist == false)
                                        {
                                            var IdPoint = sBodySerwiceAGV.pickupLocation.Split(" ");
                                            string IntIdPoint = IdPoint[1].Remove(0, 1);
                                            IntIdPoint = IntIdPoint.Remove(IntIdPoint.Length - 1);
                                            int id = Int32.Parse(IntIdPoint);
                                            var pallet = new ResourceAtLocation()
                                            {
                                                symbolicPointId = id,
                                                resourceType = sBodySerwiceAGV.resourceTypes,
                                                amount = 1,
                                                shelfId = sBodySerwiceAGV.pickupShelfId
                                            };
                                            try { 
                                            CreatePallet_pozagv02.SetResourses(pallet);
                                            }
                                            catch
                                            {
                                                Console.WriteLine("Błąd przy stworzeniu palety dla serwera pozagv02 dla punktu: "+ pallet.symbolicPointId);
                                            }
                                        }
                                        HttpResponseMessage responseAGV=new HttpResponseMessage();
                                        // Tworzenie zadania
                                        try { 
                                        responseAGV = await CreateTask_pozagv02.POST(sBodySerwiceAGV);
                                        }
                                        catch
                                        {
                                            Console.WriteLine("Błąd przy stworzeniu zadania z punktu: " + sBodySerwiceAGV.pickupLocation +" do punktu: "+ sBodySerwiceAGV.targetLocation);
                                        }
                                        //
                                         
                                        if (responseAGV.IsSuccessStatusCode)
                                        {
                                            Console.WriteLine($"Utworzono zadanie AGV dla maszyny {machine.Name} z Id: {CreateTask_pozagv02.responseJSON.createdId}. | " + "{ pickupLocation:" + sBodySerwiceAGV.pickupLocation + ", pickupShelfId:" + sBodySerwiceAGV.pickupShelfId + ", targetLocation:" + sBodySerwiceAGV.targetLocation + ", targetShelfId:" + sBodySerwiceAGV.targetShelfId + ", resourceTypes:" + sBodySerwiceAGV.resourceTypes + "}");
                                            // Zadanie na serwer POZMDA01
                                            var sBodySerwice = new CreateTaskPozmda01_sBody() { MachineNumber = $"{item.MachineName}", Name = "AGV_Odbiór pełnej palety_AUTO_" + machine.PalletType.ToString(), Details = $"{ CreateTask_pozagv02.responseJSON.createdId}", Priority = 0 };
                                            var response = await CreateTask_pozmda02.POST(sBodySerwice);
                                        }
                                        
                                    }
                                }
                                // Komórka w MachineMatrix PUSTA: zadanie z automatu do seriwsu. 
                                // Przypadek gdy przełącznik "Funkcja alarmowa IPOINTA" jest przełączony i AGV nie będzi epodejmował działań- tylko SERWIS.
                                else
                                {
                                    var sBodySerwice = new CreateTaskPozmda01_sBody() { MachineNumber = $"{item.MachineName}", Name = "SERVICE_Odbiór pełnej palety_AUTO ", Details = machine.PalletType.ToString(), Priority = 0 };
                                    // Sprawdzenie czy zadanie już nie występuje na liście zadań do wykonania dla AGV.
                                    foreach (var task in ServiceTasks)
                                    {
                                        if (task.machineNumber == sBodySerwice.MachineNumber && task.name == sBodySerwice.Name)
                                        {
                                            SERVICE_TaskExist = true;
                                        }
                                        else { 
                                            // Sprawdzenie czy zadanie już nie występuje na liście zadań do wykonania dla AGV.
                                            foreach (var task2 in Program.tasks_pozagv02)
                                            {
                                                if (!(task2.MissionType == "Wait" || task2.MissionType == "Manual"))
                                                {
                                                    //
                                                    var finalTargetId = task2.FinalTarget.Split(" ");
                                                    var CurrentTarget_name = task2.Steps[0].CurrentTarget.Split(" ");
                                                    var IntCurrentTarget_name = Int32.Parse(CurrentTarget_name[1]);
                                                    //
                                                    var IdAgv = agv_machine.pick.Split(" ");
                                                    string IntIdAGV = IdAgv[1].Remove(0, 1);
                                                    IntIdAGV = IntIdAGV.Remove(IntIdAGV.Length - 1);
                                                    int id = Int32.Parse(IntIdAGV);
                                                    //
                                                    if ((finalTargetId[1] == "4001" && task2.Steps[0].CurrentTarget == agv_machine.pick) || (finalTargetId[1] == "4001" && IntCurrentTarget_name == id))
                                                    {
                                                        SERVICE_TaskExist = true;
                                                    }
                                                }
                                            }
                                        }

                                    }
                                    if(SERVICE_TaskExist == false)
                                    { 
                                    // Zadanie dla SERWISU 
                                        var response = await CreateTask_pozmda02.POST(sBodySerwice);
                                        if (response.IsSuccessStatusCode)
                                        {
                                            Console.WriteLine($"Utworzono zadanie dla SERWISU dla maszyny {machine.Name}. | " + "{ Details:" + sBodySerwice.Details + ", Name:" + sBodySerwice.Name + "}");
                                        }
                                    }
                                }
                                //
                                AGV_TaskExist = false;
                                SERVICE_TaskExist = false;



                            }
                            //SERVICE
                            else if (item.REQ_FullPaletPick && (!agv_machine.pickActive))
                            {
                                var sBodySerwice = new CreateTaskPozmda01_sBody() { MachineNumber = $"{item.MachineName}", Name = "SERVICE_TESTY Odbiór pełnej palety ", Details = machine.PalletType.ToString(), Priority = 0 };
                                // Sprawdzenie czy zadanie już nie występuje na liście zadań do wykonania dla AGV.
                                foreach (var task in ServiceTasks)
                                {
                                    if (task.machineNumber == sBodySerwice.MachineNumber && task.name == sBodySerwice.Name)
                                    {
                                        SERVICE_TaskExist = true;
                                    }
                                }
                                if (SERVICE_TaskExist == false)
                                {
                                    // Zadanie dla SERWISU

                                    var response = await CreateTask_pozmda02.POST(sBodySerwice);
                                    if (response.IsSuccessStatusCode)
                                    {
                                        Console.WriteLine($"Utworzono zadanie dla SERWISU dla maszyny {machine.Name}.  | " + "{ Details:" + sBodySerwice.Details + ", Name:" + sBodySerwice.Name + "}");
                                    }
                                }
                                SERVICE_TaskExist = false;
                            }
                            //AGV
                            if (item.REQ_EmptyPaletsDrop && agv_machine.dropActive)
                            {
                                // Zadanie dla AGV
                                var sBodySerwiceAGV = new CreateTaskPozagv02_sBody() { machineType = "", startTime = "", priority = 4, };
                                //
                                #region sBody
                                if (machine.PalletType == EnumPalletType.Euro || machine.PalletType == EnumPalletType.NewEuro || machine.PalletType == EnumPalletType.EuroChep || machine.PalletType == EnumPalletType.EuroJYSK)
                                {
                                    sBodySerwiceAGV.pickupLocation = agv_machine.pp_e;
                                    sBodySerwiceAGV.targetLocation = agv_machine.drop;
                                    sBodySerwiceAGV.resourceTypes = 3;
                                    sBodySerwiceAGV.pickupShelfId = -1;
                                    //
                                    if (agv_machine.shelf)
                                    {
                                        sBodySerwiceAGV.targetShelfId = 1;
                                    }
                                    else
                                    {
                                        sBodySerwiceAGV.targetShelfId = -1;
                                    }
                                }
                                else if (machine.PalletType == EnumPalletType.Ang || machine.PalletType == EnumPalletType.AngChep)
                                {
                                    sBodySerwiceAGV.pickupLocation = agv_machine.pp_a;
                                    sBodySerwiceAGV.targetLocation = agv_machine.drop;
                                    sBodySerwiceAGV.resourceTypes = 1;
                                    sBodySerwiceAGV.pickupShelfId = -1;
                                    //
                                    sBodySerwiceAGV.targetShelfId = 2;

                                }
                                #endregion
                                if (!(sBodySerwiceAGV.targetLocation == null || sBodySerwiceAGV.pickupLocation == null))
                                {
                                    // Sprawdzenie czy zadanie już nie występuje na liście zadań do wykonania dla AGV.
                                    foreach (var task in Program.tasks_pozagv02)
                                    {
                                        var pickId = agv_machine.drop.Split(" ");
                                        string stringPickId = pickId[1].Remove(pickId[1].Length - 1);
                                        stringPickId = stringPickId.Substring(1);
                                        var finalTargetId = task.FinalTarget.Split(" ");
                                        if (stringPickId == finalTargetId[1])
                                        {
                                            AGV_TaskExist = true;
                                        }
                                    }
                                    if(AGV_TaskExist == false)
                                    { 
                                        var responseAGV = await CreateTask_pozagv02.POST(sBodySerwiceAGV);
                                        //
                                        if (responseAGV.IsSuccessStatusCode)
                                        {
                                            Console.WriteLine($"Utworzono zadanie dla maszyny {machine.Name} z Id: {CreateTask_pozagv02.responseJSON.createdId}. | " + "{ pickupLocation:" + sBodySerwiceAGV.pickupLocation + ", pickupShelfId:" + sBodySerwiceAGV.pickupShelfId + ", targetLocation:" + sBodySerwiceAGV.targetLocation + ", targetShelfId:" + sBodySerwiceAGV.targetShelfId + ", resourceTypes:" + sBodySerwiceAGV.resourceTypes + "}");
                                            //Zadanie na serwer POZMDA01
                                            var sBodySerwice = new CreateTaskPozmda01_sBody() { MachineNumber = $"{item.MachineName}", Name = "AGV_Dostarczenie pustej palety_AUTO_" + machine.PalletType.ToString(), Details = $"{ CreateTask_pozagv02.responseJSON.createdId}", Priority = 0 };
                                            var response = await CreateTask_pozmda02.POST(sBodySerwice);
                                        }
                                    }
                                }
                                else
                                {
                                    var sBodySerwice = new CreateTaskPozmda01_sBody() { MachineNumber = $"{item.MachineName}", Name = "SERVICE_Dostarczenie pustej palety_AUTO ", Details = machine.PalletType.ToString(), Priority = 0 };
                                    // Sprawdzenie czy zadanie już nie występuje na liście zadań do wykonania dla AGV.
                                    foreach (var task in ServiceTasks)
                                    {
                                        if (task.machineNumber == sBodySerwice.MachineNumber && task.name == sBodySerwice.Name)
                                        {
                                            SERVICE_TaskExist = true;
                                        }
                                    }
                                    if (SERVICE_TaskExist == false)
                                    {
                                        // Zadanie dla SERWISU
                                        var response = await CreateTask_pozmda02.POST(sBodySerwice);
                                        if (response.IsSuccessStatusCode)
                                        {
                                            Console.WriteLine($"Utworzono zadanie dla SERWISU dla maszyny {machine.Name}. | " + "{ Details:" + sBodySerwice.Details + ", Name:" + sBodySerwice.Name + "}");
                                        }
                                    }
                                }
                                AGV_TaskExist = false;
                                SERVICE_TaskExist = false;
                            }
                            //SERVICE
                            else if (item.REQ_EmptyPaletsDrop && (!agv_machine.dropActive))
                            {
                                var sBodySerwice = new CreateTaskPozmda01_sBody() { MachineNumber = $"{item.MachineName}", Name = "SERVICE_TESTY Dostarczenie pustej palety ", Details = machine.PalletType.ToString(), Priority = 0 };
                                // Sprawdzenie czy zadanie już nie występuje na liście zadań do wykonania dla AGV.
                                foreach (var task in ServiceTasks)
                                {
                                    if (task.machineNumber == sBodySerwice.MachineNumber && task.name == sBodySerwice.Name)
                                    {
                                        SERVICE_TaskExist = true;
                                    }
                                }
                                if (SERVICE_TaskExist == false)
                                {
                                    // Zadanie dla SERWISU
                                    var response = await CreateTask_pozmda02.POST(sBodySerwice);
                                    if (response.IsSuccessStatusCode)
                                    {
                                        Console.WriteLine($"Utworzono zadanie dla SERWISU dla maszyny {machine.Name}. | " + "{ Details:" + sBodySerwice.Details + ", Name:" + sBodySerwice.Name + "}");
                                    }
                                }
                                SERVICE_TaskExist = false;
                            }
                        }
                    }
                }
            }
        }

        static async Task OPC_ReadData()
        {  
                foreach (var item in OPCNode)
                {
                    try
                    {
                        //Paletyzers REQUEST Signals 
                        var opc_client = new OpcClient("opc.tcp://POZOPC01:5023/Softing_dataFEED_OPC_Suite_POZOPC_AGV");
                        opc_client.Connect();
                        //
                        var FullPalletToPick = opc_client.ReadNode(item.OpcNode_FullPaletPick);
                        var EmptyPalletToDrop = opc_client.ReadNode(item.OpcNode_EmptyPaletsDrop);
                        //
                        item.REQ_FullPaletPick = Convert.ToBoolean(FullPalletToPick.Value);
                        item.REQ_EmptyPaletsDrop = Convert.ToBoolean(EmptyPalletToDrop.Value);
                        opc_client.Disconnect();
                        Thread.Sleep(100);  
                    }
                    catch (Exception e)
                    {

                        Console.WriteLine($"Problem with machine: {item.MachineName} // Type: {0}. Message : {1}", e.GetType(), e.Message);
                        throw;
                    }

            }
            
        }


        
    }

}
