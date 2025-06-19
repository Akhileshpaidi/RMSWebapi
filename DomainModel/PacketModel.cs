using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainModel
{
    [Table("packetmaster")]
    public class PacketModel
    {
        [Key]
        public int PacketMasterID { get; set; }
        public string PacketID { get; set; }
        public int EquipmentID { get; set; }
        public string OriginalStatus { get; set; }
        public string LockStatus { get; set; }
        public string ParameterListSize { get; set; }
        public string Hours { get; set; }
        public string Minutes { get; set; }
        public string Seconds { get; set; }
        public string MilliSeconds { get; set; }
        public string TransmissionDate { get; set; }
    }
}
