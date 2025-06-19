using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("penalty_category")]
    public class SupAdmin_PenaltyCategoryModel
    {
        [Key]
        public int penalty_category_id { get; set; }
        public string penalty_category_name { get; set; }
        public string penalty_category_description { get; set; }
        public string penalty_category_status { get; set; }
        public string penalty_category_date { get; set; }

        public int createdby { get; set; }
    }
}
