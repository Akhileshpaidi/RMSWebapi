using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace DomainModel
{
    [Table("notificationmailalert")]
    public  class Notificationreminder
    {
        [Key]
        public int NotificationMailAlertid { get; set; }
        public string NameofAlert { get; set; }
    }
}
