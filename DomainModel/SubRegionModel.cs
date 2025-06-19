using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("sub_regionmaster")]
    public class SubRegionModel
    {
        [Key]
        public int Sub_RegionMasterID { get; set; }
        public int RegionMasterID { get; set; }
        public string Sub_RegionName { get; set; }
        public string Description { get; set; }
        public string SubRegionStatus { get; set; }

        public int createdBy {  get; set; }
       public string createddate {  get; set; }
        public string subregiontable { get; set; }
        public string source { get; set; }


    }
}

