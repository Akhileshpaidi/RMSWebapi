using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("authoritytype_master")]
    public class AuthorityTypeMaster
    {
        [Key]
        public int AuthorityTypeID { get; set; }
        public string AuthorityTypeName { get; set; }
        public string AuthorityTypeDescription { get; set; }
        public string Authoritytype_status { get; set; }
        public string Authoritytype_createdDate { get; set; }
    
}
}
