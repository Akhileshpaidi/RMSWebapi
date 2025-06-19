using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("helpdesk")]
    public class HelpModel
    {
        [Key]
        public int helpdeskID { get; set; }
        public int task_id { get; set; }
        public int ROLE_ID { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
    }
}
