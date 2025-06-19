using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DomainModel
{
    [Table("department_master")]
    public class DepartmentModel
    {
        [Key]

        public int Department_Master_id { get; set; }
        public string Department_Master_name { get; set; }
        public string Department_Master_Desc { get; set; }
        public string Department_Master_Status { get; set; }

        public string Department_Master_CreatedDate{ get; set; }
    }
}
