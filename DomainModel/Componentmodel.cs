using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1.Cms;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("m_appcomponent")]
    public class Componentmodel
    {
        [Key]
        public int id { get; set; }

        public string name {  get; set; }


        public string description { get; set; }

        public  string status { get; set; }
        public int? task_id { get; set; }
        public int? roletypeid { get; set; }
        public int? createdby { get; set; }
        public DateTime? createddate { get; set; }
        public int? modifiedby { get; set; }
        public DateTime? modifieddate { get; set; }
        public string functional_activity_headers { get; set;}
        public string mandatory {  get; set;}
        public string menu_item { get; set;}
    }
}
