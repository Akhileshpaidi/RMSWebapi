using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("doctype_master")]
    public class DocTypeMasterModel
    {
        [Key]
        public int docTypeID { get; set; }
        public string docTypeName { get; set; }
        public string docTypeDescription { get; set; }

        //[ForeignKey("doctype_master")]
        public int task_id { get; set; }
        public string DocType_Status { get; set; }
        public string Doctype_CreatedDate { get; set; }
    }
}
