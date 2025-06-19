using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("RiskBusinessFunction")]
    public class RiskBusinessFunctionModel
    {
        [Key]
        public int riskBusinessfunctionid {  get; set; }

        public string riskbusinessname { get; set; }

        public string riskbusinessdescription { get; set; }

        public int departmentid { get; set; }

        public int entityid { get; set;}

        public string unitlocationid { get; set; }

        public int createdby { get; set; }

        public string createddate { get; set; }

        public string status { get; set; }
    }

    public class UpdateDatabusiness
    {
        public RiskBusinessFunctionModel RiskBusinessFunctionModels { get; set; }
        // Other properties may be defined here if needed
    }
}
