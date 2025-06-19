using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("riskSubkeyarea")]
    public class riskquestionbankattributeSubkeyarea
    {
        [Key]
        public int subkeyID { get; set; }


        public int keyareaID { get; set; }

        public string subkeyname { get; set; }

        public string description { get; set; }

        public string status { get; set; }

        public int createdBy { get; set; }

        public string source { get; set; }

        public string createddate { get; set; }
    }
}
