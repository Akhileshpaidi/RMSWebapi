using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("user_workgroup_mapping")]
    public class useractivitymapping
    {
        [Key]
      public int user_workgroup_mapping_id {  get; set; }
        public string user_workgroup_mapping_name { get; set; }
        public string  user_workgroup_mapping_desc { get; set; }
        public int  activityworkgroup_id { get; set; }
        public string  userid { get; set; }
        public int createdby { get; set; }
        public string  createddate {  get; set; }
        public string  status { get; set; } 

        public string useractivitymappingunigueid {  get; set; }
    }

    public class UpdateData1
    {
        public useractivitymapping useractivitymappingmodels { get; set; }
        // Other properties may be defined here if needed
    }
}
