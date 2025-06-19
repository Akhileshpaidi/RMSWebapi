using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("risk_businessprocess_l2")]
    public class Risk_Sub_ProcessL2
    {
        [Key]
        public int BusinessProcessL2ID { get; set; }
        public int BusinessProcessL1ID { get; set; }
        public int businessprocessID { get; set; }
        public int? entityid { get; set; }
        public int? departmentid { get; set; }
        public int? riskBusinessfunctionid { get; set; }
        public int? unitlocationid { get; set; }
        public string BusinessSubProcessL2Name { get; set; }
        public string BusinessSubProcessL2Description { get; set; }
        public string SubProcessObjestiveL2 { get; set; }
        public string ProcessL2Status { get; set; }
    }
}
