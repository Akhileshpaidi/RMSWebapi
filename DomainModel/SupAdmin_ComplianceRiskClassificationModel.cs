using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("compliance_risk_classification")]
    public class SupAdmin_ComplianceRiskClassificationModel
    {
        [Key]
        public int compliance_risk_classification_id { get; set; }
        public string compliance_risk_classification_name { get; set; }
        public string compliance_risk_classification_description { get; set; }
        public string compliance_risk_classification_date { get; set; }
        public string compliance_risk_classification_status { get; set; }
        public int createdby { get; set; }
    }
}
