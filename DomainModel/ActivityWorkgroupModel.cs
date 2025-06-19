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
    [Table("activityworkgroup")]
    public class ActivityWorkgroupModel
    {
        [Key]
      public int activity_Workgroup_id {  get; set; }
     public string name_ActivityWorkgroup {  get; set; }
        public string desc_ActivityWorkgroup { get; set; }
        public string locationdepartmentmappingid { get; set; }
        public int  roles { get; set; } 
        public int createdby { get; set; }
        public string status { get; set; }

        public string  createddate { get; set; }

        public  string unigueActivityid {  get; set; }

    }
    public class UpdateData3
    {
        public ActivityWorkgroupModel activityWorkgroupModels { get; set; }
        // Other properties may be defined here if needed
    }
}
