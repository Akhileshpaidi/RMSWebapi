using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("riskoveralappitite")]
    public  class riskoverallappititeModel
    {

        [Key]
        public int overallriskappititeid { get; set; }


        public string RiskAppetitename { get; set; }

        public string trolerancecoparison { get; set; }


        public int departmentid { get; set; }

        public int entityid { get; set; }

        public int unitlocationid { get; set; }

        public int riskBusinessfunctionid { get; set; }


        public string businessprocessID { get; set; }

        public string trigervalue {  get; set; }

        public int acceptanceid {  get; set; }
        public int createdBy { get; set; }

        public string status { get; set; }

        public string createddate { get; set; }
    }
    public class UpdateoverallriskAppetite
    {
        public riskoverallappititeModel riskoverallappititeModels { get; set; }
        // Other properties may be defined here if needed
    }
}
