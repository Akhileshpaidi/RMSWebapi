using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace DomainModel
{
    [Table("risk_statementfilemaster")]
    public class Risk_StatementFileMaster
    {
        [Key]
        public int StatementFileID { get; set; }
        public int RiskStatementID { get; set; }
        public string FileCategory { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string CreatedDate { get; set; }
        public string Status { get; set; }
        public string CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public string RiskLibrarySeq { get; set; }
       //public int RiskRegisterMasterID { get; set; }

    }
}
