using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("UserOtps")]
    public class UserOtpModel
    {
        public int id { get; set; }
        public int userId { get; set; }
        public string otp { get; set; }
        public DateTime expiryTime { get; set; }
    }
}
