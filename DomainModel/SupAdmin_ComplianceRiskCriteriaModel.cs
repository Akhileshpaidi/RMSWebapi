using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("compliance_risk_classification_criteria")]
    public class SupAdmin_ComplianceRiskCriteriaModel

    {
        [Key]
        public int compliance_risk_criteria_id { get; set; }
        public string compliance_risk_criteria_name { get; set; }
        public string compliance_risk_criteria_description { get; set; }
        public string compliance_risk_criteria_date { get; set; }
        public string compliance_risk_criteria_status { get; set; }
        public int createdby { get; set; }
       

    }
}
