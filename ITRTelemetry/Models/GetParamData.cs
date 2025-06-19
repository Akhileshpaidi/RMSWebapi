using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITR_TelementaryAPI.Models
{
    public class GetParamData
    {
        public int parameterid { get; set; }
        public string name { get; set; }
        public string mean { get; set; }
        public int min { get; set; }
        public int max { get; set; }
       
    }
    public class GetParamDataByMission
    {
        public int tsid { get; set; }
        public string tsname { get; set; }
        public int priority { get; set; }
        public string port { get; set; }

    }

    public class GetPacketDetails
    {

        public int PacketDetailsMasterID { get; set; }
        public int EquipmentID { get; set; }
        public string ParameterName { get; set; }
        public int StartByte { get; set; }
        public int EndByte { get; set; }
        public string DataType { get; set; }


    }

    public class GetPriorityMapping
    {
        public int PriorityMappingID { get; set; }
        public int EquipmentID { get; set; }
        public int MissionID { get; set; }
        public int TSID { get; set; }
        public int Priority { get; set; }
        public string TelemetryName { get; set; }
    }

    public class ParameterType
    {
        public int parameterTypeID { get; set; }
        public string parameterType { get; set; }

    }
    public class MultiLineParameters
    {
       public string Time { get; set; }
       public string[] ParameterID { get; set; }
        public string[] ParamData { get; set; }
    }
}
