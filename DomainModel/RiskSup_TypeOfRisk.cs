using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("risk_typeofrisk")]
    public class RiskSup_TypeOfRisk
    {
        [Key]
        public int TypeOfRiskID { get; set; }
        public string TypeOfRiskName { get; set; }
        public string TypeOfRiskDescription { get; set; }
        public string TypeOfRiskStatus { get; set; }
        //public DateTime TypeOfRiskcreatedby { get; set; }
    }
}
