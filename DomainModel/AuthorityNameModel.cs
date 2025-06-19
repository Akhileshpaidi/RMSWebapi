using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("authorityname_master")]
    public class AuthorityNameModel
    {
        [Key]
       
        public int AuthoritynameID { get; set; }
        public string AuthorityName { get; set; }
        public string AuthorityNameDescription { get; set; }
        public int AuthorityTypeID { get; set; }

        public string Authority_Status { get; set; }
        public string Authority_CreatedDate { get; set; }


    }
}
