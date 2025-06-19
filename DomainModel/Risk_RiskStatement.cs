using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace DomainModel
{
    [Table("risk_risk_statement")]
    public class Risk_RiskStatement
    {
        [Key]
        public int RiskStatementID { get; set; }
        public string RiskStatementName { get; set; }
        public string RiskDescription { get; set; }
        public string CreatedDate { get; set; }
        public string Status { get; set; }
        public int CreatedBy { get; set; }
       public string CreatedOn { get; set; }
       public string DisableReason { get; set; }

        public int? RiskRegisterStatementID { get; set; }
        public string RiskDefinition { get; set; }
        public List<Risk_StatementFileMaster> Attachments { get; set; } = new List<Risk_StatementFileMaster>();
    }

    
}
