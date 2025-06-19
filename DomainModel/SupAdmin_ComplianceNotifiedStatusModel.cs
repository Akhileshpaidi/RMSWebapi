using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("compliance_notified_status")]
    public class SupAdmin_ComplianceNotifiedStatusModel
    {
        [Key]
        public int compliance_notified_id { get; set; }
        public string compliance_notified_name { get; set; }
        public string compliance_notified_description { get; set; }
        public string compliance_notified_date { get; set; }
        public string compliance_notified_status { get; set; }
        public int createdby { get; set; }
    }
}
