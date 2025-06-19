using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("frequencymaster")]
    public class SupAdmin_FrequencyModel
    {
        [Key]

        public int frequencyid { get; set; }


        public string recurenceid { get; set; }
        public string frequencyperiod { get; set; }
        public string frequencyDescription { get; set; }

        public string timeperiod { get; set; }

        public string timeinterval { get; set; }
        public string createddate { get; set; }

        public string status { get; set; }

        public int createdby { get; set; }
        public string frequencymastertablename { get; set; }
        public int nooffrequencyintervals { get; set; }

    }
}
