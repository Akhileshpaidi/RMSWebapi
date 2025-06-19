using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    public class BeginAssModel
    {
        [Key]
        public int question_id { get; set; }
        public int response_type { get; set; }
        public string question { get; set; }
        public string base64 { get; set; }
        public List<BeginAssQstnsoptions> BeginAssQstnsoptions { get; set; }

    }
    public class BeginAssQstnsoptions
    {
        public int questionbank_optionID { get; set; }
        public string options { get; set; }

    }
    public class TotalQuestions
    {
        public int total { get; set; }
        public int question_id { get; set; }
    }

    [Table("user_ass_ans_details")]
    public class user_ass_ans_details
    {
        [Key]
        public int UserAss_Ans_DetailsID { get; set; }
        public int AssessementTemplateID { get; set; }
        public int UserID { get; set; }
        public int question_id { get; set; }
        public int? user_Selected_Ans { get; set; }
        public int correct_answer { get; set; }
        public string TypeofQuestion { get; set; }
        public int FlagQuestion { get; set; }
        public string Status { get; set; }
        public string TextFieldAnswer { get; set; }
        public int finalsubmit { get; set; }
        public DateTime CreatedDate { get; set; }


    }

    public class attemptedqstns
    {
        [Key]
        public int? user_Selected_Ans { get; set; }
        public int question_id { get; set; }
        public string TextFieldAnswer { get; set; }
    }
    public class UserAnswer
    {
        [Key]
        public int UserAss_Ans_DetailsID { get; set; }
        public int AssessementTemplateID { get; set; }
        public int UserID { get; set; }
        public int question_id { get; set; }
        public int? user_Selected_Ans { get; set; }
        public int correct_answer { get; set; }
        public string TypeofQuestion { get; set; }
        public int FlagQuestion { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string question { get; set; }
        public List<BeginAssQstnsoptions> BeginAssQstnsoptions { get; set; }
    }

    
    public class AssScheduleModel
    {
        public int assessment_builder_id { get; set; }
        public int ass_template_id { get; set; }
        public int Schedule_Assessment_id { get; set; }
        public string Type_Name { get; set; }
        public string assessment_name { get; set; }
        public string assessment_description { get; set; }
        public string SubType_Name { get; set; }
        public string Competency_Name { get; set; }
        public string AssessementDueDate { get; set; }
        public string AssessementcompletedDate { get; set; }
        public DateTime? created_date { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public string keywords { get; set; }
        public string status { get; set; }

    }
}
