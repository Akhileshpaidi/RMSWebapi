using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DomainModel
{
    [Table("flighttable")]
    public class FlightModel
    {
        [Key]
        public int FlightID { get; set; }
        public int MissionID { get; set; }

      
        public string FlightNumber { get; set; }
        public string CallSign { get; set; }
        public int EquipmentID { get; set; }
        public string Purpose { get; set; }
        public string Description { get; set; }
        public string DateTime { get; set; }
        public string Status { get; set; }

        public int TSID { get; set; }

    }
}
