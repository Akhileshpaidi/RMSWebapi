using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("jurisdiction_location_list")]
    public class SupAdmin_JurisdictionLocationModel
    {
        [Key]
        public int jurisdiction_location_id { get; set; }
        public int jurisdiction_country_id { get; set; }
        public int jurisdiction_state_id { get; set; }
        public string jurisdiction_district { get; set; }
        public string jurisdiction_location_create_date { get; set; }
        public string status { get; set; }
        

        public int createdby { get; set; }
    }
}
