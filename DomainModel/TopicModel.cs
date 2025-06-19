using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("topic")]
    public class TopicModel
    {
        [Key]

        public int Topic_id { get; set; }
        public string Topic_Name { get; set; }
        public string Topic_Desc { get; set; }
        public string Topic_Status { get; set; }

        public String Topic_createddate { get; set; }

        public int Subject_id { get; set; }
    }
}
