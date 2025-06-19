using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1.Cms;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("map_user_role")]
    public class  mapuserrolemodel
    {
        [Key]
        public int mapuserid {  get; set; }

        public int USR_ID { get; set;}

        public int ROLE_ID { get; set; }

       public string mapuserrolestatus { get; set;}

        public int user_location_mapping_id {  get; set; }


        public int taskID { get; set; }
    }
}
