using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("natureof_doc")]
    public class NatureOf_DocumentMasterModel
    {
        [Key]

        public int NatureOf_Doc_id { get; set; }
        public string NatureOf_Doc_Name { get; set; }
        public string NatureOf_Doc_Desc { get; set; }
        public string natureof_Status { get; set; }
        public string natureof_createdDate { get; set; }
    }
}
