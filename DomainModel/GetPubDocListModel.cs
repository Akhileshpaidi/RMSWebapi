using System;
using Org.BouncyCastle.Asn1.Cms;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    public class GetPubDocListModel
    {


        [Key]

        public int AddDoc_id { get; set; }
        public string Document_Id { get; set; }
        public string document_name { get; set; }
        public string Title_Doc { get; set; }
        public string Sub_title_doc { get; set; }
        public string Obj_Doc { get; set; }
        public int NatureOf_Doc_id { get; set; }
        public string OtpMethod { get; set; }
        public string Publisher_name { get; set; }
        public string NatureOf_Doc_Name { get; set; }
        public string doc_last_Edited_On { get; set; }
        public string lastEditedby { get; set; }
        public string latestVersion { get; set; }
        public int DocTypeID { get; set; }

        public string DocTypeName { get; set; }

        public int Doc_CategoryID { get; set; }
        public string Doc_CategoryName { get; set; }

        public int Doc_SubCategoryID { get; set; }

        public string Doc_SubCategoryName { get; set; }

        public int AuthorityTypeID { get; set; }
        public string AuthorityTypeName { get; set; }
        public string addDoc_createdDate { get; set; }
        public string FilePath { get; set; }

        public int AuthoritynameID { get; set; }
        public string AuthorityName { get; set; }
        public string Provide_Access_status { get; set; }
        public string Doc_Confidentiality { get; set; }
        public string Initial_creation_doc_date { get; set; }
        public string Eff_Date { get; set; }
        public string Doc_internal_num { get; set; }

        public string Doc_Inter_ver_num { get; set; }
        public string Doc_Phy_Valut_Loc { get; set; }
        public string Doc_process_Owner { get; set; }
        public string Doc_Approver { get; set; }
        public string Date_Doc_Revision { get; set; }
        public string Date_Doc_Approver { get; set; }

        public string freq_period { get; set; }
        public string review_start_Date { get; set; }
        public string freq_period_type { get; set; }
        public string Keywords_tags { get; set; }
        public string pub_doc { get; set; }
        public string publisher_comments { get; set; }
        public string indicative_reading_time { get; set; }
        public string Time_period { get; set; }
        public string firstname { get; set; }
        public string CREATED_DATE { get; set; }
        public string Publisher_Date_Range { get; set; }
        public string Select_Opt { get; set; }
        public string Unit_location_Master_id { get; set; }
        public string Entity_Master_id { get; set; }

        public List<UpdateDoc_referenceNo> Doc_referenceNo { get; set; }
        public List<UpdateRevision_summary> Revision_summary { get; set; }

        public string VersionControlNo { get; set; }
        public int Review_Frequency_Status { get; set; }
        public int Doc_Linking_Status { get; set; }

        public string Review_Status { get; set; }
        public int NoofDays { get; set; }
        public string validations { get; set; }
        public string todaysdate { get; set; }
        public string datesub { get; set; }
        public DateTime startdate { get; set; }
        public DateTime enddate { get; set; }
        public int USR_ID { get; set; }
        public string status_permission { get; set; }
        public Boolean favorite { get; set; }
        public string Linking_Doc_names { get; set; }


        public int? MainpageCount { get; set; }

        public int? supportFilesCount { get; set; }
        public string addDoc_Status { get; set; }
        public string DisableReason { get; set; }
        public DateTime? duedate { get; set; }
        public int? DaysLeft { get; set; }


        public string companyname { get; set; }
        public string locationname { get; set; }
        public DateTime? ackusercreateddate { get; set; }
        public string ack_status { get; set; }
        public string emailid { get; set; }
        public string nameofuser { get; set; }
        public int Doc_User_Access_mapping_id { get; set; }
        public DateTime? readcompleted { get; set; }
        public DateTime? acknowledgementdate { get; set; }
        public TimeSpan? TimeTakenToComplete { get; set; }
        public TimeSpan? TimeToAcknowledge { get; set; }

        public int noOfMappedUsers { get; set; }
        public DateTime? mappingDate { get; set; }
        public string permissions_names { get; set; }

        public string provideack_status { get; set; }

        public string combinedReadingTime {  get; set; }

    }



    public class GettingAssessmentTemplateModel
    {
        [Key]
        public int? question_id { get; set; }
        public string? question { get; set; }
        public int? response_type { get; set; }
        public int? no_of_selectionchoices { get; set; }
        public int? correct_answer { get; set; }
        public string? question_hint { get; set; }
        public string? questionmarked_favourite { get; set; }
        public int? score_weightage { get; set; }
        public string? check_level { get; set; }
        public float? checklevel_weightage { get; set; }
        public int? estimated_time { get; set; }

        public string? assessor_randomselection { get; set; }
        public string? assessment_randomsetting { get; set; }
        public int? subjectid { get; set; }
        public int? topicid { get; set; }
        public string? ref_to_governance_control { get; set; }
        public string? question_disabled { get; set; }
        public string? objective { get; set; }
        public string? base64 { get; set; }

        public string? fullresponse { get; set; }
        public int? userid { get; set; }

        public int? Assessment_generationID { get; set; }
        public List<AssessmentBuilder> options { get; set; }

        public DateTime? created_date { get; set; }

        public DateTime? disabled_date { get; set; }
        public string? status { get; set; }
        public string? DocTypeName { get; set; }
        public string? Doc_CategoryName { get; set; }

        public string? Doc_SubCategoryName { get; set; }


        //public int Assessment_generation_details_ID { get; set; }
        public int? assessment_builder_id { get; set; }
        public string? assessment_name { get; set; }
        public string? assessment_description { get; set; }
        public int? Type_id { get; set; }
        public int? SubType_id { get; set; }
        public string? show_hint { get; set; }

        public int? Competency_id { get; set; }
        public string? SubType_Name { get; set; }
        public string? Type_Name { get; set; }
        public int? no_of_questions_mapped { get; set; }
        public string ass_template_id { get; set; }

        public string? Subject_Name { get; set; }

        public string? Topic_Name { get; set; }
        public string? Competency_Name { get; set; }
        public string? keywords { get; set; }

        public string? show_explaination { get; set; }
        public string? firstname { get; set; }
        public int? updated_user_id { get; set; }
        public DateTime? updated_date { get; set; }
        public string? UpdateUsername { get; set; }
        public string? total_questions { get; set; }
        public string? total_estimated_time { get; set; }
        public string? reason_for_disable { get; set; }
        public string? UnitLocationMasterID { get; set; }
        public string? EntityMasterID { get; set; }
        public string? CompanyLocation { get; set; }
        public string? CompanyName { get; set; }
        public string? usersCount { get; set; }
        public string? Users { get; set; }
        public string? userName { get; set; }
        public string? email { get; set; }
        public DateTime? MappedDate { get; set; }

        //scheduled table

        public int? Schedule_Assessment_id { get; set; }
        public DateTime? Date_Of_Request { get; set; }
        public int? value_Frequency { get; set; }
        public string? frequency_period { get; set; }
        public string? Duration_of_Assessment { get; set; }
        public DateTime? repeatEndDate { get; set; }
        public int? Unit_location_Master_id { get; set; }
        public int? Entity_Master_id { get; set; }
        public int? Department_Master_id { get; set; }
        public string? document_type { get; set; }
        public string? document_category { get; set; }
        public string? document_subCategory { get; set; }
        public DateTime? scheduled_created_date { get; set; }
        public string? scheduled_status { get; set; }
        public string? AssessorName { get; set; }
        public int? Shuffle_Questions { get; set; }
        public int? Shuffle_Answers { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public string? scheduled_objective { get; set; }
        public string? message { get; set; }
        public string? mapped_user_name { get; set; }
        public int? total_mapped_users { get; set; }
        public string? tpauser_name { get; set; }
        public string? AssessmentStatus { get; set; }
        public int? login_userid { get; set; }
        public string? uq_ass_schid { get; set; }
        public string? pagetype { get; set; }
        public string? unitlocationname { get; set; }
        public string? entityname { get; set; }
        public string? emailid { get; set; }

        public string? depatment { get; set; }

        public string? exemptionreason { get; set; }
        public DateTime? acknowledgemet_date { get; set; }
        public string? remaining_time_left { get; set; }
        public string AcknowledgementStatus { get; set; }

        public int? verson_no { get; set; }
        public int usage_count { get; set; }

        public string requestingpersonname { get; set; }
        public string user_names { get; set; }

        public string disableby { get; set; }

        public DateTime? CreatedDate { get; set; }


        public string? AssUserPermissionName { get; set; }


    }

    //List of Ass With Result

    public class CompletedAssScheduleModel
    {
        public int assessment_builder_id { get; set; }
        public int CorrectAnswers { get; set; }

        public string ass_template_id { get; set; }
        public int verson_no { get; set; }
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
        public string? document_type { get; set; }
        public string? document_category { get; set; }
        public string? document_subCategory { get; set; }
        public int? total_mapped_users { get; set; }
        public int? number_of_ques { get; set; }
        public string? assessor_name { get; set; }
        public string? assessment_type { get; set; }
        public string? frequencyperiod { get; set; }
        public string? total_estimated_time { get; set; }
        public int userid { get; set; }
        public int? No_of_Questions { get; set; }
        public int? No_of_answered_Questions { get; set; }
        public double? scoreindictor { get; set; }
        public string ScoreName { get; set; }
        public string type { get; set; }

        public string requestingname { get; set; }

        public string scoreIndicator { get; set; }

        public string Key_Impr_Indicator_Name {  get; set; }
        public string AssessmentStatus {  get; set; }

        public int? total_scheduled_users {  get; set; }
        public int? total_completed_users {  get; set; }

        public DateTime? EndDateTime {  get; set; }

        public double AccuracyPercentage {  get; set; }
    }


    //list of result by check level 

    public class Getcountchecklevel
    {
        public int USR_ID { get; set; }
        public string firstname { get; set; }
        public string Subject_Name { get; set; }
        public string Topic_Name { get; set; }
        public int No_of_Questions { get; set; }
        public int no_of_answered_qstns { get; set; }
        public double ScoreIndicator { get; set; }
        public string ScoreIndicatorName { get; set; }
        // Add new fields here
        public DateTime Date_Of_Request { get; set; }
        public string frequency_period { get; set; }
        public string Duration_of_Assessment { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public string AssessmentStatus { get; set; }
        public string pagetype { get; set; }
        public DateTime? scheduled_date { get; set; }
        public string scheduled_status { get; set; }
        public string assessment_name { get; set; }
        public string total_questions { get; set; }
        public string total_estimated_time { get; set; }
        public string assessor_name { get; set; }
        public string Type_Name { get; set; }
        public string SubType_Name { get; set; }
        public string Competency_Name { get; set; }
        public string DocTypeName { get; set; }
        public string Doc_CategoryName { get; set; }
        public string Doc_SubCategoryName { get; set; }
    }










    //MitigationActioModel
    public class MitigationDataModel
    {

        public int suggestions_id { get; set; }

        public int mitigations_id { get; set; }
        public string suggestions { get; set; }
        public string status { get; set; }
        public DateTime? mitigationcreateddate { get; set; }
        public int suggested_by { get; set; }
        public string mitigation_requestedby { get; set; }
        public int acknowledged_by { get; set; }
        public int action_required { get; set; }
        public string notify_management { get; set; }
        public DateTime input_date { get; set; }
        public int assigned_responsibility { get; set; }
        public int tentative_timeline { get; set; }
        public string suggested_documents { get; set; }
        public int action_priority { get; set; }
        public int acknowledge { get; set; }
        public string remarks { get; set; }
        public string TrackerID { get; set; }
        public string PO_remarks { get; set; }
        public string file_path { get; set; }
        public string file_name { get; set; }
        public string management_remarks { get; set; }
        public string completed_date { get; set; }
        public string assessment_id { get; set; }
        public string assessment_name { get; set; }
        public DateTime? assessment_startDate { get; set; }
        public DateTime? assesssment_endDate { get; set; }
        public string person_requesting_assess { get; set; }
        public string doctype { get; set; }
        public string docCategory { get; set; }
        public string document_subCategor { get; set; }
        public string assess_type { get; set; }
        public string assess_subType { get; set; }
        public string name_of_assessor { get; set; }
        public string overal_score_award { get; set; }
        public string key_result { get; set; }
        public string num_of_attempts { get; set; }
        public string num_of_Tasks { get; set; }
        public string num_of_tasks_completed { get; set; }
        public string company_name { get; set; }
        public string locationName { get; set; }
        public string asigneeEmailId { get; set; }
        public DateTime AssCompletionDate { get; set; }

        public string ass_template_id { get; set; }

        public string uq_ass_schid {  get; set; }

    }



    public class QuestionModel_for_Report
    {
        public int? question_id { get; set; }
        public string? question { get; set;}
        public int? user { get; set; }
        public string? created_by { get; set; }
        public string? status { get; set; }
        public DateTime? created_date { get; set; }
        public string? competency_check_level { get; set; }
        public float? weightage { get; set; }
        public int? number_of_times_use { get; set; }
        public int? estimated_time { get; set; } 
        public string? subject { get; set; }
        public string? topic { get; set; }
        public string? company_name { get; set; }
        public string? location_name { get; set; }
        public string? department { get; set; }
        public int? number_of_questions_created { get; set; }
        public int? no_of_questions_used { get; set; }
        public int? no_of_active_questions { get; set; }
        public int? usability_factor { get; set; }
        public DateTime? que_last_created_on { get; set; }
        public string keywords { get; set; }
        public string objective_in_Assessment { get; set; }


        public string disable_date { get; set; }

        public string lastupdted_date {  get; set; }
    }


    public class DateModel
    {
        public string datetype { get; set; }
        public string today { get; set; }
        public string oneMonthAgo { get; set; }

        public int userid {  get; set; }
    }


}
