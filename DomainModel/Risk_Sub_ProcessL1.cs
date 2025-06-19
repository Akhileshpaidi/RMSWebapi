using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("risk_businessprocess_l1")]
    public class Risk_Sub_ProcessL1
    {
        [Key]
        public int BusinessProcessL1ID { get; set; }
        public int BusinessprocessID { get; set; }
        public int entityid { get; set; }
        public int departmentid { get; set; }
        public int? riskBusinessfunctionid { get; set; }
        public int unitlocationid { get; set; }
        public string BusinessSubProcessL1Name { get; set; }
        public string BusinessSubProcessL1Description { get; set; }
        public string SubProcessObjestive { get; set; }
         public string ProcessL1Status { get; set; }
    }
}
