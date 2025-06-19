using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace DomainModel
{
   
    public class GetUserModel
    {
        [Key]
        public int ROLE_ID { get; set; }
        public string ROLE_NAME { get; set; }
        public int USR_ID { get; set; }
        public string firstname { get; set; }
        
    }

    public class Notificationmailalert
    {
        [Key]
        public int NotificationMailAlertid { get; set; }
        public string NameofAlert { get; set; }
    }

    [Table("complaince_user_mapping")]
    public class complainceusermapping
    {
        [Key]
        public int Complaince_User_Mapping_id { get; set; }
        public bool Include_Holiday_Factor { get; set; }
        public bool Make_Attachement_Mandatory { get; set; }
        public bool Attachement_Format_Allowed { get; set; }
        public bool PDF { get; set; }
        public bool doc_docx { get; set; }
        public bool xls_xlsx { get; set; }
        public bool ppt_pptx { get; set; }
        public bool compressed_zip { get; set; }
        public bool Include_Audit_Workflow { get; set; }
        public bool Include_Authorization_Workflow_required { get; set; }
        public bool Include_Review_Activity_Workflow { get; set; }
        public bool Include_Approve_Activity_Workflow { get; set; }
        public bool Include_Review_Approve_by_Same_User { get; set; }
        public int No_of_Escalations { get; set; }
        public int No_of_Attachements { get; set; }
        public int No_of_Reminders { get; set; }
       
        public int OverdueReminderDays { get; set; }
        public string OverdueReminderPeriodicity { get; set; }
        public string OverdueReminderNotification { get; set; }
        public string company_compliance_ids { get; set; }
        public string Apply_Scheduler_On { get; set; }
        public DateTime Effective_StartDate { get; set; }
        public string StartDate { get; set; }
        public DateTime Effective_EndDate { get; set; }
        public string EndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
    }

    [Table("complaince_user_activity_mapping")]

    public class complaince_user_activity_mappingModel
    {
        [Key]
        public int Complaince_User_Activity_MappingID { get; set; }
        public int Complaince_User_Mapping_id { get; set; }
        public int UpdateActivity { get; set; }
        public int ReviewActivity { get; set; }
        public int ApproveActivity { get; set; }
        public string MonitorActivity { get; set; }
        public int AuditActivity { get; set; }
        public string ViewActivity { get; set; }
        public int RemediationActivity { get; set; }
        public int BackupUserActivity { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }


    }
    public class ComplainceUserMappingDto
    {
        public complainceusermapping? ComplainceUserMapping;
        public complaince_user_activity_mappingModel? ComplainceUserActivityMapping;
        public complaince_escalation_remindersModel? ComplainceEscalation;
    }

    [Table("complaince_escalation_reminders")]
    public class complaince_escalation_remindersModel
    {
        [Key]
        public int Complaince_Escalation_Reminders_id { get; set; }

        public int Complaince_User_Mapping_id { get; set; }
        public int reminder_index { get; set; }
        public int reminder_alert_days { get; set; }
        public string periodicity_of_overdue_reminder { get; set; }
        public bool level_check_overdue_reminder { get; set; }
        public int Notification_overdue_reminder { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
    }

    [Table("compliance_escalation_levels")]
    public class Level
    {
        [Key]
        public int compliance_escalation_levels_Id { get; set; }
        public int Complaince_User_Mapping_id { get; set; }

        public int levelindex { get; set; }
        public bool levelcheck { get; set; }
        public List<int> levelcheckusername { get; set; }
        public bool levelcheckOverduereminder { get; set; }
        public List<Reminder> reminders { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
    }
    [Table("compliance_escalation_level_reminders")]
    public class Reminder
    {
        [Key]
        public int compliance_escalation_level_reminders_Id { get; set; }
        public int Complaince_User_Mapping_id { get; set; }

        public int remindersindex { get; set; }
        public int reminderalertdays { get; set; }
        public string Periodicityofoverduereminder { get; set; }
        public bool levelcheckOverduereminder { get; set; }
        public int Notification_overduereminder { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
    }

    public class complaincelistmodel
    {
        public string Entity_Master_Name { get; set; }
        public string Unit_location_Master_name { get; set; }
        public int create_company_compliance_id { get; set; }
        public int entityid { get; set; }
        public int unitlocationid { get; set; }
        public string Department_Master_name { get; set; }
        public int departmentid { get; set; }
        public int No_of_Attachements { get; set; }
        public string compliance_name { get; set; }
        public Boolean mandatory {  get; set; }
        public string act_rule_name { get; set; }
        public string actregulatoryname { get; set; }
        public string section_rule_regulation_ref { get; set; }
        public string compliance_type_name { get; set; }
        public string law_Categoryname { get; set; }
        public string ComplaincePeriod { get; set; }
        public string complianceStage { get; set; }
        public string riskClassification { get; set; }
        public int location_department_mapping_id { get; set; }
        public string compliance_id { get; set; }


        public string days_status { get; set; }
        public string remedial_days { get; set; }

        public string compliance_stage_progress { get; set; }
        public DateTime due_date { get; set; }
        

        public DateTime Effective_StartDate { get; set; }
        public DateTime Effective_EndDate { get; set; }
        public DateTime? approved_remedial_comply_date { get; set; }
        public DateTime? proposed_remedial_comply_date { get; set; }
        public DateTime? rpa_request_date { get; set; }
        public string remediation_request_remarks { get; set; }
        public string compliance_status { get; set; }
        public int remediation_plan_id { get; set; }
        public string AuditWorkflow { get; set; }
        public string AuthWorkflow { get; set; }
        public string compliance_description { get; set; }
        public string Review_Workflow { get; set; }
        public string Approve_Workflow { get; set; }



    }


    public class Reviewcomplaincelistmodel
    {
        public string Entity_Master_Name { get; set; }
        public string Unit_location_Master_name { get; set; }
        public int? create_company_compliance_id { get; set; }
        public int? entityid { get; set; }
        public int? unitlocationid { get; set; }
        public string Department_Master_name { get; set; }
        public int? departmentid { get; set; }
        public int location_department_mapping_id { get; set; }
        public int? No_of_Attachements { get; set; }
        public string compliance_name { get; set; }
        public Boolean? mandatory { get; set; }
        public string act_rule_name { get; set; }
        public string actregulatoryname { get; set; }
        public string section_rule_regulation_ref { get; set; }
        public string compliance_type_name { get; set; }
        public string law_Categoryname { get; set; }
        public string ComplaincePeriod { get; set; }
        public string complianceStage { get; set; }
        public string riskClassification { get; set; }
        public string days_status { get; set; }
        public string compliance_stage_progress { get; set; }
        public string review_days { get; set; }
        public DateTime? Effective_StartDate { get; set; }
        public DateTime? due_date { get; set; }
        public string approved_remedial_comply_date { get; set; }
        public string proposed_remedial_comply_date { get; set; }
        public string rpa_request_date { get; set; }
        public string remediation_request_remarks { get; set; }
        public string compliance_status { get; set; }
        public string review_status { get; set; }

        public string AuditWorkflow { get; set; }
        public string AuthWorkflow { get; set; }
        public string compliance_description { get; set; }
        public string Review_Workflow { get; set; }
        public string Approve_Workflow { get; set; }
        public DateTime? ComplainceUpdatedDate { get; set; }
        public string applicability_status { get; set; }
        public DateTime? actual_complied_date { get; set; }
        public string amount_paid { get; set; }
        public string penalty_paid { get; set; }
        public string updation_remarks { get; set; }
        public int? update_compliance_id { get; set; }
        public string compliance_id { get; set; }
        public IList<updatefiledocuments> updatefiledocuments { get; set; }

    }
    public class Approvecomplaincelistmodel
    {
        public string compliance_id { get; set; }
        public string Entity_Master_Name { get; set; }
        public string Unit_location_Master_name { get; set; }
        public int? create_company_compliance_id { get; set; }
        public int? entityid { get; set; }
        public int? unitlocationid { get; set; }
        public string Department_Master_name { get; set; }
        public int? departmentid { get; set; }
        public int? No_of_Attachements { get; set; }
        public string compliance_name { get; set; }
        public string approve_status { get; set; }
        public Boolean? mandatory { get; set; }
        public string act_rule_name { get; set; }
        public string actregulatoryname { get; set; }
        public string section_rule_regulation_ref { get; set; }
        public string compliance_type_name { get; set; }
        public string law_Categoryname { get; set; }
        public string ComplaincePeriod { get; set; }
        public string complianceStage { get; set; }
        public string riskClassification { get; set; }
        public string days_status { get; set; }
        public string compliance_stage_progress { get; set; }
        public string review_activity_result { get; set; }
        public string reject_remarks { get; set; }
        public string updation_activity_result { get; set; }
        public string approver_activity_result { get; set; }
        public string audit_activity_result { get; set; }
        public DateTime? reviewer_due_date { get; set; }
        public DateTime? compliance_update_date { get; set; }
        public DateTime? approval_due_date { get; set; }
        public DateTime? audit_date { get; set; }
        public DateTime? approval_date { get; set; }
        public DateTime? reviewer_date { get; set; }
        public int location_department_mapping_id { get; set; }

        public DateTime? Effective_StartDate { get; set; }
        public DateTime? due_date { get; set; }
        public string approved_remedial_comply_date { get; set; }
        public string proposed_remedial_comply_date { get; set; }
        public string rpa_request_date { get; set; }
        public string remediation_request_remarks { get; set; }
        public string compliance_status { get; set; }

        public string AuditWorkflow { get; set; }
        public string AuthWorkflow { get; set; }
        public string compliance_description { get; set; }
        public string Review_Workflow { get; set; }
        public string Approve_Workflow { get; set; }
        public DateTime? ComplainceUpdatedDate { get; set; }
        public string applicability_status { get; set; }
        public DateTime? actual_complied_date { get; set; }
        public string amount_paid { get; set; }
        public string penalty_paid { get; set; }
        public string updation_remarks { get; set; }
        public int? update_compliance_id { get; set; }
        public IList<updatefiledocuments> updatefiledocuments { get; set; }

    }

    public class Auditcomplaincelistmodel
    {
        public string Entity_Master_Name { get; set; }
        public string Unit_location_Master_name { get; set; }
        public int? create_company_compliance_id { get; set; }
        public int? entityid { get; set; }
        public int? unitlocationid { get; set; }
        public string Department_Master_name { get; set; }
        public int? departmentid { get; set; }
        public int? No_of_Attachements { get; set; }
        public string compliance_name { get; set; }
        public string audit_status { get; set; }
        public Boolean? mandatory { get; set; }
        public string act_rule_name { get; set; }
        public string actregulatoryname { get; set; }
        public string section_rule_regulation_ref { get; set; }
        public string compliance_type_name { get; set; }
        public string law_Categoryname { get; set; }
        public string ComplaincePeriod { get; set; }
        public string complianceStage { get; set; }
        public string riskClassification { get; set; }
        public string days_status { get; set; }
        public string compliance_stage_progress { get; set; }
        public int location_department_mapping_id { get; set; }
        public string compliance_id { get; set; }

        public DateTime? Effective_StartDate { get; set; }
        public DateTime? due_date { get; set; }
        public string approved_remedial_comply_date { get; set; }
        public string proposed_remedial_comply_date { get; set; }
        public string rpa_request_date { get; set; }
        public string remediation_request_remarks { get; set; }
        public string compliance_status { get; set; }

        public string AuditWorkflow { get; set; }
        public string AuthWorkflow { get; set; }
        public string compliance_description { get; set; }
        public string Review_Workflow { get; set; }
        public string Approve_Workflow { get; set; }
        public DateTime? ComplainceUpdatedDate { get; set; }
        public string applicability_status { get; set; }
        public DateTime? actual_complied_date { get; set; }
        public string amount_paid { get; set; }
        public string penalty_paid { get; set; }
        public string updation_remarks { get; set; }
        public int? update_compliance_id { get; set; }
        public IList<updatefiledocuments> updatefiledocuments { get; set; }

    }

    public class updatefiledocuments
    {
        public int id { get; set; }
        public int update_compliance_id { get; set; }

        public string file_name { get; set; }
        public string file_type { get; set; }
        public string file_path { get; set; }
        public string nature_of_attachment { get; set; }
        public string status { get; set; }

    }

}
