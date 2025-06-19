using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
namespace DomainModel
{
    [Table("remediation_plan")]
    public class RemediationPlanModel
    {
        [Key]
        public int remediation_plan_id { get; set;}
        public string compliance_id { get; set;}
        public int entity_id { get; set;}
        public int unit_location_id { get; set;}
        public int department_id { get; set;}
        public int requester_id { get; set;}
        public int approver_id { get; set; }
        //public int data_for { get; set; }
        //public string compliance_name { get; set;}
        public string remediation_request_remarks { get; set;}
        public string remediation_approval_remarks { get; set;}   
        public string remediation_rejection_remarks { get; set; }
        public string compliance_stage_progress { get;set;}
        public string compliance_status { get; set;}
        public DateTime proposed_remedial_comply_date { get; set;}
        public DateTime rpa_request_date { get; set;}
        public DateTime rpa_approval_date { get; set; }
        public DateTime approved_remedial_comply_date { get; set;}
        public DateTime rpa_reject_date { get; set; }
        //public List<ComplianceDetail> OriginalData { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }

    }
    //public class ComplianceDetail
    //{
    //    public string EntityName { get; set; }
    //    public string ComplianceName { get; set; }
    //    public DateTime DueDate { get; set; }
    //    public string ComplianceStage { get; set; }
    //}


    public class RemediationPlanModels
    {
        
        public int requester_id { get; set;}
        public string proposed_remedial_comply_date { get; set; }
        
        public string remediation_request_remarks { get; set; }
       
        public List<ComplianceData> originalData { get; set; }
        public string Status { get; set; }
    }
    public class ComplianceData
    {
        public string entityName { get; set; }
        public string complianceName { get; set; }
        public DateTime dueDate { get; set; }  // Change to string if dueDate is not a DateTime object
        public string complianceStage { get; set; }
        public int entity_id { get; set; }
        public int unit_location_id { get; set; }
        public int department_id { get; set; }
        public string compliance_id { get; set; }
    }


    public class RemediationPlanModell
    {
        
        public string remediation_approval_remarks { get; set; }
        public string remediation_rejection_remarks { get; set; }

        public string approved_remedial_comply_date { get; set; }
        public int approver_id { get; set; }
        public int rejecter_id { get; set; }

        public List<Remediatemodel> originalData { get; set; }



    }

    public class Remediatemodel
    {
        public int remediation_plan_id { get; set; }
        public string complianceStage { get; set;}
    }

}
