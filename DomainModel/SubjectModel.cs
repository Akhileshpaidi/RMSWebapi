using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("subject")] 
    public class SubjectModel
    {

        [Key]

        public int Subject_id { get; set; }
        public string Subject_Name { get; set; }
        public string Subject_Desc { get; set; }
        public string Subject_Status { get; set; }

        public String Subject_CreatedDate { get; set; }

        
    }
}
