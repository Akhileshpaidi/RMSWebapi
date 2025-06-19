using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("tbllog")]
    public class LogModel
    {
        [Key]
        public int LOG_ID { get; set; }
        public string USER_ID { get; set; }
        public string LOG_INFO { get; set; }
        public DateTime UPDATED_TIME { get; set; }
        public DateTime LogOut_Time { get; set; }





    }
}
