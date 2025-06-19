using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("assessment_generation_details")]
    public class AssessGenDetailsModel
    {
        [Key]
        public int Assessment_generation_details_ID { get; set; }
        public int Assessment_generationID { get; set; }
        public int question_id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
    }

}
