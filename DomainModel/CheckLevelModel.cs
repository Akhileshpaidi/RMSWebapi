using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainModel
{
    [Table("competency_check_level")]
    public class CheckLevelModel
    {
        [Key]
        public int check_level_id { get; set; }
        public string Position { get; set; }
        public string Skill_Level_Name { get; set; }
        public string Check_Level_Weightage { get; set; }
        public string Check_Level_Status { get; set; }
        public string Check_level_Description { get; set; }
        public string Created_Date { get; set;}

    }
}
