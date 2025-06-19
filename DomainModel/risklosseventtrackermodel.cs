using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("risklosseventtracker")]
    public  class risklosseventtrackermodel
    {
        [Key]
        public int losseventtrackerid { get; set; }


        public string losseventname { get; set; }

        public string losseventdescription { get; set; }


        public int departmentid { get; set; }

        public int entityid { get; set; }

        public int unitlocationid { get; set; }

        public int riskBusinessfunctionid { get; set; }

        public long startValue { get; set; }

        public long endValues { get; set; }

        public string reportingusers { get; set; }

        public string additionalusers { get; set; }
        public int createdBy { get; set; }

        public string status { get; set; }

        public string createddate { get; set; }
    }
    public class Updatelossevent
    {
        public risklosseventtrackermodel risklosseventtrackermodels { get; set; }
        // Other properties may be defined here if needed
    }
}
