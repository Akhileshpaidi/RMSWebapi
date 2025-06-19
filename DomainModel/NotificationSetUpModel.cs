using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("notificationsetup")]
    public class NotificationSetUpModel
    {
        [Key]
        public int NotificationSetUpID { get; set; }
        public int EscalationStatus { get; set; }
        public int EnterDays { get; set; }
        public string DefaultNotifiers { get; set; }
        public string AdditionalNotifiers { get; set; }
        public string EnterComb { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedDate { get; set; }
       public int  RiskRegisterMasterID { get; set; }
        public int AddDoc_id { get; set; }
        public string Document_Id { get; set; }
        public int USR_ID { get; set; }
        public DateTime? review_start_Date { get; set; }


    }
}
