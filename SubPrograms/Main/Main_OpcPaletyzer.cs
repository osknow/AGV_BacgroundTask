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
        //static List<OPCNode_Paletyzer> OPCNode = new List<OPCNode_Paletyzer> { OPC_PSM003, OPC_PSM004, OPC_PSM017, OPC_PSM054,OPC_PSM067};
        static List<OPCNode_Paletyzer> OPCNode = new List<OPCNode_Paletyzer> {  OPC_PSM054};
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
            //
            int MissionPickupId = 0;
            int MissionPickupShelfId = 0;
            int MissionPickupRequiredLoadType = 0;
            int MissionDropoffId = 0;
            int MissionDropoffShelfId = 0;
            int MissionDropoffRequiredLoadType = 0;
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
                    //Jeśli wystąpi błąd IPOINTa kasujemy "miejsce" IPOINT żeby zadanie  trafiło do SERWISU __ JEŚLI funkcja alarmowa jest aktywna !!!
                    //
                    if ((Program.IpointStatus.EndOfMaterial == true || Program.IpointStatus.Fault == true || Program.IpointStatus.SafetyRelay == false || Program.IpointStatus.E_Stop == false) && Program.IpointStatus.SpecialAlarmFunction)
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
                            //AGV FUll
                            if (item.REQ_FullPaletPick && agv_machine.pickActive)
                                {
                                // Zadanie dla AGV
                                #region sBody
                                var sBodySerwiceAGV = new CreateTaskPozagv02_sBody() { machineType ="", startTime="", priority=4,};
                                //
                                if (machine.PalletType == EnumPalletType.Euro || machine.PalletType == EnumPalletType.NewEuro || machine.PalletType == EnumPalletType.EuroChep || machine.PalletType == EnumPalletType.EuroJYSK)
                                {
                                    sBodySerwiceAGV.pickupLocation = agv_machine.pick;
                                        MissionPickupId = Convert.ToInt16(agv_machine.pick);
                                    sBodySerwiceAGV.targetLocation = agv_machine.ipoint;
                                        MissionDropoffId = Convert.ToInt16(agv_machine.ipoint);
                                    sBodySerwiceAGV.resourceTypes = 3;
                                        MissionDropoffRequiredLoadType = 3;
                                        MissionPickupRequiredLoadType = 3;
                                    sBodySerwiceAGV.targetShelfId = 1;
                                        MissionDropoffShelfId = 1;
                                    //
                                    if (agv_machine.shelf)
                                    {
                                        sBodySerwiceAGV.pickupShelfId = 1;
                                            MissionPickupShelfId = 1;
                                    }
                                    else
                                    {
                                        sBodySerwiceAGV.pickupShelfId = -1;
                                            MissionPickupShelfId = -1;
                                    }
                                }
                                else if(machine.PalletType == EnumPalletType.Ang || machine.PalletType == EnumPalletType.AngChep )
                                {
                                    sBodySerwiceAGV.pickupLocation = agv_machine.pick;
                                        MissionPickupId = Convert.ToInt16(agv_machine.pick);
                                    sBodySerwiceAGV.targetLocation = agv_machine.ipoint;
                                        MissionDropoffId = Convert.ToInt16(agv_machine.ipoint);
                                    sBodySerwiceAGV.resourceTypes = 1;
                                        MissionDropoffRequiredLoadType = 1;
                                        MissionPickupRequiredLoadType = 1;
                                    sBodySerwiceAGV.targetShelfId = 2;
                                        MissionDropoffShelfId = 2;
                                    //
                                    sBodySerwiceAGV.pickupShelfId = 2;
                                        MissionPickupShelfId = 2;
                                }
                                #endregion
                                // Sprawdzenie czy komórka w MachineMatrix NIE jest pusta: jeśli tak to zadanie z automatu do seriwsu. 
                                if (!(sBodySerwiceAGV.targetLocation == null || sBodySerwiceAGV.pickupLocation == null))
                                {
                                    // Sprawdzenie czy zadanie już nie występuje na liście zadań do wykonania dla AGV.
                                    foreach (var task in Program.tasks_pozagv02)
                                    {
                                        if(! (task.MissionType == "Wait" || task.MissionType == "Manual" || task.MissionType == "Charge"))
                                        { 
                                            var finalTargetId = task.FinalTarget.Split(" ");
                                            if (finalTargetId[1] == "4001" && task.Steps[0].CurrentTarget.Contains(agv_machine.pick))
                                            {
                                                AGV_TaskExist = true;   
                                            }
                                        }
                                    }
                                    if (AGV_TaskExist == false)
                                    {
                                        //Tworzenie palety dla systemu AGV
                                        if (sBodySerwiceAGV.targetLocation.Contains("4001") && SERVICE_TaskExist == false)
                                        {
                                            var pallet = new ResourceAtLocation()
                                            {
                                                symbolicPointId = Convert.ToInt16(sBodySerwiceAGV.pickupLocation),
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
                                            #region MissionBody

                                            //
                                            var sBodyMissinsAGV = new
                                            {
                                                ExternalId = "Zadanie PALL Pick: "+item.MachineName,
                                                Name = "DUNI_TASK_AGV",
                                                Options = new
                                                {
                                                    Priority = 4
                                                },
                                                Steps = new object[]
                                                    {
                                                    new {
                                                        StepType = "Pickup",
                                                        Options = new
                                                        {
                                                            Load = new
                                                            {
                                                                RequiredLoadStatus = "LoadAtLocation",
                                                                RequiredLoadType = MissionPickupRequiredLoadType
                                                            },
                                                            SortingRules = new[] { "Priority", "Closest" }
                                                        },
                                                        AllowedTargets = new[]
                                                        {
                                                            new { Id = MissionPickupId, ShelfId = MissionPickupShelfId }
                                                        }
                                                    }, // <- Brakujący nawias klamrowy został dodany tutaj
                                                    new
                                                    {
                                                        StepType = "Dropoff",
                                                        Options = new
                                                        {
                                                            Load = new
                                                            {
                                                                RequiredLoadType = MissionDropoffRequiredLoadType,
                                                                RequiredLoadStatus = "LocationHasRoom"
                                                            }
                                                        },
                                                        AllowedTargets = new[]
                                                        {
                                                            new { Id = MissionDropoffId, ShelfId = MissionDropoffShelfId }
                                                        },
                                                        AllowedWaits = new[]
                                                        {
                                                            new { Id = 6010 },
                                                            new { Id = 6009 }
                                                        }
                                                    }
                                                    }
                                            };

                                            #endregion
                                            //
                                            responseAGV = await CreateMission_pozagv02.POST(sBodyMissinsAGV);
                                        
                                        }
                                        catch
                                        {
                                            Console.WriteLine("Błąd przy stworzeniu zadania z punktu: " + sBodySerwiceAGV.pickupLocation +" do punktu: "+ sBodySerwiceAGV.targetLocation);
                                        }
                                        //
                                         
                                        if (responseAGV.IsSuccessStatusCode && CreateMission_pozagv02.responseJSON.Success)
                                        {
                                            Console.WriteLine($"Utworzono zadanie AGV dla maszyny {machine.Name} z Id: {CreateMission_pozagv02.responseJSON.InternalId}. | " + "{ pickupLocation:" + sBodySerwiceAGV.pickupLocation + ", pickupShelfId:" + sBodySerwiceAGV.pickupShelfId + ", targetLocation:" + sBodySerwiceAGV.targetLocation + ", targetShelfId:" + sBodySerwiceAGV.targetShelfId + ", resourceTypes:" + sBodySerwiceAGV.resourceTypes + "}");
                                            // Zadanie na serwer POZMDA01
                                            var sBodySerwice = new CreateTaskPozmda01_sBody() { MachineNumber = $"{item.MachineName}", Name = "AGV_Odbiór pełnej palety_AUTO " + machine.PalletType.ToString(), Details = $"{ CreateMission_pozagv02.responseJSON.InternalId}", Priority = 0 };
                                            var response = await CreateTask_pozmda02.POST(sBodySerwice);
                                        }
                                        
                                    }
                                }
                                // Komórka w MachineMatrix PUSTA: zadanie z automatu do seriwsu. 
                                // Przypadek gdy przełącznik "Funkcja alarmowa IPOINTA" jest przełączony i AGV nie będzi epodejmował działań- tylko SERWIS.
                                else
                                {
                                    var sBodySerwice = new CreateTaskPozmda01_sBody() { MachineNumber = $"{item.MachineName}", Name = "SERVICE_Odbiór pełnej palety AUTO ", Details = machine.PalletType.ToString(), Priority = 0 };
                                    // Sprawdzenie czy zadanie już nie występuje na liście zadań do wykonania dla AGV i dla Serwisu.
                                    foreach (var task in ServiceTasks)
                                    { 
                                        if (task.machineNumber == sBodySerwice.MachineNumber)
                                        {
                                            //Sprawdzenie czy task istnieje już na tablecie serwisanta.
                                            if (task.name.Contains("Odbór"))
                                            {
                                                SERVICE_TaskExist = true;
                                            }
                                        }
 
                                        // Sprawdzenie czy zadanie już nie występuje na liście zadań do wykonania dla AGV.
                                        foreach (var task2 in Program.tasks_pozagv02)
                                        {
                                            if (!(task2.MissionType == "Wait" || task2.MissionType == "Manual"))
                                            {
                                                //
                                                var finalTargetId = task2.FinalTarget.Split(" ");
                                                if (finalTargetId[1] == "4001" && task2.Steps[0].CurrentTarget.Contains(agv_machine.pick))
                                                {
                                                    SERVICE_TaskExist = true;
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
                            //SERVICE Full
                            else if (item.REQ_FullPaletPick && (!agv_machine.pickActive))
                            {
                                var sBodySerwice = new CreateTaskPozmda01_sBody() { MachineNumber = $"{item.MachineName}", Name = "SERVICE_Odbiór pełnej palety AUTO ", Details = machine.PalletType.ToString(), Priority = 0 };
                                // Sprawdzenie czy zadanie już nie występuje na liście zadań do wykonania dla AGV.
                                foreach (var task in ServiceTasks)
                                {
                                    if (task.machineNumber == sBodySerwice.MachineNumber)
                                    {
                                        if (task.name.Contains("Odbiór"))
                                        {
                                            SERVICE_TaskExist = true;
                                        }
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
                            //AGV Empty
                            //
                            // TO DO YET
                            //
                            if (item.REQ_EmptyPaletsDrop && agv_machine.dropActive)
                            {
                                // Zadanie dla AGV
                                var sBodySerwiceAGV = new CreateTaskPozagv02_sBody() { machineType = "", startTime = "", priority = 4, };
                                //
                                #region sBody
                                if (machine.PalletType == EnumPalletType.Euro || machine.PalletType == EnumPalletType.NewEuro || machine.PalletType == EnumPalletType.EuroChep || machine.PalletType == EnumPalletType.EuroJYSK)
                                {
                                    sBodySerwiceAGV.pickupLocation = agv_machine.pp_e;
                                        MissionPickupId = Convert.ToInt16(agv_machine.pp_e);
                                    sBodySerwiceAGV.targetLocation = agv_machine.drop;
                                        MissionDropoffId = Convert.ToInt16(agv_machine.drop);
                                    sBodySerwiceAGV.resourceTypes = 3;
                                        MissionPickupRequiredLoadType = 3;
                                        MissionDropoffRequiredLoadType = 3;
                                    sBodySerwiceAGV.pickupShelfId = -1;
                                        MissionPickupShelfId = -1;
                                    //
                                    if (agv_machine.shelf)
                                    {
                                        sBodySerwiceAGV.targetShelfId = 1;
                                            MissionDropoffShelfId = 1;
                                    }
                                    else
                                    {
                                        sBodySerwiceAGV.targetShelfId = -1;
                                            MissionDropoffShelfId = -1;
                                    }
                                }
                                else if (machine.PalletType == EnumPalletType.Ang || machine.PalletType == EnumPalletType.AngChep)
                                {
                                    sBodySerwiceAGV.pickupLocation = agv_machine.pp_a;
                                        MissionPickupId = Convert.ToInt16(agv_machine.pp_e);
                                    sBodySerwiceAGV.targetLocation = agv_machine.drop;
                                        MissionPickupId = Convert.ToInt16(agv_machine.drop);
                                    sBodySerwiceAGV.resourceTypes = 1;
                                        MissionPickupRequiredLoadType = 1;
                                        MissionDropoffRequiredLoadType = 1;
                                    sBodySerwiceAGV.pickupShelfId = -1;
                                        MissionPickupShelfId = -1;
                                    //
                                    sBodySerwiceAGV.targetShelfId = 2;
                                    MissionDropoffShelfId = 2;

                                }
                                #endregion
                                if (!(sBodySerwiceAGV.targetLocation == null || sBodySerwiceAGV.pickupLocation == null))
                                {
                                    // Sprawdzenie czy zadanie już nie występuje na liście zadań do wykonania dla AGV.
                                    foreach (var task in Program.tasks_pozagv02)
                                    {
                                        if (!(task.MissionType == "Wait" || task.MissionType == "Manual" || task.MissionType == "Charge"))
                                        {
                                            if (task.Steps[1].CurrentTarget.Contains(agv_machine.drop))
                                            {
                                                AGV_TaskExist = true;
                                            }
                                        }
                                    }
                                    if(AGV_TaskExist == false)
                                    {
                                        #region MissionBody

                                        //
                                        var sBodyMissinsAGV = new
                                        {
                                            ExternalId = "Zadanie PALL Drop: " + item.MachineName,
                                            Name = "DUNI_TASK_AGV",
                                            Options = new
                                            {
                                                Priority = 4
                                            },
                                            Steps = new object[]
                                                {
                                                    new {
                                                        StepType = "Pickup",
                                                        Options = new
                                                        {
                                                            Load = new
                                                            {
                                                                RequiredLoadStatus = "LoadAtLocation",
                                                                RequiredLoadType = MissionPickupRequiredLoadType
                                                            },
                                                            SortingRules = new[] { "Priority", "Closest" }
                                                        },
                                                        AllowedTargets = new[]
                                                        {
                                                            new { Id = MissionPickupId, ShelfId = MissionPickupShelfId }
                                                        }
                                                    }, // <- Brakujący nawias klamrowy został dodany tutaj
                                                    new
                                                    {
                                                        StepType = "Dropoff",
                                                        Options = new
                                                        {
                                                            Load = new
                                                            {
                                                                RequiredLoadType = MissionDropoffRequiredLoadType,
                                                                RequiredLoadStatus = "LocationHasRoom"
                                                            }
                                                        },
                                                        AllowedTargets = new[]
                                                        {
                                                            new { Id = MissionDropoffId, ShelfId = MissionDropoffShelfId }
                                                        }
                                                    }
                                                }
                                        };

                                        #endregion
                                        //
                                        var responseAGV = await CreateMission_pozagv02.POST(sBodyMissinsAGV);

                                        //
                                        if (responseAGV.IsSuccessStatusCode)
                                        {
                                            Console.WriteLine($"Utworzono zadanie dla maszyny {machine.Name} z Id: {CreateMission_pozagv02.responseJSON.InternalId}. | " + "{ pickupLocation:" + sBodySerwiceAGV.pickupLocation + ", pickupShelfId:" + sBodySerwiceAGV.pickupShelfId + ", targetLocation:" + sBodySerwiceAGV.targetLocation + ", targetShelfId:" + sBodySerwiceAGV.targetShelfId + ", resourceTypes:" + sBodySerwiceAGV.resourceTypes + "}");
                                            //Zadanie na serwer POZMDA01
                                            var sBodySerwice = new CreateTaskPozmda01_sBody() { MachineNumber = $"{item.MachineName}", Name = "AGV_Dostarczenie pustej palety_AUTO " + machine.PalletType.ToString(), Details = $"{ CreateMission_pozagv02.responseJSON.InternalId}", Priority = 0 };
                                            var response = await CreateTask_pozmda02.POST(sBodySerwice);
                                        }
                                    }
                                }
                                else
                                {
                                    // Brak przypadku gdzie dla stosów palet pustych kasujemy
                                    // TARGET lub PICKUP PointName bo  IPOINT mający błąd / awarię nie jest problemem dla dostarczania stosów.  
                                }
                                AGV_TaskExist = false;
                                SERVICE_TaskExist = false;
                            }
                            //SERVICE Empty
                            else if (item.REQ_EmptyPaletsDrop && (!agv_machine.dropActive))
                            {
                                var sBodySerwice = new CreateTaskPozmda01_sBody() { MachineNumber = $"{item.MachineName}", Name = "SERVICE_Dostarczenie pustej palety AUTO ", Details = machine.PalletType.ToString(), Priority = 0 };
                                // Sprawdzenie czy zadanie już nie występuje na liście zadań do wykonania dla AGV.
                                foreach (var task in ServiceTasks)
                                {
                                    if (task.machineNumber == sBodySerwice.MachineNumber && task.name == sBodySerwice.Name)
                                    {
                                        if (task.name.Contains("Dostarczenie")) { 
                                            SERVICE_TaskExist = true;
                                        }
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
                        Console.WriteLine(e);
                    }

            }
            
        }


        
    }

}
