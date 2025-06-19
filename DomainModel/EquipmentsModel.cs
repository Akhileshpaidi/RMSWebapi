using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;


namespace DomainModel
{
    [Table("equipmentmaster")]
    public class EquipmentsModel
    {
        [Key]
        public int EquipmentID { get; set; }
        public string EquipmentName { get; set; }

        public string EquipmentClass { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; }

        public string DateTime { get; set; }
    }
}
