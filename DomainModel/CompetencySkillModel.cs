using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("competency_skill")]
    public class CompetencySkillModel
    {
        [Key]

        public int Competency_id { get; set; }
        public string Competency_Name { get; set; }
        public string Competency_Desc { get; set; }
        public string Competency_Weightage { get; set; }
        public string Competency_status { get; set; }
        public string competency_createdDate { get; set; }
        public string Position { get; set; }
    }
}
