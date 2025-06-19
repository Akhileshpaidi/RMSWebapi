using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("sub_regionmaster")]
    public class SupAdmin_SubRegionModel
    {
        [Key]
        public int Sub_RegionMasterID { get; set; }

        public string Sub_RegionName { get; set; }
        public string Description { get; set; }
        public int RegionMasterID { get; set; }
        public DateTime? createddate { get; set; }
        public string subregiontable { get; set; }
        public int createdBy { get; set; }
        public DateTime? updated_at { get; set; }
        public string SubRegionStatus { get; set; }
    }
}
