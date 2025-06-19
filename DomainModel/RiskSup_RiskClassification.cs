using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("risk_riskclassification")]
    public class RiskSup_RiskClassification
    {
        [Key]
        public int RiskClassificationID { get; set; }
        public int TypeOfRiskID { get; set; }
        public string RiskClassificationName { get; set; }
        public string RiskClassificationDescription { get; set; }
        public string RiskClassificationStatus { get; set; }

    }
}
