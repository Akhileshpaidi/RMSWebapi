using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DomainModel
{
    //[Table("userlogin")]
    //public class LoginModel
    //{
    //    [Key]
    //    public int LoginID { get; set; }
    //    public string UserName { get; set; }
    //    public string Password { get; set; }
    //    public string FirstName { get; set; }
    //    public int  RoleID { get; set; }
    //    public string ROLE_NAME { get; set; }

    //    //[ForeignKey("UserRegID")]
    //    //public int UserRegID { get; set; }
    //    //public List<UserRegistration> UserRegistration { get; set; }
    //}
    public class LoginViewModel
    {
        [Key]
        public int LoginID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public int RoleID { get; set; }

    }

   
}
