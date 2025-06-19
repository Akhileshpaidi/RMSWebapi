using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{

    public class AssessmentResultsModel
    {
        [Key]
        public int UserAss_Ans_DetailsID { get; set; }
        public string AssessementTemplateID { get; set; }
        public int UserID { get; set; }
        public int question_id { get; set; }
        public string correct_answer { get; set; }
        public string firstname { get; set; }
        public string TypeofQuestion { get; set; }
        public string question { get; set; }
        public string IsAnswerCorrect { get; set; }


        public string? FlagQuestion { get; set; }

        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }

        public string user_Selected_Ans { get; set; }
        public string ref_to_governance_control { get; set; }

        public string CheckLevelName { get; set; }
        public string TopicName {  get; set; }

        public  string SubjectName { get; set; }

    }
    public class AssessmentResults
    {

        [Key]
        public int question_id { get; set; }
        public string question { get; set; }

    }


}
