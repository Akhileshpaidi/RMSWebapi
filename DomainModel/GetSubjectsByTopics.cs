using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    public class GetSubjectsByTopics
    {
        [Key]

        public int Subject_id { get; set; }
        public string Subject_Name { get; set; }

        public List<Topics> topics { get; set; }
    }

    public class Topics
    {
        public int Topic_id { get; set; }
        public string Topic_Name { get; set; }
        public int Subject_id { get; set; }
    }
}

