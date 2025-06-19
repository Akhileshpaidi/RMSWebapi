using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainModel
{
    [Table("userlogin")]
    public class UserLogin
    {
        [Key]
        public int LoginID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public int RoleId { get; set; }
        public string ROLE_NAME { get; set; }
    }
}
