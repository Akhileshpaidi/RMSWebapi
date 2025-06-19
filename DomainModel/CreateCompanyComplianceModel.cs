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
    [Table("create_company_compliance")]
    public class CreateCompanyComplianceModel
    {
        [Key]
        public int create_company_compliance_id { get; set; }
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
        public int? periodicity_factor { get; set; }
        public string compliance_year_factor { get; set; }
        public int? start_month_compliance_year { get; set; }

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
        public string company_compliance_id { get; set; }
        public DateTime created_date { get; set; }
        public DateTime? effective_from_date { get; set; }
        public DateTime? effective_to_date { get; set; }
        public string created_by { get; set; }
        public DateTime updated_date { get; set; }
        public string status { get; set; }
        public string subscription_type { get; set; }
        public string mapping_status { get; set; }
        public string IsImportedData { get; set; }
        public string updated_by { get; set; }
        public string version_no { get; set; }
        public int? loop_current_periodicity_factor { get; set; }
        public int? loop_next_periodicity_factor { get; set; }
        public int? loop_day_month_periodicity_factor { get; set; }
        public List<CompanyComplianceScheduler> instancesData { get; set; }

       


    }

    [Table("company_compliance_scheduler_master")]

    public class CompanyComplianceScheduler
    {
        [Key]
        public int company_compliance_sheduler_id { get; set; }
        public int create_company_compliance_id { get; set; }
        public string currentcompliancePeriod { get; set; }
        public string nextCompliancePeriod { get; set; }
        public string dueDateDay { get; set; }
        public int? dueDateMonth { get; set; }
        public string extendedDueDateDay { get; set; }
        
        public int? extendedDueDateMonth { get; set; }
        public int? current_periodicity_factor { get; set; }
        public int? next_periodicity_factor { get; set; }
        public int? day_month_periodicity_factor { get; set; }
        public bool extendstatus { get; set; }
        public string IsImportedData { get; set; }
    }

    [Table("periodicity_factor")]
    public class PeriodicityFactorModel
    {
        [Key]
        public int Periodicity_FactorID { get; set; }
        public string Periodicity_Factor_Name { get; set; }
        public string Increment_Factor { get; set; }
    }


    [Table("month")]
    public class MonthModel
    {
        [Key]
        public int MonthID { get; set; }
        public string Month { get; set; }
    }



    //public class complianceuserlocation
    //{
    //    public string locationdepartmentmappingid { get; set; }


    //    public int category_of_law_id { get; set; }

    //    public int law_type_id {  get; set; }

    //    public int rule_id {  get; set; }

    //    public int regulatory_authority_id {  get; set; }

    //    public int jursdiction_category_id {  get; set; }


    //    public int country_id { get; set; }

    //    public int state_id { get; set; }

    //    public int jursdiction_Location_id { get; set; }

    //    public int compliance_type_id { get; set; }


    //    public int frequency_period_id {  get; set; }
    //    }

}



//public class CompanyComplianceScheduler
//{
//    [Key]
//    public int company_compliance_sheduler_id { get; set; }

//    // Define foreign key property
//    [ForeignKey("CreateCompanyComplianceModel")]
//    public int create_company_compliance_id { get; set; }
//    public CreateCompanyComplianceModel CreateCompanyComplianceModel { get; set; }

//    public int currentcompliancePeriod { get; set; }
//    public int nextCompliancePeriod { get; set; }
//    public string dueDateDay { get; set; }
//    public string dueDateMonth { get; set; }
//    public string extendedDueDateDay { get; set; }
//    public string extendedDueDateMonth { get; set; }
//    public bool extendstatus { get; set; }
//}

