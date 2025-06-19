using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace DomainModel
{
    [Table("doc_perm_rights")]
   public class UserRightsPermissionModel
    {
        [Key]
        public int Doc_perm_rights_id { get; set; }
        public string Publish_type { get; set; }
        public string publish_Name { get; set; }
        public string Doc_perm_rights_status { get; set; }
        public string Doc_perm_rights_createdDate { get; set; }
    }
}
