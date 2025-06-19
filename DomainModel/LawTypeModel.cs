using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("law_type")]
    public class LawTypeModel
    {
        [Key]
        public int law_type_id { get; set; }
        public string type_of_law { get; set; }
        public string law_status { get; set; }
        public string law_description { get; set; }
        public string law_create_date {  get; set; }
        public int createdby { get; set; }
        public string source { get; set; }



    }
}
