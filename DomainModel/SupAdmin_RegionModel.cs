using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("regionmaster")]
    public class SupAdmin_RegionModel
    {
        [Key]
        public int RegionMasterID { get; set; }
        public string RegionName { get; set; }
        public string regionDesc { get; set; }

        public DateTime? createddate { get; set; }

        public DateTime? updated_at { get; set; }

        public int createdBy { get; set; }

      
        public string regiontablename { get; set; }
        public string RegionStatus { get; set; }
        

    }
}
