using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGV_BackgroundTask
{
    public enum EnumPalletType
    {
        Euro = 3000000,
        NewEuro = 3000001,
        //Paleta papierowa wkrótce
        //PaperPalette = 3000002,
        EuroJYSK = 3970007,
        EuroChep = 3910006,
        Ang = 3970131,
        AngChep = 3910025

    }

    public class ResourceAtLocation
    {
        public int symbolicPointId { get; set; }
        public int resourceType { get; set; }
        public int amount { get; set; }
        public int shelfId { get; set; }

    }
    public class POST_Response
    {
        public string success { get; set; }
    }
    public class LoadAtLocation
    {
        public int symbolicPointId { get; set; }
    }
    public class Steps_GetMissions
    {
        public string StepType { get; set; }
        public string StepStatus { get; set; }
        public string CurrentTarget  { get; set; }
        public int CurrentTargetId { get; set; }
        public string WaitTarget { get; set; }
        public int TargetShelfId { get; set; }
        public string LoadType { get; set; }
        public int LoadTypeId { get; set; }
        public bool ReservingLocation { get; set; }
    }
    public class GetMissions
    {
        public int id { get; set; }
        public string MissionType { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string AssignedMachine { get; set; }
        public int AssignedMachineId { get; set; }
        public int CurrentStepIndex { get; set; }
        public string FinalTarget { get; set; }
        public int FinalTargetId { get; set; }
        public List<Steps_GetMissions> Steps { get; set; }
        
    }
    public class GetCurrentTask
    {
        public int id { get; set; }
        public string machineNumber { get; set; }
        public string name { get; set; }
        public string details { get; set; }
        public int type { get; set; }
        public int status { get; set; }
        public string statusText { get; set; }
        public int imageCounter { get; set; }
        public int priority { get; set; }
        public bool helpNeeded { get; set; }
        public string addedTime { get; set; }
        public string joinedTime { get; set; }
        public string loginTime { get; set; }
        public string endedTime { get; set; }
    }
    public class AGV_SubMachine
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool E_Stop { get; set; }
        public bool SafetyRelay { get; set; }
        public bool Fault { get; set; }
        public bool EndOfMaterial { get; set; }
        public bool SpecialAlarmFunction { get; set; }
        public int Setup_PaletNotPickedTime { get; set; }
        public int Setup_IPOINT_EmailPaletNotPickedTime { get; set; }
        public int Real_PaletNotPickedTime { get; set; }
        public bool Error_PaletNotPicked { get; set; }
        public bool Email_Sended { get; set; }
        public DateTime UpdatedTime { get; set; }

    }
    public class OPCNode_Paletyzer
    {
        public string MachineName { get; set; }
        public string OpcNode_FullPaletPick { get; set; }
        public string OpcNode_EmptyPaletsDrop { get; set; }
        public bool REQ_FullPaletPick { get; set; }
        public bool REQ_EmptyPaletsDrop { get; set; }
    }
    public class AGV_Matrix
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool active { get; set; }
        public string mp_a { get; set; }
        public string mp_e { get; set; }
        public string pp_a { get; set; }
        public string pp_e { get; set; }
        public string ipoint { get; set; }
        public bool pall { get; set; }
        public string pick { get; set; }
        public string drop { get; set; }
        public bool shelf { get; set; }
        public bool pickActive { get; set; }
        public bool dropActive { get; set; }
    }
    public class MachineModel
    {
        public int Id { get; set; }
        public bool Active { get; set; } 
        public string Name { get; set; }
        public string Group { get; set; }
        public int Priiority { get; set; }
        public string Status { get; set; }
        public string Order { get; set; }
        public string OrderDecription { get; set; }
        public string Article { get; set; }
        public EnumPalletType PalletType { get; set; }

    }
    public class AGV_sBodyTask
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }
    public class CreateTaskPozmda01_sBody
    {
        public string MachineNumber { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public int Priority { get; set; }

    }
    public class CreateTaskPozagv02_sBody
    {
        public string machineType { get; set; }
        public string startTime { get; set; }
        public string pickupLocation { get; set; }
        public string targetLocation { get; set; }
        public int resourceTypes { get; set; }
        public int pickupShelfId { get; set; }
        public int targetShelfId { get; set; }
        public int priority { get; set; }

    }
    public class TaskPozagv02_sBodyResponse
    {
        public int result { get; set; }
        public int createdId { get; set; }
    }
    public class MissionsPozagv02_sBodyResponse
    {
        public string ExternalId { get; set; }
        public int InternalId { get; set; }
        public bool Success { get; set; }
        public string Description { get; set; }
        public string ReplyResult { get; set; }
    }
}
