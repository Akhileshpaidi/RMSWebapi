
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("riskactivityfrequency")]
    public class risksuperadminactivityfrequencymodel
    {

        [Key]
        public int activityid { get; set; }

        public string activityname { get; set; }

        public string activitydesc { get; set; }

        public int activityvalue { get; set; }

        public string createddate { get; set; }

        public string status { get; set; }

   
    }
}
