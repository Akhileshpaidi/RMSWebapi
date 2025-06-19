using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DomainModel
{
    [Table("userregistration")]
    public class UserRegistration
    {
        [Key]
        public int UserRegID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Pincode { get; set; }
        public string AadharNo { get; set; }
        public string PhoneNo { get; set; }
        //public LoginModel Login { get; set; }

    }
}
