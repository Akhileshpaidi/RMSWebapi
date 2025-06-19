using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainModel
{
    [Table("regulatory_authority")]
    public class RegulatoryAuthorityModel
    {
        [Key]
        public int regulatory_authority_id { get; set; }

        public string regulatory_authority_name { get; set; }

        public string regulatory_authority_description { get; set; }

        public string regulatory_authority_status { get; set; }

        public string regulatory_authority_created_date { get; set; }
        public string source { get; set; }

        public int createdby { get; set; }
    }
}
