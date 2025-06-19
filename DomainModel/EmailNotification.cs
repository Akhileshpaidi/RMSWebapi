using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("mailnotification")]
    public class EmailNotification
    {
        [Key]
        public int MailNotificationID { get; set; }
        public int SenderID { get; set; }
        public int RecevierID { get; set; }
        public string Body { get; set; }
        public string SenderStatus { get; set; }
        public string RecevierStatus { get; set; }
        public string ThanksBody { get; set; }
    }
}
