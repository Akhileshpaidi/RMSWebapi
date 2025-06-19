using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainModel
{
    [Table("equipmentparameterdata")]
    public class EquipmentParameterDetailModel
    {
        [Key]
        public int ParamDataID { get; set; }
        public string ParamData { get; set; }
        public int FlightID { get; set; }
        public DateTime Date { get; set; }
        public string UDPPacketSequenceNo { get; set; }
        public string UPDPacketID { get; set; }
        public int ParameterID { get; set; }
        public int PacketMasterID { get; set; }
       // public string ParameterName { get; set; }
    }
}
