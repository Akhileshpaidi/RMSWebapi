using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("riskkeyarea")]
    public  class riskquestionbankattributekeyarea
    {
        [Key]
        public int keyareaID {  get; set; }

        public string keyName {  get; set; }

         public string keyDesc {  get; set; }
          public int createdBy {  get; set; }

        public string createdDate { get; set; }

        public string status { get; set; }


        public string source {  get; set; }

    }
}
