using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("alerts_reminders")]
    public class Alertsandremindersmodel
    {

        [Key]
        public int alerts_reminders_id { get; set; }
        public string workactivity { get; set; }

        public int expectedDays { get; set; }
        public string expectedperiodicity { get; set; }
        public bool expectedhoilday { get; set; }
        public int reminderalertdays { get; set; }
        public string reminderalertperidicty { get; set; }
        public int notification { get; set; }
       public int createdby { get; set; }
        public string createddate {  get; set; }

        public string status { get; set; }
    }

}
