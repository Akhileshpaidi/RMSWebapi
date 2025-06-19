using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace DomainModel
{
    [Table("holidaymaster")]
    public  class Holidaymaster
    {

        [Key]
        public int holidayid { get; set; }


        public string holidayname { get; set; }

      
       public string recurence {  get; set; }
        public string holidaytimeperiod {  get; set; } 
         public string hoildaytimeinterval {  get; set; } 
         public string holidaydescription {  get; set; }
           public int createdby {  get; set; } 
          public string createddate {  get; set; } 
           public string status {  get; set; }
        public string source { get; set; }


    }
}
