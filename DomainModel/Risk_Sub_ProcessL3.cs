using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace DomainModel
{
    [Table("risk_businessprocess_l3")]
    public class Risk_Sub_ProcessL3
    {
        [Key]
        public int BusinessProcessL3ID { get; set; }
        public int BusinessProcessL2ID { get; set; }
        public int BusinessProcessL1ID { get; set; }
        public int BusinessprocessID { get; set; }
        public int? entityid { get; set; }
        public int? departmentid { get; set; }
        public int? riskBusinessfunctionid { get; set; }
        public int? unitlocationid { get; set; }
        public string BusinessSubProcessL3Name { get; set; }
        public string BusinessSubProcessL3Description { get; set; }
        public string SubProcessObjestiveL3 { get; set; }
        public string ProcessL3Status { get; set; }
    }
}
