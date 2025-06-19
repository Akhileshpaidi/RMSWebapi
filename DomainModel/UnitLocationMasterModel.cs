using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("unit_location_master")]

    public class UnitLocationMasterModel
    {
        [Key]
        public int Unit_location_Master_id { get; set; }
        public string Unit_location_Master_name { get; set; }
        public string Unit_location_Master_Desc { get; set; }
        public int Entity_Master_id { get; set; }
        public string Unit_location_Master_Status { get; set; }
        public string Unit_location_Master_createdDate { get; set; }

    }
}
