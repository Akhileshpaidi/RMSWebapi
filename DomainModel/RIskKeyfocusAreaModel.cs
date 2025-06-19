using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("riskkeyfocusarea")]
    public class RIskKeyfocusAreaModel
    {

        [Key]
        public int keyfocusareaid { get; set; }


        public string keyfousname { get; set; }

        public string keyfousdescription {  get; set; }


        public int departmentid { get; set; }

        public int entityid { get; set; }

        public int unitlocationid { get; set; }

        public int riskBusinessfunctionid { get; set; }


        public string businessprocessID { get; set; }

        public int bpmaturity { get; set; }

        public int createdby { get; set; }

        public string status { get; set; }
        
        public string createddate { get; set; }
    }


    public class Updatekeyfocusarea
    {
        public RIskKeyfocusAreaModel RIskKeyfocusAreaModels { get; set; }
        // Other properties may be defined here if needed
    }
}
