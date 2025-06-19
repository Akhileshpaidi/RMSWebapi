using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;




namespace DomainModel
{

    [Table("type")]
    public class TypeModel
    {
        [Key]

        public int Type_id { get; set; }
        public string Type_Name { get; set; }
        public string Type_DESC { get; set; }
        public string Type_Status { get; set; }

        public String Type_CreatedDate { get; set; }


    }
}
