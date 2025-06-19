using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("entity_master")]
    public class UnitMasterModel
    {
        [Key]
        public int Entity_Master_id { get; set; }
        public string Entity_Master_Name { get; set; }
        public string Entity_Master_Desc { get; set; }
        public string Entity_Master_Status { get; set; }
        public string Entity_Master_createdDate { get; set; }


    }
}
