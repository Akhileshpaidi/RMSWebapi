using DocumentFormat.OpenXml.Bibliography;
using DomainModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("create_global_compliance")]
    public class CreateGlobalComplianceModel
    {
        [Key]
        public int create_global_compliance_id { get; set; }
        public int? act_id { get; set; }
        public int? rule_id { get; set; }
        public string compliance_name { get; set; }

        public string compliance_description { get; set; }
        public string section_rule_regulation_ref { get; set; }//
        public int? category_of_law_id { get; set; }
        public int? law_type_id { get; set; }
        public int? regulatory_authority_id { get; set; }
        public int? jursdiction_category_id { get; set; }
        public int? country_id { get; set; }
        public int? state_id { get; set; }
        public string district { get; set; }
        public int? compliance_type_id { get; set; }
        public int? jursdiction_Location_id { get; set; }
        public int? compliance_record_type_id { get; set; }
        public string form_no_record_name_required { get; set; }
        public string form_no_record_name_reference_ids { get; set; }
        public string special_instr_statutory_form_update { get; set; }
        public int? compliance_group_id { get; set; }
        public int? compliance_notified_status_id { get; set; }
        public int? frequency_period_id { get; set; }
        public string original_due_date_defined_by { get; set; }
        public string recurrence { get; set; }
        public string frequency { get; set; }
        public string day { get; set; }
        public string periodicity_factor { get; set; }
        public string compliance_year_factor { get; set; }
        public string start_month_compliance_year { get; set; }

        public bool extended_due_date_required { get; set; }
        //
        public string bussiness_sector_ids { get; set; }
        public string industry_ids { get; set; }
        public string any_additional_references { get; set; }
        public string penalty_provision_required { get; set; }
        public string compliance_penalty_ids { get; set; }
        public int? suggested_risk_id { get; set; }
        public int? risk_classification_criteria_id { get; set; }
        public string key_words_tags { get; set; }
        public string global_compliance_id { get; set; }
        public DateTime created_date { get; set; }
        public string effective_from_date { get; set; }
        public string effective_to_date { get; set; }
        public string created_by { get; set; }
        public DateTime updated_date { get; set; }
        public string status { get; set; }
        public string subscription_type { get; set; }
        public string mapping_status { get; set; }
        public string IsImportedData { get; set; }
        public string updated_by { get; set; }
        public string version_no {  get; set; }

        public List<GlobalComplianceScheduler> instancesData { get; set; }

    }

    [Table("global_compliance_scheduler_master")]

    public class GlobalComplianceScheduler
    {
        [Key]
        public int global_compliance_sheduler_id { get; set; }
        public int create_global_compliance_id { get; set; }
        public int currentcompliancePeriod { get; set; }
        public int nextCompliancePeriod { get; set; }
        public string dueDateDay { get; set; }
        public string dueDateMonth { get; set; }
        public string extendedDueDateDay { get; set; }
        public string extendedDueDateMonth { get; set; }
        public bool extendstatus { get; set; }
    }

}

