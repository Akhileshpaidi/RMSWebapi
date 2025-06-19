using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DomainModel
{
    [Table("missiontable")]
    public class MissionModel
    {
        [Key]
        public int MissionID { get; set; }
        public string MissionName { get; set; }

        public string Code { get; set; }
        public int EquipmentID { get; set; }
        public string EquipmentClass { get; set; }
        public string MissionDirector { get; set; }
        public string Date { get; set; }

        public string Time { get; set; }
        public string Status { get; set; }

        public string Year { get; set; }
    }
}
