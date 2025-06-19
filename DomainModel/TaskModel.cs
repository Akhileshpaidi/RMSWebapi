using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DomainModel
{
    [Table("task_master")]
    public class TaskModel
    {
        [Key]

        public int task_id { get; set; }
        public string task_name { get; set; }
        public string task_desc { get; set; }
        public string task_status { get; set; }

        public string task_createdDate { get; set; }
        



    }
}
