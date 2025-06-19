using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("riskAssessmentsubtypemaster")]
    public class riskAssessmenttemplatesubtypemodel
    {
        [Key]
        public int subtypeID {  get; set; }


        public int typeID { get; set; }

        public string subtypename { get; set; }

        public string description { get; set; }

        public string status { get; set; }

        public int createdBy { get; set; }

        public string source { get; set; }

        public string createddate { get; set; }
    }
}
