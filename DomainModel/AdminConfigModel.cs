using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("admin_config")]
    public class AdminConfigModel
    {

        [Key]

        public int admin_config_id { get; set; }
        public string Direct_upload { get; set; }
        public string noOfDocuploaded { get; set; }
        public string sizelimit { get; set; }

        public string admin_config_status { get; set; }
        
    }
}
