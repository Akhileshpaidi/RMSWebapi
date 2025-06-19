using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("riskAssessmenttypemaster")]
    public class assessmentAttributesmodel
    {
        [Key]
        public int typeID { get; set; }

        public string typeName { get; set; }

        public string typeDesc { get; set; }

        public string status { get; set; }

        public int createdBy { get; set; }

        public string source { get; set; }

        public string createddate { get; set; }

    }
}
