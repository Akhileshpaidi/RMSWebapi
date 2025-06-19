using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("roletypemaster")]
    public  class typeofrolemodel
    {
        [Key]
        public int roletypeid { get; set; }
        public string roletypename { get; set; }
        public int task_id { get; set; }
    }
}
