using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("compliance_group")]
    public class SupAdmin_ComplianceGroupModel
    {
        [Key]
        public int compliance_group_id { get; set; }

        public string compliance_group_name { get; set; }

        public string compliance_group_description { get; set; }

        public string compliance_group_Create_date { get; set; }

        public string compliance_group_status { get; set; }
        public int createdby { get; set; }
    }
}
