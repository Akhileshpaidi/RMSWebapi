using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DomainModel
{
    [Table("riskcontrolmonitoring")]
    public class risksuperadmincontrolmonitoringmodel
    {
        [Key]
        public int controlmonitoringid { get; set; }

        public string controlmonitoringname { get; set; }

        public string controlmonitoringDesc { get; set; }

        public string createddate { get; set; }

        public string status { get; set; }
    }
}
