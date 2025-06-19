using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace DomainModel
{
    [Table("sub_type")]
    public class SubTypeModel
    {
        [Key]

        public int SubType_id { get; set; }
        public string SubType_Name { get; set; }
        public string SubType_DESC { get; set; }
        public string SubType_Status { get; set; }

        public String SubType_CreatedDate { get; set; }

        public int Type_id { get; set; }
    }
}
