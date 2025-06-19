using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("compliance_record_type")]
    public class SupAdmin_ComplianceRecordTypeModel
    {
        [Key]
        public int compliance_record_type_id { get; set; }

        public string compliance_record_name { get; set; }

        public string compliance_record_description { get; set; }

        public int compliance_createdby { get; set; }

        public string compliance_record_status { get; set; }
        public string compliance_record_create_date { get; set; }
    }
}
