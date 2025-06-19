using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("compliance_type")]
    public class ComplianceModel
    {
        [Key]
        public int compliance_type_id { get; set; }
        public string compliance_type_name { get; set; }
        public string compliance_type_description { get; set; }
        public string compliance_type_status { get; set; }
        public string compliance_type_create_date { get; set; }
        public string source { get; set; }

        public int createdby { get; set; }
     

    }
}
