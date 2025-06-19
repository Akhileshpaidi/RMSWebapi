using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("jurisdiction_category_list")]
    public class SupAdmin_JurisdictionListModel
    {
        [Key]
        public int jurisdiction_category_id { get; set; }
        public string jurisdiction_categoryname { get; set; }
        public string jurisdiction_category_description { get; set; }
        public string jurisdiction_category_create_date { get; set; }
        public int createdby { get; set; }
        public string status { get; set; }
    }
}
