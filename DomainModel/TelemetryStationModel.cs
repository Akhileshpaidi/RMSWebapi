using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DomainModel
{
    [Table("telemetrystationtable")]
    public class TelemetryStationModel
    {
        [Key]
        public int TSID { get; set; }
        public string Name { get; set; }
        public string GeoLocation { get; set; }
        public string Address { get; set; }
        public string Type { get; set; }

        public string Incharge { get; set; }

        public string InchargeContactNo { get; set; }
        public string DateTime { get; set; }
        public string Status { get; set; }
        public string Port { get; set; }
        public string IPAddress { get; set; }




    }
}
