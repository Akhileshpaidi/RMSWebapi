using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Bibliography;
using DomainModel;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("generate_current_batch_compliance")]
    public class CurrentBatchCompliance
    {
        [Key]
        public int id { get; set; }
        public int createCompanyComplianceId { get; set; }
        public int batchId { get; set; }
        public string locationDepartmentMappingId { get; set; }
        public int updateUser { get; set; }
        public int reviewUser { get; set; }
        public int remediationUser { get; set; }
        public int approveUser { get; set; }
        public int auditUser { get; set; }
        public int backupUser { get; set; }
        public string startDate { get; set; }
        public string viewUser { get; set; }
        public string monitorUser { get; set; }
        public string frequency { get; set; }
        public string Apply_Scheduler_On { get; set; }
        public string taskId { get; set; }
        public string endDate { get; set; }
        public string status { get; set; }
        public int createdBy { get; set; }
        public DateTime createdDate { get; set; }
    }


    [Table("batch_compliance")]
    public class BatchCompliance
    {
        [Key]
        public string compliance_id { get; set; }
        public DateTime compliance_due_date_created { get; set; }

        [NotMapped]
        public DateTime? extended_compliance_due_date_created { get; set; }
        public string compliance_period { get; set; }
        public string location_department_mapping_id { get; set; }
        public int generate_current_batch_compliance_id { get; set; }
        public int create_company_compliance_id { get; set; }
        public string task_id { get; set; }
        public string status { get; set; }

    }
}
