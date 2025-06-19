using DocumentFormat.OpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("mailnotification")]
    public class NotificationCenterModel
    {
        [Key]
        public int MailNotificationID { get; set; }
        public int SenderID { get; set; }
        public int ReceiverID { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string SenderStatus { get; set; }
        public string RecevierStatus { get; set; }
        public string ThanksBody { get; set; }
        public string created_at{get;set;}
        public string updated_at { get;set;}

        public string senderName { get; set; }
        public string receiverName { get; set; }
        public string senderMail { get; set; }
        public string receiverMail { get; set; }
        public int favourite { get; set; }


    }
}
