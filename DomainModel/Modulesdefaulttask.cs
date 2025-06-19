using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{

    [Table("defaulttaskuser")]

    public class defualtmodulerole
    {
        [Key]
        public int idDefaultTaskuser { get; set; }
        public int task_id { get; set; }
        public int ROLE_ID { get; set; }
    

    }
}
