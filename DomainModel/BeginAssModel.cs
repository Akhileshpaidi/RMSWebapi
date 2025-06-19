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
        public string show_hint { get; set; }
        public string question_hint { get; set; }
        public List<BeginAssQstnsoptions> BeginAssQstnsoptions { get; set; }

    }
    public class BeginAssQstnsoptions
    {
        public int questionbank_optionID { get; set; }
        public int OptionId { get; set; }
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
        public string? AssessementTemplateID { get; set; }
        public int UserID { get; set; }
        public int question_id { get; set; }
        public int? user_Selected_Ans { get; set; }
        public int correct_answer { get; set; }
        public string TypeofQuestion { get; set; }
        public string uq_ass_schid { get; set; }
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
        public string AssessementTemplateID { get; set; }
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

    public class Getcounts0fAssessment
    {
        public int No_of_Users_Assigned { get; set; }
        public int No_of_Users_completed { get; set; }
        public int No_of_Users_incomplete { get; set; }
        public int No_of_Users_ass_Expired { get; set; }
        public string Doc_CategoryName { get; set; }
        public string DocTypeName { get; set; }
        public string assessment_name { get; set; }
        public string Doc_SubCategoryName { get; set; }
        public DateTime? endDate { get; set; }

        public string uq_ass_schid { get; set; }

    }

    public class GetcountsofUserScore
    {
        public int UserID { get; set; }
        public string firstname { get; set; }
        public string ScoreName { get; set; }
        public string Key_Impr_Indicator_Name { get; set; }
        public int total_questions { get; set; }
        public double OverallScore { get; set; }
        public double TotalScoreofAssessement { get; set; }
        public int TotalQuestionsAnswered { get; set; }
        public double Percentage { get; set; }
        public string Days { get; set; }

        public string Remarks { get; set; }

        public int? CorrectAnswers { get; set; }
        public double AccuracyPercentage { get; set; }

        public string ass_template_id {  get; set; }

        public string uq_ass_schid { get; set; }

        public string Type_Name { get; set; }
   
        public string SubType_Name { get; set; }
        public string Competency_Name { get; set; }

    }
    public class AssScheduleModel
    {
        public int assessment_builder_id { get; set; }
        public string ass_template_id { get; set; }
        public int Schedule_Assessment_id { get; set; }
        public int Competency_id { get; set; }
        public string Type_Name { get; set; }
        public string assessment_name { get; set; }
        public string assessment_description { get; set; }
        public string SubType_Name { get; set; }
        public string Competency_Name { get; set; }
        public string AssessementDueDate { get; set; }
        public string AssessementcompletedDate { get; set; }
        public DateTime? created_date { get; set; }
        public DateTime? startDate { get; set; }
        public string startDates { get; set; }
        public DateTime? endDate { get; set; }
        public string endDates { get; set; }
        public string keywords { get; set; }
        public string status { get; set; }
        public string uq_ass_schid { get; set; }
        public string firstname { get; set; }
        public string Remarks { get; set; }
        public string created_dates { get; set; }
        public string pagetype { get; set; }
        public string verson_no { get; set; }
        public string objective { get; set; }
        public string message { get; set; }
        public string builder_version_firstname {  get; set; }
        public int total_assigned_users { get; set; }

        public int completed_users { get; set; }

        public string OverallAssessmentStatus {  get; set; }

    }


    public class AssScheduleModelNew
    {
        public int assessment_builder_id { get; set; }
        public string? ass_template_id { get; set; }
        public int Schedule_Assessment_id { get; set; }
        public int assessment_builder_versionsID { get; set; }
        public int Competency_id { get; set; }
        public string Type_Name { get; set; }
        public string assessment_name { get; set; }
        public string assessment_description { get; set; }
        public string SubType_Name { get; set; }
        public string Competency_Name { get; set; }
        public string AssessementDueDate { get; set; }
        public string AssessementcompletedDate { get; set; }
        public DateTime? created_date { get; set; }
        public DateTime? startDate { get; set; }
        public string startDates { get; set; }
        public DateTime? endDate { get; set; }
        public string endDates { get; set; }
        public string keywords { get; set; }
        public string status { get; set; }
        public string uq_ass_schid { get; set; }
        public string firstname { get; set; }
        public string Remarks { get; set; }
        public string created_dates { get; set; }
        public string pagetype { get; set; }
        public string verson_no { get; set; }
        public string objective { get; set; }
        public string message { get; set; }
        public string builder_version_firstname { get; set; }

    }


    public class GetComptencyskillmodel
    {
        public int userid { get; set; }
        public string firstname { get; set; }

        public int No_of_Easy_Questions { get; set; }
        public int No_of_Qstns { get; set; }
        public int No_of_Easy_answered_Questions { get; set; }
        public double Easyscoreindictor { get; set; }
        public int No_of_Medium_Questions { get; set; }
        public int No_of_medium_answered_Questions { get; set; }
        public double Mediumscoreindictor { get; set; }
        public int No_of_Hard_Questions { get; set; }
        public int No_of_hard_answered_Questions { get; set; }
        public double Hardscoreindictor { get; set; }
        public string HardScoreName { get; set; }
        public string EasyScoreName { get; set; }
        public string MediumScoreName { get; set; }
        public string type { get; set; }

    }

    public class GetComptencyskill
    {
        public int userid { get; set; }
        public string firstname { get; set; }

        public int No_of_Questions { get; set; }

        public int No_of_answered_Questions { get; set; }
        public double scoreindictor { get; set; }

        public string ScoreName { get; set; }

        public string type { get; set; }
        public string Skill_Level_Name { get; set; }
        public int CorrectAnswers { get; set; }
        public double AccuracyPercentage { get; set; }
        public int USR_ID { get; set; }
    
        public string Subject_Name { get; set; }
        public string Topic_Name { get; set; }

        public int no_of_answered_qstns { get; set; }
        public double ScoreIndicator { get; set; }

        public string ScoreIndicatorName { get; set; }
        public string Key_Impr_Indicator_Name { get; set; }
  

    }


    public class Getcountsubjecttopic
    {
        public int USR_ID { get; set; }
        public string firstname { get; set; }
        public string Subject_Name { get; set; }
        public string Topic_Name { get; set; }

        public int No_of_Questions { get; set; }

        public int no_of_answered_qstns { get; set; }
        public double ScoreIndicator { get; set; }

        public string ScoreIndicatorName { get; set; }
        public string Key_Impr_Indicator_Name { get; set; }
        public string Skill_Level_Name { get; set; }
        public int CorrectAnswers { get; set; }
        public double AccuracyPercentage { get; set; }




    }


    public class GetTimeTakenDetails
    {
        public int USR_ID { get; set; }
        public string firstname { get; set; }


        public int Days { get; set; }





    }
    public class GetScoreIndicatorModel
    {
        public int USR_ID { get; set; }
        public int Score_id { get; set; }
        public int no_of_users { get; set; }
        public int Key_Impr_Indicator_id { get; set; }
        public string ScoreIndicator { get; set; }
        public string Key_Impr_Indicator_Name { get; set; }








    }



    public class GetQuestionsUserModel
    {
        public int question_id { get; set; }
        public int No_of_Users_Answered_Correct { get; set; }
        public int No_of_Users_Answered_InCorrect { get; set; }
        public int No_of_Users_Answered_Not_Attempted { get; set; }
        public int No_of_Users_Answered_Attempted { get; set; }
        public string question { get; set; }
        public string correct_answer { get; set; }
        public string firstname { get; set; }


        public string uq_ass_schid { get; set; }
        public string CheckLevelName { get; set; }
        public string TopicName { get; set; }

        public string SubjectName { get; set; }





    }
}
