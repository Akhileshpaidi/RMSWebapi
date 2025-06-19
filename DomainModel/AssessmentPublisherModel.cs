using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("assessment_builder")]
    public class AssessmentPublisherModel
    {
        [Key]
        public int assessment_builder_id { get; set; }
        public int Assessment_generationID { get; set; }
        public int Competency_id { get; set; }
        public int show_explaination { get; set; }
        public int show_hint { get; set; }
        public int Type_id { get; set; }
        public int SubType_id { get; set; }
        public string assessment_name { get; set; }
        public string assessment_description { get; set; }
        public string keywords { get; set; }
        public string status { get; set; }
        public DateTime created_date { get; set; }
        public string ass_template_id { get; set; }
        public int total_questions { get; set; }
        public int total_estimated_time { get; set; }
        public int verson_no {  get; set; }

        public int user_id { get; set; }


    }


    public class AssessmentPublisherModelNew
    {
        [Key]
        public int assessment_builder_id { get; set; }
        public int Assessment_generationID { get; set; }
        public int Competency_id { get; set; }
        public int show_explaination { get; set; }
        public int show_hint { get; set; }
        public int Type_id { get; set; }
        public int SubType_id { get; set; }
        public string assessment_name { get; set; }
        public string assessment_description { get; set; }
        public string keywords { get; set; }
        public string status { get; set; }
        public DateTime created_date { get; set; }
        public string ass_template_id { get; set; }
        public int total_questions { get; set; }
        public int total_estimated_time { get; set; }


        public int user_id { get; set; }
        public List<int> OldQuestionsIds { get; set; } 
        


    }

    [Table("schedule_assessment")]
    public class RepeatFrequencyModel
    {
        [Key]

        public int Schedule_Assessment_id { get; set; }
        public string Date_Of_Request { get; set; }
        public string? value_Frequency { get; set; } // Make it nullable
        public string? frequency_period { get; set; }
        public DateTime? Assessment_start_Date { get; set; } // Make it nullable
        public string Duration_of_Assessment { get; set; }
        public string repeatEndDate { get; set; } // Make it nullable
        public string userid { get; set; }
        public int DocTypeID { get; set; }
        public int Doc_CategoryID { get; set; }
        public int Doc_SubCategoryID { get; set; }
        public int Entity_Master_id { get; set; }
        public int Unit_location_Master_id { get; set; }

        public DateTime created_date { get; set; }
        public string ass_template_id { get; set; }
        public int Shuffle_Questions { get; set; }
        public int Shuffle_Answers { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string objective { get; set; }
        public string message { get; set; }
        public string status { get; set; }
        public string firstname { get; set; }
        public string AssessmentStatus { get; set; }
        public int[] mapped_user { get; set; }

        public int tpauserid {  get; set; }

        public string Exemption_user_reason { get; set; }
        public int[] Exemption_user { get; set; }
        //public int[] map_user_id { get; set; }
       // public int[] Department_Master_id { get; set; }
        public string pagetype { get; set; }
      //  public int Req_per_userid {  get; set; }
        public int login_userid {  get; set; }
        public string uq_ass_schid {  get; set; }
        public string requester_name { get; set; }
        public string DocTypeName { get; set; }
        public string Doc_CategoryName { get; set; }
        public string Doc_SubCategoryName { get; set; }
        public string Entity_Master_Name { get; set; }
        public string Unit_location_Master_name { get; set; }
        public string Department_Master_name { get; set; }
        public string assessment_name {  get; set; }



    }



    [Table("scheduled_mapped_user")]
    public class scheduledMappedUserModel
    {
        [Key]
        public int Scheduled_mapped_user_id { get; set; }
        public int department_Master_id { get; set; }
        public int[] map_user_id { get; set; }
        public int Schedule_Assessment_id { get; set; }
        public DateTime? created_date { get; set; }
        public int docTypeID { get; set; }
        public int doc_CategoryID { get; set; }
        public int doc_SubCategoryID { get; set; }
        public string usr_ID { get; set; }
        public string status { get; set; }
        //public string firstname { get; set; }
        public string defaultkey { get; set; }
        public int requstingperson { get; set; }
        public string scheduledstatus { get; set; }
        public int riskAssesserid { get; set; }
        public string requester_name { get; set; }
        public string assessor_name { get; set; }
        public int tpaenityid { get; set; }
        public int tpauserid { get; set; }
        public int entity_Master_id { get; set; }
        public int unit_location_Master_id { get; set; }
        public int excludedusrid { get; set; }
        public string exculededdescription {  get; set; }
        public string tpaenitydescription { get; set; }
        public string DocTypeName { get; set; }
        public string Doc_CategoryName { get; set; }
        public string Doc_SubCategoryName { get; set; }
        public string Entity_Master_Name { get; set; }
        public string Unit_location_Master_name { get; set; }
        public string Department_Master_name { get; set; }


        public string Date_Of_Request { get; set; }





    }

    [Table("scheduled_excluded_user")]
    public class scheduledExcludedUserModel
    {
        [Key]
        public int Scheduled_Excluded_user_id { get; set; }
        public int[] Exemption_user { get; set; }
        public int[] map_user_id { get; set; }
        public int Schedule_Assessment_id { get; set; }
        public DateTime created_date { get; set; }
    }


    [Table("tbluser")]
    public class tableuser
    {
        [Key]
        public int USR_ID { get; set; }
        public string firstname { get; set; }
        public string emailid { get; set; }
        public int Department_Master_id { get; set; }
    }



    public class getschedulesass
    {
        [Key]
        public int Schedule_Assessment_id { get; set; }
        public int ass_template_id { get; set; }
        public int USR_ID { get; set; }
        public string assessment_name { get; set; }
        public string firstname { get; set; }
        public string Duration_of_Assessment { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string bgcolor { get; set; }

    }



    public class getschedulesassNew
    {
        [Key]
        public int Schedule_Assessment_id { get; set; }
        public string ass_template_id { get; set; }
        public int USR_ID { get; set; }
        public string assessment_name { get; set; }
        public string firstname { get; set; }
        public string Duration_of_Assessment { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string bgcolor { get; set; }

    }

    public class ViewAssDetails
    {
        [Key]
        public int Schedule_Assessment_id { get; set; }
        public string uq_ass_schid { get; set; }
        public string ass_template_id { get; set; }
        public int USR_ID { get; set; }
        public string assessment_name { get; set; }
        public string firstname { get; set; }
        public string Duration_of_Assessment { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string Entity_Master_Name { get; set; }
        public string Unit_location_Master_name { get; set; }
        public string Type_Name { get; set; }
        public string SubType_Name { get; set; }
        public string Competency_Name { get; set; }
        public string AssessmentStatus { get; set; }
        public string value_Frequency { get; set; }
        public string frequency_period { get; set; }
        public string Date_Of_Request { get; set; }
        public string mapped_users { get; set; }



    }





    //insert model for internal ass 

    public class RepeatFrequencyModelNew
    {
        [Key]

        public int Schedule_Assessment_id { get; set; }
        public string? Date_Of_Request { get; set; }
        public string? value_Frequency { get; set; } // Make it nullable
        public string? frequency_period { get; set; }
        public DateTime? Assessment_start_Date { get; set; } // Make it nullable
        public string? Duration_of_Assessment { get; set; }
        public string? repeatEndDate { get; set; } // Make it nullable
        public string? userid { get; set; }
        public int? DocTypeID { get; set; }
        public int[] Doc_CategoryID { get; set; }
        public int[] Doc_SubCategoryID { get; set; }
        
        public int? Entity_Master_id { get; set; }
        public int? Unit_location_Master_id { get; set; }

        public DateTime? created_date { get; set; }
        public string? ass_template_id { get; set; }
        public int? Shuffle_Questions { get; set; }
        public int? Shuffle_Answers { get; set; }
        public string? startDate { get; set; }
        public string? endDate { get; set; }
        public string? objective { get; set; }
        public string? message { get; set; }
        public string? status { get; set; }
        public string? firstname { get; set; }
        public string? AssessmentStatus { get; set; }
        public int[] mapped_user { get; set; }

        public int? tpauserid { get; set; }

        public string? Exemption_user_reason { get; set; }
        public int[] Exemption_user { get; set; }
        //public int[] map_user_id { get; set; }
         public List<int> Department_Master_id { get; set; }
        public string? pagetype { get; set; }
        //  public int Req_per_userid {  get; set; }
        public int login_userid { get; set; }
        public string? uq_ass_schid { get; set; }
        public string? requester_name { get; set; }
        public string? DocTypeName { get; set; }
        public string? Doc_CategoryName { get; set; }
        public string? Doc_SubCategoryName { get; set; }
        public string? Entity_Master_Name { get; set; }
        public string? Unit_location_Master_name { get; set; }
        public string? Department_Master_name { get; set; }
        public string? assessment_name { get; set; }
        public string? verson_no { get; set; }
        public string? defaultkey { get; set; }


    }
}
