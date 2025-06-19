using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("update_compliance")]
    public class UpdateComplianceModel
    {
        [Key]
        public int update_compliance_id { get; set; }
        public string compliance_id { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string applicability_status { get; set; }
        //public string nature_of_attachment { get; set; }
        public string amount_paid { get; set; }
        public string penalty_paid { get; set; }
        public string updation_remarks { get; set; }
        public DateTime? actual_complied_date { get; set; }
        public string no_of_days_taken_to_update { get; set; }
        public string no_of_days_taken_to_review { get; set; }
        public string no_of_days_taken_to_approve { get; set; }
        public string no_of_days_taken_to_audit { get; set; }
        public DateTime? due_date {  get; set; }
        public string compliance_stage_progress { get; set; }
        public string compliance_status { get; set; }
        public string auditWorkflow { get; set; }
        public string authWorkflow { get; set; }
        public string review_Workflow { get; set; }
        public string approve_Workflow { get; set; }
        public string audit_remarks { get; set; }
        public string reviewer_remarks { get; set; }
        public string approval_remarks { get; set; }
        public string review_activity_result { get; set; }
        public string reject_remarks { get; set; }
        public string updation_activity_result { get; set; }
        public string approver_activity_result { get; set; }
        public string audit_activity_result { get; set; }
        public int? updated_by { get; set; }
        public int? created_by { get; set; }
        public DateTime? reviewer_due_date { get; set; }
        public DateTime? compliance_update_date { get; set; }
        public DateTime? approval_due_date { get; set; }
        public DateTime? audit_date { get; set; }
        public DateTime? approval_date { get; set; }
        public DateTime? reviewer_date { get; set; }
        public DateTime? reject_date { get; set; }
        public List<UpdateComplianceFilesModels> attachments { get; set; } // List of attachments
    }

    public class UpdateComplianceFilesModels
    {
        [Key]
        public int Id { get; set; }

        [NotMapped] // Ignore this property in the database
        public IFormFile file { get; set; }

        public string file_name { get; set; }
        public string file_type { get; set; }
        public string file_path { get; set; }
        public string nature_of_attachment { get; set; }
        public int? update_compliance_id { get; set; }
        public string status { get; set; }

    }

}
