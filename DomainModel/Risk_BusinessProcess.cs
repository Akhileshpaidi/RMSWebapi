using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("risk_businessprocess")]
    public class Risk_BusinessProcess
    {
        [Key]
        public int businessprocessID { get; set; }
        public int entityid { get; set; }
        public int departmentid { get; set; }
        public int? riskBusinessfunctionid { get; set; }
        public int unitlocationid { get; set; }
        public string BusinessProcessName { get; set; }
        public string BusinessProcessDescription { get; set; }
        public string BuinessProcessStatus { get; set; }

    }
}
