using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace DomainModel
{
    [Table("scheduled_excluded_user")]
    public class Sheduledexcluededusersmodel
    {
        [Key]
        public int Scheduled_Excluded_user_id { get; set; }

        public  int Exemption_user {  get; set; }

        public string excludeddescription {  get; set; }

        public int? Schedule_Assessment_id {  get; set; }

       public int ?Scheduled_mapped_user_id { get; set; }

        public string defaultkey {  get; set; }
        public string created_date {  get; set; }
    }
}
