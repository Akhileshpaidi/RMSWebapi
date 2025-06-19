using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using Org.BouncyCastle.Bcpg;
using ITR_TelementaryAPI.Models;
using ITRTelemetry.Models;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Security.AccessControl;
using System.Security.Cryptography;
using Ubiety.Dns.Core;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using System.ComponentModel;
using static Peg.Base.PegBaseParser;
using Microsoft.Extensions.DependencyInjection;

namespace ITRTelemetry.Controllers
{
    //[Route("api/[controller]")]
    // [ApiController]
    [Produces("application/json")]
    public class CreateCompanyComplianceController : ControllerBase
    {
        private MySqlDBContext mySqlDBContext;
        private readonly IServiceProvider _serviceProvider;
        public IConfiguration Configuration { get; }
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CreateCompanyComplianceController(MySqlDBContext mySqlDBContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;

        }

        [Route("api/createcompanycompliance/GetactandruleDetails/{ruleid}")]
        [HttpGet]

        public IEnumerable<object> GetactandruleDetails(int ruleid)
        {
            try
            {
                var result = (from act_rule_regulatory in mySqlDBContext.Rulesandregulatorymodels
                              join actmaster in mySqlDBContext.Actregulatorymodels on act_rule_regulatory.actregulatoryid equals actmaster.actregulatoryid
                              join country in mySqlDBContext.CountryModels on act_rule_regulatory.id equals country.id into countryGroup
                              from countrynull in countryGroup.DefaultIfEmpty()
                              join state in mySqlDBContext.StateModels on act_rule_regulatory.State_id equals state.id into stateGroup
                              from statenull in stateGroup.DefaultIfEmpty()
                              join district in mySqlDBContext.JurisdictionLocationModels on act_rule_regulatory.jurisdiction_location_id equals district.jurisdiction_location_id into districtGroup
                              from districtnull in districtGroup.DefaultIfEmpty()
                              join jurisdiction in mySqlDBContext.Jurisdictionmodels on act_rule_regulatory.jurisdiction_category_id equals (int?)jurisdiction.jurisdiction_category_id into jurisdictiongroup
                              from jursd in jurisdictiongroup.DefaultIfEmpty()
                              where act_rule_regulatory.status == "Active" && act_rule_regulatory.act_rule_regulatory_id == ruleid
                              select new
                              {
                                  ActRuleRegulatoryId = act_rule_regulatory.act_rule_regulatory_id,
                                  actid = act_rule_regulatory.actregulatoryid,
                                  rule_name = act_rule_regulatory.act_rule_name,
                                  LawTypeId = act_rule_regulatory.law_type_id,
                                  CategoryOfLawId = act_rule_regulatory.category_of_law_ID,
                                  jurisdiction_category_id = (jursd != null) ? jursd.jurisdiction_category_id : (int?)null,
                                  jurisdiction_category_name = (jursd != null) ? jursd.jurisdiction_categoryname : "N/A",
                                  JurisdictionLocationDistrict = (districtnull != null) ? districtnull.jurisdiction_district : "N/A",
                                  RegulatoryAuthorityId = act_rule_regulatory.regulatory_authority_id,
                                  ActRuleApplDes = act_rule_regulatory.act_rule_appl_des,
                                  type_of_business = act_rule_regulatory.type_bussiness,
                                  bussiness_operations = act_rule_regulatory.bussiness_operations,
                                  no_of_employees = act_rule_regulatory.no_of_employees,
                                  bussiness_investment = act_rule_regulatory.bussiness_investment,
                                  bussiness_turnover = act_rule_regulatory.bussiness_turnover,
                                  working_conditions = act_rule_regulatory.working_conditions,
                                  bussiness_registration = act_rule_regulatory.bussiness_registration,
                                  other_factor = act_rule_regulatory.other_factor,
                                  act_name = actmaster.actregulatoryname,
                                  country = (countrynull != null) ? countrynull.name : "N/A",
                                  state = (statenull != null) ? statenull.name : "N/A",
                                  countryid = (countrynull != null) ? countrynull.id : (int?)null,
                                  stateid = (statenull != null) ? statenull.id : (int?)null,
                                  act_description = actmaster.actrequlatorydescription
                              }).ToList();

                // Fetch all files associated with the same act ID
                var actFiles = mySqlDBContext.Actregulatoryfilemodels
                                             .Where(af => result.Select(r => r.actid).Contains(af.actregulatoryid))
                                             .Select(af => new { af.filecategory, af.filepath })
                                             .ToList();

                var joinedResult = (from item in result
                                    join law_type in mySqlDBContext.LawTypeModels on item.LawTypeId equals law_type.law_type_id into lawgroup
                                    from law in lawgroup.DefaultIfEmpty()
                                    join category in mySqlDBContext.catageoryoflawmodels on item.CategoryOfLawId equals category.category_of_law_ID into categorygroup
                                    from cat in categorygroup.DefaultIfEmpty()
                                        //join jurisdiction in mySqlDBContext.SupAdmin_JurisdictionListModels on item.jurisdiction_category_id equals jurisdiction.jurisdiction_category_id into jurisdictiongroup
                                        //from jursd in jurisdictiongroup.DefaultIfEmpty()
                                        //join jurisdiction_location in mySqlDBContext.SupAdmin_JurisdictionLocationModels on item.JurisdictionLocationId equals jurisdiction_location.jurisdiction_location_id into jurisdiction_locationgroup
                                        //from jurd_loc in jurisdiction_locationgroup.DefaultIfEmpty()
                                    join regulator in mySqlDBContext.RegulatoryAuthorityModels on item.RegulatoryAuthorityId equals regulator.regulatory_authority_id into regulatorgroup
                                    from regulator in regulatorgroup.DefaultIfEmpty()
                                    join ruleFiles in mySqlDBContext.ActRuleregulatoryfilemodels on item.ActRuleRegulatoryId equals ruleFiles.act_rule_regulatory_id into ruleFilesGroup
                                    from rule in ruleFilesGroup.DefaultIfEmpty()
                                    select new
                                    {
                                        countryName = item.country,
                                        stateName = item.state,
                                        lawID = item.LawTypeId,
                                        lawType = law.type_of_law,
                                        categoryID = item.CategoryOfLawId,
                                        category = cat.law_Categoryname,
                                        countryId = item.countryid,
                                        stateId = item.stateid,
                                        jurisdiction_district = item.JurisdictionLocationDistrict,
                                        jurisdictionCategory = item.jurisdiction_category_name,
                                        jurisdictionCategoryID = item.jurisdiction_category_id,
                                        regulatorID = item.RegulatoryAuthorityId,
                                        regulator = regulator.regulatory_authority_name,
                                        description = item.ActRuleApplDes,
                                        rulename = item.rule_name,
                                        actname = item.act_name,
                                        act_description = item.act_description,
                                        type_of_business = item.type_of_business,
                                        bussiness_operations = item.bussiness_operations,
                                        no_of_employees = item.no_of_employees,
                                        bussiness_investment = item.bussiness_investment,
                                        bussiness_turnover = item.bussiness_turnover,
                                        working_conditions = item.working_conditions,
                                        bussiness_registration = item.bussiness_registration,
                                        other_factor = item.other_factor,
                                        rulefiles = ruleFilesGroup.Select(rf => new { rf.filecategory, rf.filepath }).ToList(),
                                        actfiles = actFiles,
                                    }).ToList();


                return joinedResult.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw; // Propagate the exception up the call stack
            }

        }


        [Route("api/createcompanycompliance/GetGlobalComplianceDetails")]
        [HttpGet]
        public IEnumerable<object> GetGlobalComplianceDetails()
        {

            try
            {
                var result = (from view_global_compliaces in mySqlDBContext.CreateCompanyComplianceModels
                              join act_regulatory in mySqlDBContext.Actregulatorymodels on view_global_compliaces.act_id equals act_regulatory.actregulatoryid
                              join rule_regulatory in mySqlDBContext.Rulesandregulatorymodels on view_global_compliaces.rule_id equals rule_regulatory.act_rule_regulatory_id
                              join compliance_type in mySqlDBContext.ComplianceModels on view_global_compliaces.compliance_type_id equals compliance_type.compliance_type_id
                              join catgoryoflaw in mySqlDBContext.catageoryoflawmodels on view_global_compliaces.category_of_law_id equals catgoryoflaw.category_of_law_ID
                              join RegulatoryAuthority in mySqlDBContext.RegulatoryAuthorityModels on view_global_compliaces.regulatory_authority_id equals RegulatoryAuthority.regulatory_authority_id
                              join jurisdictor in mySqlDBContext.Jurisdictionmodels on view_global_compliaces.jursdiction_category_id equals jurisdictor.jurisdiction_category_id
                              join country in mySqlDBContext.CountryModels on view_global_compliaces.country_id equals country.id into countryGroup
                              from countrynull in countryGroup.DefaultIfEmpty()
                              join state in mySqlDBContext.StateModels on view_global_compliaces.state_id equals state.id into stateGroup
                              from statenull in stateGroup.DefaultIfEmpty()
                              join district in mySqlDBContext.JurisdictionLocationModels on view_global_compliaces.jursdiction_Location_id equals district.jurisdiction_location_id into districtGroup
                              from districtnull in districtGroup.DefaultIfEmpty()
                              join compliancrecordtype in mySqlDBContext.ComplianceRecordTypeModels on view_global_compliaces.compliance_record_type_id equals compliancrecordtype.compliance_record_type_id 
                              join compliancgroup in mySqlDBContext.ComplianceGroupModels on view_global_compliaces.compliance_group_id equals compliancgroup.compliance_group_id
                              join compliancenotified in mySqlDBContext.ComplianceNotifiedStatusModels on view_global_compliaces.compliance_notified_status_id equals compliancenotified.compliance_notified_id
                              join compliancefreq in mySqlDBContext.Frequencymodels on view_global_compliaces.frequency_period_id equals compliancefreq.frequencyid
                              join classificationrisk in mySqlDBContext.ComplianceRiskClassificationCriteriaModels on view_global_compliaces.risk_classification_criteria_id equals classificationrisk.compliance_risk_criteria_id
                              select new
                              { 
                                  act_id = act_regulatory.actregulatoryid,
                                  act_name = act_regulatory.actregulatoryname,
                                  company_compliance_id = view_global_compliaces.company_compliance_id,
                                  rule_id = rule_regulatory.act_rule_regulatory_id,
                                  rule_name = rule_regulatory.act_rule_name,
                                  global_compliance_id = view_global_compliaces.company_compliance_id,
                                  compliance_name = view_global_compliaces.compliance_name,
                                  compliance_des = view_global_compliaces.compliance_description,
                                  compliance_type = compliance_type.compliance_type_name,
                                  keywors_tags = view_global_compliaces.key_words_tags,
                                  compliance_type_id = view_global_compliaces.compliance_type_id,
                                  create_compliance_id = view_global_compliaces.create_company_compliance_id,
                                  law_Categoryname = catgoryoflaw.law_Categoryname,
                                  regulatory_authority_name = RegulatoryAuthority.regulatory_authority_name,
                                  country = (countrynull != null) ? countrynull.name : "N/A",
                                  state = (statenull != null) ? statenull.name : "N/A",
                                  JurisdictionLocationDistrict = (districtnull != null) ? districtnull.jurisdiction_district : "N/A",
                                  compliance_record_name = compliancrecordtype.compliance_record_name,
                                  compliance_group_name = compliancgroup.compliance_group_name,
                                  compliance_notified_name = compliancenotified.compliance_notified_name,
                                  compliance_risk_criteria_name = classificationrisk.compliance_risk_criteria_name,
                                  //bussiness_operations = act_rule_regulatory.bussiness_operations,
                                  //no_of_employees = act_rule_regulatory.no_of_employees,
                                  //bussiness_investment = act_rule_regulatory.bussiness_investment,
                                  //bussiness_turnover = act_rule_regulatory.bussiness_turnover,
                                  //working_conditions = act_rule_regulatory.working_conditions,
                                  //bussiness_registration = act_rule_regulatory.bussiness_registration,
                                  //other_factor = act_rule_regulatory.other_factor,

                              }).ToList();
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;

            }

        }



        [Route("api/createcompanycompliance/GetSelectedGlobalComplianceDetailsByID/{complianceID}")]
        [HttpGet]
        public IEnumerable<object> GetSelectedGlobalComplianceDetailsByID(int complianceID)
        {

            try
            {

                var result = (from view_global_compliances in mySqlDBContext.CreateCompanyComplianceModels
                              join act_regulatory in mySqlDBContext.Actregulatorymodels on view_global_compliances.act_id equals act_regulatory.actregulatoryid into actRegGroup
                              from actReg in actRegGroup.DefaultIfEmpty()
                              join rule_regulatory in mySqlDBContext.Rulesandregulatorymodels on view_global_compliances.rule_id equals rule_regulatory.act_rule_regulatory_id into ruleRegGroup
                              from ruleReg in ruleRegGroup.DefaultIfEmpty()
                              join compliance_type in mySqlDBContext.ComplianceModels on view_global_compliances.compliance_type_id equals compliance_type.compliance_type_id into compTypeGroup
                              from compType in compTypeGroup.DefaultIfEmpty()
                              join compliance_record_type in mySqlDBContext.ComplianceRecordTypeModels on view_global_compliances.compliance_record_type_id equals compliance_record_type.compliance_record_type_id into compRecTypeGroup
                              from compRecType in compRecTypeGroup.DefaultIfEmpty()
                              join law_type in mySqlDBContext.LawTypeModels on view_global_compliances.law_type_id equals law_type.law_type_id into lawTypeGroup
                              from lawType in lawTypeGroup.DefaultIfEmpty()
                              join category in mySqlDBContext.catageoryoflawmodels on view_global_compliances.category_of_law_id equals category.category_of_law_ID into categoryGroup
                              from cat in categoryGroup.DefaultIfEmpty()
                              join jurisdiction in mySqlDBContext.Jurisdictionmodels on view_global_compliances.jursdiction_category_id equals jurisdiction.jurisdiction_category_id into jurisGroup
                              from juris in jurisGroup.DefaultIfEmpty()
                              join jurisdiction_location in mySqlDBContext.JurisdictionLocationModels on view_global_compliances.jursdiction_Location_id equals jurisdiction_location.jurisdiction_location_id into jurisLocGroup
                              from jurisLoc in jurisLocGroup.DefaultIfEmpty()
                              join regulator in mySqlDBContext.RegulatoryAuthorityModels on view_global_compliances.regulatory_authority_id equals regulator.regulatory_authority_id into regulatorGroup
                              from reg in regulatorGroup.DefaultIfEmpty()
                              join country in mySqlDBContext.CountryModels on jurisLoc.jurisdiction_country_id equals country.id into countryGroup
                              from coun in countryGroup.DefaultIfEmpty()
                              join state in mySqlDBContext.StateModels on jurisLoc.jurisdiction_state_id equals state.id into stateGroup
                              from st in stateGroup.DefaultIfEmpty()
                              join compliance_group in mySqlDBContext.ComplianceGroupModels on view_global_compliances.compliance_group_id equals compliance_group.compliance_group_id into compGroupGroup
                              from compGroup in compGroupGroup.DefaultIfEmpty()
                              join compliance_notified_status in mySqlDBContext.ComplianceNotifiedStatusModels on view_global_compliances.compliance_notified_status_id equals compliance_notified_status.compliance_notified_id into compNotifiedGroup
                              from compNotified in compNotifiedGroup.DefaultIfEmpty()
                              join frequency_period in mySqlDBContext.Frequencymodels on view_global_compliances.frequency_period_id equals frequency_period.frequencyid into freqGroup
                              from freq in freqGroup.DefaultIfEmpty()
                              join risk_classification_criteria in mySqlDBContext.ComplianceRiskClassificationCriteriaModels on view_global_compliances.risk_classification_criteria_id equals risk_classification_criteria.compliance_risk_criteria_id into riskCriteriaGroup
                              from riskCriteria in riskCriteriaGroup.DefaultIfEmpty()
                              join suggested_risk_classification in mySqlDBContext.ComplianceRiskClassificationModels on view_global_compliances.suggested_risk_id equals suggested_risk_classification.compliance_risk_classification_id into suggRiskGroup
                              from suggRisk in suggRiskGroup.DefaultIfEmpty()
                              join created_details in mySqlDBContext.usermodels on view_global_compliances.created_by equals created_details.USR_ID.ToString() into userGroup
                              from created in userGroup.DefaultIfEmpty()
                              join updated_details in mySqlDBContext.usermodels on view_global_compliances.updated_by equals updated_details.USR_ID.ToString() into updatedGroup
                              from updated in updatedGroup.DefaultIfEmpty()

                              where view_global_compliances.create_company_compliance_id == complianceID
                              select new
                              {
                                  act_name = actReg.actregulatoryname,
                                  act_id = actReg.actregulatoryid,
                                  rule_name = ruleReg.act_rule_name,
                                  rule_id = ruleReg.act_rule_regulatory_id,
                                  type_of_business = ruleReg.type_bussiness,
                                  bussiness_operations = ruleReg.bussiness_operations,
                                  no_of_employees = ruleReg.no_of_employees,
                                  bussiness_investment = ruleReg.bussiness_investment,
                                  bussiness_turnover = ruleReg.bussiness_turnover,
                                  working_conditions = ruleReg.working_conditions,
                                  bussiness_registration = ruleReg.bussiness_registration,
                                  other_factor = ruleReg.other_factor,
                                  global_compliance_id = view_global_compliances.company_compliance_id,
                                  compliance_name = view_global_compliances.compliance_name,
                                  compliance_des = view_global_compliances.compliance_description,
                                  keywors_tags = view_global_compliances.key_words_tags,
                                  country_name = coun.name,
                                  state_name = st.name,
                                  law_type = lawType.type_of_law,
                                  category = cat.law_Categoryname,
                                  jurisdiction_district = jurisLoc.jurisdiction_district,
                                  jurisdiction_category = juris.jurisdiction_categoryname,
                                  regulator = reg.regulatory_authority_name,
                                  compliance_type_name = compType.compliance_type_name,
                                  compliance_record_type = compRecType.compliance_record_name,
                                  section_rule_regulation_ref = view_global_compliances.section_rule_regulation_ref,
                                  special_instr_for_statutory = view_global_compliances.special_instr_statutory_form_update,
                                  compliance_group = compGroup.compliance_group_name,
                                  compliance_notified_status = compNotified.compliance_notified_name,
                                  frequency_period_name = freq.frequencyperiod,
                                  recurrence = view_global_compliances.recurrence,
                                  frequency = view_global_compliances.frequency,
                                  day = view_global_compliances.day,
                                  periodicity_factor = view_global_compliances.periodicity_factor,
                                  original_due_date_defined_by = view_global_compliances.original_due_date_defined_by,
                                  extended_due_date_applied = view_global_compliances.extended_due_date_required,
                                  effective_from = view_global_compliances.effective_from_date,
                                  effective_till = view_global_compliances.effective_to_date,
                                  any_additional_references = view_global_compliances.any_additional_references,
                                  form_no_record_name_required = view_global_compliances.form_no_record_name_required,
                                  form_no_record_name_reference_ids = view_global_compliances.form_no_record_name_reference_ids,
                                  penalty_provision_required = view_global_compliances.penalty_provision_required,
                                  compliance_penalty_ids = view_global_compliances.compliance_penalty_ids,
                                  business_ids = view_global_compliances.bussiness_sector_ids,
                                  industry_ids = view_global_compliances.industry_ids,
                                  risk_classification_criteria_name = riskCriteria.compliance_risk_criteria_name,
                                  suggested_risk_classification_name = suggRisk.compliance_risk_classification_name,
                                  created_by = created.firstname,
                                 created_date = view_global_compliances.created_date,
                                 updated_by = updated.firstname,
                                 updated_date = view_global_compliances.updated_date,
                                  version_no = view_global_compliances.version_no
                              }).ToList();

                var actFiles = mySqlDBContext.Actregulatoryfilemodels
                             .Where(af => result.Select(r => r.act_id).Contains(af.actregulatoryid))
                             .Select(af => new { af.filecategory, af.filepath })
                             .ToList();

                var ruleFiles = mySqlDBContext.ActRuleregulatoryfilemodels
                             .Where(rf => result.Select(r => r.rule_id).Contains(rf.act_rule_regulatory_id))
                             .Select(rf => new { rf.filecategory, rf.filepath })
                             .ToList();

                var combinedResults = new List<object>();
                // List<(string businessName, List<string> industryNames)> businessAndIndustries = new List<(string businessName, List<string> industryNames)>();
                List<string> formattedBusinessAndIndustries = new List<string>();
                List<object> statutoryFormDetails = new List<object>();
                List<object> penaltyDetails = new List<object>(); // Initialize penalty details list

                foreach (var item in result)
                {
                    if (!string.IsNullOrEmpty(item.business_ids) && item.business_ids != "N/A")
                    {
                        List<string> businessIds = item.business_ids?.Split(',').ToList();
                        List<string> industryIds = item.industry_ids?.Split(',').ToList();

                        foreach (var businessId in businessIds)
                        {
                            int id = int.Parse(businessId.Trim()); // Convert each business ID to integer

                            // Query the business name for the current business ID
                            var businessName = (from business in mySqlDBContext.Businesssectormodels
                                                where business.businesssectorid == id
                                                select business.businesssectorname).FirstOrDefault();

                            if (businessName != null)
                            {
                                List<string> industryNames = new List<string>();

                                if (industryIds != null && industryIds.Any())
                                {
                                    foreach (var industryId in industryIds)
                                    {
                                        int industryIdInt = int.Parse(industryId.Trim()); // Convert each industry ID to integer

                                        // Query the industry name for the current business ID and industry ID
                                        var industryName = (from industry in mySqlDBContext.industrytypemodels
                                                            where industry.businesssectorid == id && industry.industrytypeid == industryIdInt
                                                            select industry.industrytypename).FirstOrDefault();

                                        if (industryName != null)
                                        {
                                            industryNames.Add(industryName); // Add the industry name to the list
                                        }
                                    }
                                }

                                // Combine business and industry names
                                string combinedNames = $"{businessName} - {string.Join(", ", industryNames)}";
                                formattedBusinessAndIndustries.Add(combinedNames); // Add the formatted string to the list
                            }
                        }
                    }

                    if (item.form_no_record_name_required == "Yes")
                    {
                        // Fetch statutory form details
                        var referenceIds = item.form_no_record_name_reference_ids?.Split(',')
                        .Select(id => int.Parse(id.Trim()))
                        .ToList();

                        foreach (var statutoryId in referenceIds)
                        {
                            var statutoryDetails = mySqlDBContext.statutoryformsrecordsmodels
                                .Where(statutory => statutory.statutoryformsid == statutoryId)
                                .Select(statutory => new
                                {
                                    statutoryid = statutory.statutoryformsid,
                                    statutoryName = statutory.recordformsname,
                                    applicable_section = statutory.applicationrefernce,
                                    description = statutory.recordformsdesc,
                                })
                                .FirstOrDefault(); // Retrieve statutory details for the current statutoryId

                            if (statutoryDetails != null)
                            {
                                var filesForStatutoryId = mySqlDBContext.statutoryformsrecordsfilemodels
                                    .Where(files => files.statutoryformsid == statutoryId)
                                    .ToList();

                                var links = filesForStatutoryId
                                    .Where(file => file.filecategory == "Weblink")
                                    .Select(file => new
                                    {
                                        filecategory = file.filecategory,
                                        filepath = file.filepath
                                    })
                                    .ToList();

                                var files = filesForStatutoryId
                                    .Where(file => file.filecategory != "Weblink")
                                    .Select(file => new
                                    {
                                        filecategory = file.filecategory,
                                        filepath = file.filepath
                                    })
                                    .ToList();

                                var statutoryFormDetail = new
                                {
                                    statutoryid = statutoryDetails.statutoryid,
                                    statutoryName = statutoryDetails.statutoryName,
                                    applicable_section = statutoryDetails.applicable_section,
                                    description = statutoryDetails.description,
                                    statutoryDetails = links.Concat(files).ToList()
                                };

                                statutoryFormDetails.Add(statutoryFormDetail);
                            }
                        }
                    }

                    if (item.penalty_provision_required == "Yes")
                    {
                        // Fetch penalty details only if penalty provision is required
                        var penaltyIds = item.compliance_penalty_ids?.Split(',')
                                                .Select(id => int.Parse(id.Trim()))
                                                .ToList();

                        foreach (var penaltyId in penaltyIds)
                        {
                            var penaltyDetail = mySqlDBContext.CompliancepenatlymasterModels
                                .Where(penalty => penalty.compliancepenaltyid == penaltyId)
                                .Join(mySqlDBContext.PenaltyCategoryModels,
                                      penalty => penalty.penalty,
                                      penaltyCategory => penaltyCategory.penalty_category_id,
                                      (penalty, penaltyCategory) => new
                                      {
                                          penalty.compliancepenaltyid,
                                          penaltyCategory.penalty_category_name,
                                          penalty.applicationselectionrule,
                                          penalty.penaltydesc,
                                          penalty.minpenalty,
                                          penalty.maxpenalty,
                                          penalty.additionalrefernce
                                      })
                                .FirstOrDefault();

                            if (penaltyDetail != null)
                            {
                                var filesForCompliancePenaltyId = mySqlDBContext.PenaltyCategoryfileModels
                                    .Where(files => files.compliancepenaltyid == penaltyId)
                                    .ToList();

                                var links = filesForCompliancePenaltyId
                                    .Where(file => file.filecategory == "Weblink")
                                    .Select(file => new
                                    {
                                        filecategory = file.filecategory,
                                        filepath = file.filepath
                                    })
                                    .ToList();

                                var files = filesForCompliancePenaltyId
                                    .Where(file => file.filecategory != "Weblink")
                                    .Select(file => new
                                    {
                                        filecategory = file.filecategory,
                                        filepath = file.filepath
                                    })
                                    .ToList();

                                var combinedPenaltyDetail = new
                                {
                                    compliancepenaltyid = penaltyDetail.compliancepenaltyid,
                                    compliancepenaltyName = penaltyDetail.penalty_category_name,
                                    applicable_section = penaltyDetail.applicationselectionrule,
                                    description = penaltyDetail.penaltydesc,
                                    minimum = penaltyDetail.minpenalty,
                                    maximum = penaltyDetail.maxpenalty,
                                    additionalreference = penaltyDetail.additionalrefernce,
                                    PenaltyDetails = links.Concat(files).ToList()
                                };

                                penaltyDetails.Add(combinedPenaltyDetail);
                            }
                        }
                    }

                    // Construct the combined result object including both statutory form details and penalty details
                    var combinedResult = new
                    {
                        item.act_name,
                        act_files = actFiles,
                        rule_files = ruleFiles,
                        item.rule_name,
                        item.type_of_business,
                        item.bussiness_operations,
                        item.no_of_employees,
                        item.bussiness_investment,
                        item.bussiness_turnover,
                        item.working_conditions,
                        item.bussiness_registration,
                        item.other_factor,
                        item.global_compliance_id,
                        item.compliance_name,
                        item.compliance_des,
                        item.keywors_tags,
                        item.country_name,
                        item.state_name,
                        item.law_type,
                        item.category,
                        item.jurisdiction_district,
                        item.jurisdiction_category,
                        item.regulator,
                        item.compliance_type_name,
                        item.compliance_record_type,
                        item.section_rule_regulation_ref,
                        item.special_instr_for_statutory,
                        item.compliance_group,
                        item.compliance_notified_status,
                        item.frequency_period_name,
                        item.original_due_date_defined_by,
                        item.extended_due_date_applied,
                        item.effective_from,
                        item.effective_till,
                        item.any_additional_references,
                        item.form_no_record_name_required,
                        item.penalty_provision_required,
                        item.recurrence,
                        item.frequency,
                        item.day,
                        item.periodicity_factor,
                        item.risk_classification_criteria_name,
                        item.suggested_risk_classification_name,
                        statutoryFormDetails, // Add the fetched statutory form details
                        penaltyDetails, // Add the fetched penalty details
                        BusinessAndIndustries = formattedBusinessAndIndustries,
                        item.created_by,
                        item.created_date,
                        item.updated_by,
                        item.updated_date,
                        item.version_no,
                    };

                    combinedResults.Add(combinedResult);

                    //foreach (var entry in businessAndIndustries)
                    //{
                    //    Console.WriteLine($"Business: {entry.businessName}");
                    //    Console.WriteLine("Industries: " + string.Join(", ", entry.industryNames));
                    //}
                }

                return combinedResults;


            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;

            }

        }

        [Route("api/createcompanycompliance/InsertCompanyCompliance")]
        [HttpPost]
        public async Task<IActionResult> InsertCompanyCompliance([FromBody] CreateCompanyComplianceModel createCompanyComplianceModels)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string connectionString = Configuration.GetConnectionString("myDb1");
            string generated_id = null;

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    await con.OpenAsync();

                    using (MySqlTransaction transaction = await con.BeginTransactionAsync())
                    {
                        try
                        {
                            //increment start from 0 in company compliance  
                            var Maxcompanycomplianceid = this.mySqlDBContext.CreateCompanyComplianceModels.Where(d => d.IsImportedData == "No").Max(d => (int?)d.create_company_compliance_id) ?? 0;

                            var newcompanycomplianceid = Maxcompanycomplianceid + 1;
                            // Remove spaces from the new compliance name
                            string sanitizedNewComplianceName = createCompanyComplianceModels.compliance_name.Replace(" ", "");

                            // Check if compliance name (ignoring spaces) already exists
                            string checkExistenceQuery = "SELECT COUNT(*) FROM create_company_compliance WHERE REPLACE(compliance_name, ' ', '') = @SanitizedComplianceName";
                            using (MySqlCommand checkExistenceCommand = new MySqlCommand(checkExistenceQuery, con))
                            {
                                checkExistenceCommand.Parameters.AddWithValue("@SanitizedComplianceName", sanitizedNewComplianceName);
                                int existingCount = Convert.ToInt32(await checkExistenceCommand.ExecuteScalarAsync());
                                if (existingCount > 0)
                                {
                                    return Conflict("Compliance name already exists");
                                }
                            }

                            // Insert into CreateCompanyComplianceModel table
                            string insertCreateCompanyComplianceQuery = @"
                INSERT INTO create_company_compliance 
                (create_company_compliance_id,act_id, rule_id, compliance_name, compliance_description, section_rule_regulation_ref, 
                category_of_law_id, law_type_id,country_id, state_id, district, jursdiction_category_id, regulatory_authority_id, jursdiction_Location_id,
                compliance_type_id, compliance_record_type_id, form_no_record_name_required, 
                form_no_record_name_reference_ids, special_instr_statutory_form_update, compliance_group_id, 
                compliance_notified_status_id, frequency_period_id, original_due_date_defined_by, recurrence, 
                frequency, day, periodicity_factor, compliance_year_factor, start_month_compliance_year, 
                extended_due_date_required, bussiness_sector_ids, industry_ids, any_additional_references, 
                penalty_provision_required, compliance_penalty_ids, suggested_risk_id, risk_classification_criteria_id, 
                key_words_tags, company_compliance_id, created_date, effective_from_date, effective_to_date, 
                created_by, updated_date,updated_by, status, subscription_type, mapping_status, IsImportedData, version_no,loop_current_periodicity_factor,loop_next_periodicity_factor,loop_day_month_periodicity_factor)
                VALUES
                (@create_company_compliance_id,@ActId, @RuleId, @ComplianceName, @ComplianceDescription, @SectionRuleRegulationRef, 
                @categoryID,@lawID,@countryId, @StateId, @District, @JurisdictionCategoryID, @RegulatorID, @jurisdictionLocationID,
                @ComplianceTypeId, @ComplianceRecordTypeId, @FormNoRecordNameRequired, 
                @FormNoRecordNameReferenceIds, @SpecialInstrStatutoryFormUpdate, @ComplianceGroupId, 
                @ComplianceNotifiedStatusId, @FrequencyPeriodId, @OriginalDueDateDefinedBy, @Recurrence, 
                @Frequency, @Day, @PeriodicityFactor, @ComplianceYearFactor, @StartMonthComplianceYear, 
                @ExtendedDueDateRequired, @BussinessSectorIds, @IndustryIds, @AnyAdditionalReferences, 
                @PenaltyProvisionRequired, @CompliancePenaltyIds, @SuggestedRiskId, @RiskClassificationCriteriaId, 
                @KeyWordsTags, @CompanyComplianceId, @CreatedDate, @EffectiveFromDate, @EffectiveToDate, 
                @CreatedBy,  @UpdatedDate, @updated_by, @Status, @SubscriptionType, @MappingStatus, @IsImportedData, @version_no, @loop_current_periodicity_factor,@loop_next_periodicity_factor,@loop_day_month_periodicity_factor);
                SELECT LAST_INSERT_ID();";

                            using (MySqlCommand mycommand = new MySqlCommand(insertCreateCompanyComplianceQuery, con))
                            {
                                string company_compliance_id = GenerateCompanyComplianceId(createCompanyComplianceModels.rule_id);
                                generated_id = company_compliance_id;
                                // Parameters setting code here
                                mycommand.Parameters.AddWithValue("@create_company_compliance_id", newcompanycomplianceid);
                                mycommand.Parameters.AddWithValue("@ActId", createCompanyComplianceModels.act_id);
                                mycommand.Parameters.AddWithValue("@RuleId", createCompanyComplianceModels.rule_id);
                                mycommand.Parameters.AddWithValue("@ComplianceName", createCompanyComplianceModels.compliance_name);
                                mycommand.Parameters.AddWithValue("@ComplianceDescription", createCompanyComplianceModels.compliance_description);
                                mycommand.Parameters.AddWithValue("@SectionRuleRegulationRef", createCompanyComplianceModels.section_rule_regulation_ref);
                                mycommand.Parameters.AddWithValue("@lawID", createCompanyComplianceModels.law_type_id);
                                mycommand.Parameters.AddWithValue("@categoryID", createCompanyComplianceModels.category_of_law_id);
                                mycommand.Parameters.AddWithValue("@countryId", createCompanyComplianceModels.country_id);
                                mycommand.Parameters.AddWithValue("@StateId", createCompanyComplianceModels.state_id);
                                mycommand.Parameters.AddWithValue("@JurisdictionCategoryID", createCompanyComplianceModels.jursdiction_category_id);
                                mycommand.Parameters.AddWithValue("@jurisdictionLocationID", createCompanyComplianceModels.jursdiction_Location_id);
                                mycommand.Parameters.AddWithValue("@RegulatorID", createCompanyComplianceModels.regulatory_authority_id);
                                mycommand.Parameters.AddWithValue("@District", createCompanyComplianceModels.district);
                                mycommand.Parameters.AddWithValue("@ComplianceTypeId", createCompanyComplianceModels.compliance_type_id);
                                mycommand.Parameters.AddWithValue("@ComplianceRecordTypeId", createCompanyComplianceModels.compliance_record_type_id);
                                mycommand.Parameters.AddWithValue("@FormNoRecordNameRequired", createCompanyComplianceModels.form_no_record_name_required);
                                mycommand.Parameters.AddWithValue("@FormNoRecordNameReferenceIds", createCompanyComplianceModels.form_no_record_name_reference_ids);
                                mycommand.Parameters.AddWithValue("@SpecialInstrStatutoryFormUpdate", createCompanyComplianceModels.special_instr_statutory_form_update);
                                mycommand.Parameters.AddWithValue("@ComplianceGroupId", createCompanyComplianceModels.compliance_group_id);
                                mycommand.Parameters.AddWithValue("@ComplianceNotifiedStatusId", createCompanyComplianceModels.compliance_notified_status_id);
                                mycommand.Parameters.AddWithValue("@FrequencyPeriodId", createCompanyComplianceModels.frequency_period_id);
                                mycommand.Parameters.AddWithValue("@OriginalDueDateDefinedBy", createCompanyComplianceModels.original_due_date_defined_by);
                                mycommand.Parameters.AddWithValue("@Recurrence", createCompanyComplianceModels.recurrence);
                                mycommand.Parameters.AddWithValue("@Frequency", createCompanyComplianceModels.frequency);
                                mycommand.Parameters.AddWithValue("@Day", createCompanyComplianceModels.day);
                                mycommand.Parameters.AddWithValue("@PeriodicityFactor", createCompanyComplianceModels.periodicity_factor);
                                mycommand.Parameters.AddWithValue("@ComplianceYearFactor", createCompanyComplianceModels.compliance_year_factor);
                                mycommand.Parameters.AddWithValue("@StartMonthComplianceYear", createCompanyComplianceModels.start_month_compliance_year);
                                mycommand.Parameters.AddWithValue("@ExtendedDueDateRequired", createCompanyComplianceModels.extended_due_date_required);
                                mycommand.Parameters.AddWithValue("@BussinessSectorIds", createCompanyComplianceModels.bussiness_sector_ids);
                                mycommand.Parameters.AddWithValue("@IndustryIds", createCompanyComplianceModels.industry_ids);
                                mycommand.Parameters.AddWithValue("@AnyAdditionalReferences", createCompanyComplianceModels.any_additional_references);
                                mycommand.Parameters.AddWithValue("@PenaltyProvisionRequired", createCompanyComplianceModels.penalty_provision_required);
                                mycommand.Parameters.AddWithValue("@CompliancePenaltyIds", createCompanyComplianceModels.compliance_penalty_ids);
                                mycommand.Parameters.AddWithValue("@SuggestedRiskId", createCompanyComplianceModels.suggested_risk_id);
                                mycommand.Parameters.AddWithValue("@RiskClassificationCriteriaId", createCompanyComplianceModels.risk_classification_criteria_id);
                                mycommand.Parameters.AddWithValue("@KeyWordsTags", createCompanyComplianceModels.key_words_tags);
                                mycommand.Parameters.AddWithValue("@CompanyComplianceId", company_compliance_id);
                                mycommand.Parameters.AddWithValue("@CreatedDate", System.DateTime.Now);
                                mycommand.Parameters.AddWithValue("@EffectiveFromDate", createCompanyComplianceModels.effective_from_date.HasValue ? (object)createCompanyComplianceModels.effective_from_date.Value : DBNull.Value);
                                mycommand.Parameters.AddWithValue("@EffectiveToDate", createCompanyComplianceModels.effective_to_date.HasValue ? (object)createCompanyComplianceModels.effective_to_date.Value : DBNull.Value);
                                mycommand.Parameters.AddWithValue("@CreatedBy", createCompanyComplianceModels.created_by);
                                mycommand.Parameters.AddWithValue("@UpdatedDate", System.DateTime.Now);
                                mycommand.Parameters.AddWithValue("@updated_by", createCompanyComplianceModels.created_by);
                                mycommand.Parameters.AddWithValue("@Status", "Active");
                                mycommand.Parameters.AddWithValue("@SubscriptionType", "free");
                                mycommand.Parameters.AddWithValue("@MappingStatus", "Not Mapped");
                                mycommand.Parameters.AddWithValue("@IsImportedData", "No");
                                mycommand.Parameters.AddWithValue("@version_no", "1.0");
                                //mycommand.Parameters.AddWithValue("@compliance_stage_progress", "Active");
                                //mycommand.Parameters.AddWithValue("@compliance_status", "Active");
                                mycommand.Parameters.AddWithValue("@loop_current_periodicity_factor", createCompanyComplianceModels.loop_current_periodicity_factor);
                                mycommand.Parameters.AddWithValue("@loop_next_periodicity_factor", createCompanyComplianceModels.loop_next_periodicity_factor);
                                mycommand.Parameters.AddWithValue("@loop_day_month_periodicity_factor", createCompanyComplianceModels.loop_day_month_periodicity_factor);

                                int insertedcreatedCompanyId = Convert.ToInt32(await mycommand.ExecuteScalarAsync());


                                // primary key start from 0 in company compliance 
                                var Maxcompanycompliancesidsecond = this.mySqlDBContext.CompanyComplianceSchedulerModels.Where(d => d.IsImportedData == "No").Max(d => (int?)d.company_compliance_sheduler_id) ?? 0;

                                var newcompanycomplianceschedulerid = Maxcompanycompliancesidsecond + 1;
                                // Insert into company_compliance_scheduler_master table
                                string insertCompanyComplianceSchedulerQuery = @"
                    INSERT INTO company_compliance_scheduler_master 
                    (create_company_compliance_id, currentcompliancePeriod, nextCompliancePeriod, 
                    dueDateDay, dueDateMonth, current_periodicity_factor, day_month_periodicity_factor, next_periodicity_factor, extendedDueDateDay, extendedDueDateMonth, extendstatus, IsImportedData)
                    VALUES
                    (@CreateCompanyComplianceId, @CurrentCompliancePeriod, @NextCompliancePeriod, 
                    @DueDateDay, @DueDateMonth, @current_periodicity_factor, @day_month_periodicity_factor, @next_periodicity_factor, @ExtendedDueDateDay, @ExtendedDueDateMonth, @ExtendStatus, @IsImportedData);";

                                foreach (var instanceData in createCompanyComplianceModels.instancesData)
                                {
                                    using (MySqlCommand insertCompanyComplianceSchedulerCommand = new MySqlCommand(insertCompanyComplianceSchedulerQuery, con))
                                    {
                                        // Parameters setting code here
                                        //insertCompanyComplianceSchedulerCommand.Parameters.AddWithValue("@company_compliance_sheduler_id", newcompanycomplianceschedulerid);current_periodicity_factor day_month_periodicity_factor  next_periodicity_factor
                                        insertCompanyComplianceSchedulerCommand.Parameters.AddWithValue("@CreateCompanyComplianceId", newcompanycomplianceid);
                                        insertCompanyComplianceSchedulerCommand.Parameters.AddWithValue("@CurrentCompliancePeriod", instanceData.currentcompliancePeriod);
                                        insertCompanyComplianceSchedulerCommand.Parameters.AddWithValue("@NextCompliancePeriod", instanceData.nextCompliancePeriod);
                                        insertCompanyComplianceSchedulerCommand.Parameters.AddWithValue("@DueDateDay", instanceData.dueDateDay);
                                        insertCompanyComplianceSchedulerCommand.Parameters.AddWithValue("@DueDateMonth", instanceData.dueDateMonth);
                                        insertCompanyComplianceSchedulerCommand.Parameters.AddWithValue("@current_periodicity_factor", instanceData.current_periodicity_factor);
                                        insertCompanyComplianceSchedulerCommand.Parameters.AddWithValue("@day_month_periodicity_factor", instanceData.day_month_periodicity_factor);
                                        insertCompanyComplianceSchedulerCommand.Parameters.AddWithValue("@next_periodicity_factor", instanceData.next_periodicity_factor);
                                        insertCompanyComplianceSchedulerCommand.Parameters.AddWithValue("@ExtendedDueDateDay", instanceData.extendedDueDateDay);
                                        insertCompanyComplianceSchedulerCommand.Parameters.AddWithValue("@ExtendedDueDateMonth", instanceData.extendedDueDateMonth);
                                        insertCompanyComplianceSchedulerCommand.Parameters.AddWithValue("@ExtendStatus", instanceData.extendstatus);
                                        insertCompanyComplianceSchedulerCommand.Parameters.AddWithValue("@IsImportedData", "No");

                                        await insertCompanyComplianceSchedulerCommand.ExecuteNonQueryAsync();
                                    }
                                }
                            }
                            await transaction.CommitAsync();
                            //using (var scope = _serviceprovider.createscope())
                            //{
                            //    var controller = scope.serviceprovider.getrequiredservice<batchcompliancegeneration>();
                            //    controller.processcompliancedata();
                            //}
                            return Ok(new { statusCode = 200, id = generated_id });


                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            //throw;
                            return StatusCode(500, $"An error occurred: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        //[Route("api/createcompanycompliance/InsertGlobalCompliance")]
        //[HttpPost]
        //public IActionResult CreateRole([FromBody] CreateCompanyComplianceModel createCompanyComplianceModels)
        //{

        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }
        //    var createcompanycompliance = this.mySqlDBContext.CreateCompanyComplianceModels;
        //    {
        //        createcompanycompliance.Add(createCompanyComplianceModels);
        //        createCompanyComplianceModels.company_compliance_id = GenerateGlobalActId(createCompanyComplianceModels.rule_id);
        //        DateTime dt = DateTime.Now;
        //        createCompanyComplianceModels.created_date = dt;
        //        createCompanyComplianceModels.status = "Active";
        //        createCompanyComplianceModels.mapping_status = "Not Mapped";
        //        mySqlDBContext.SaveChanges();

        //        // Retrieve the generated create_company_compliance_id
        //        int createdCompanyId = createCompanyComplianceModels.create_company_compliance_id;

        //        foreach (var instanceData in createCompanyComplianceModels.instancesData)
        //        {
        //            var companyComplianceScheduler = new CompanyComplianceScheduler
        //            {
        //                create_company_compliance_id = createdCompanyId,
        //                currentcompliancePeriod = instanceData.currentcompliancePeriod,
        //                nextCompliancePeriod = instanceData.nextCompliancePeriod,
        //                dueDateDay = instanceData.dueDateDay,
        //                dueDateMonth = instanceData.dueDateMonth,
        //                extendedDueDateDay = instanceData.extendedDueDateDay,
        //                extendedDueDateMonth = instanceData.extendedDueDateMonth,
        //                extendstatus = instanceData.extendstatus
        //            };

        //            // Add the new CompanyComplianceScheduler object to the context and save changes
        //            this.mySqlDBContext.CompanyComplianceSchedulerModels.Add(companyComplianceScheduler);
        //            this.mySqlDBContext.SaveChanges();
        //        }

        //        return Ok(); // Return a success response
        //    }

        //}
        private string GenerateCompanyComplianceId(int? company_act_rule_id)
        {

            string newCompanyComplianceId = ""; // Initialize outside the block

            var act_rule_company_ids = this.mySqlDBContext.Rulesandregulatorymodels
                .Where(x => x.act_rule_regulatory_id == company_act_rule_id)
                .Select(x => x.global_rule_id)
                .ToList();

            if (act_rule_company_ids.Count > 0)
            {
                // Use the first company_rule_id from the list
                string id = act_rule_company_ids.First();

                // Extract act_id and rule_id from the provided id
                string actId = id.Substring(2, 4);
                string ruleId = id.Substring(7, 3);

                // Retrieve existing company_rule_id values with the same prefix
                var existingCompanyACTRuleIds = this.mySqlDBContext.CreateCompanyComplianceModels
                    .Where(x => x.company_compliance_id.StartsWith($"CC{actId}.{ruleId}"))
                    .Select(x => x.company_compliance_id)
                    .ToList();

                // If there are existing company_rule_id values, find the maximum sequence and increment it by 1
                int maxSequence = existingCompanyACTRuleIds
                    .Select(x => int.Parse(x.Substring(x.Length - 4)))
                    .DefaultIfEmpty(0)
                    .Max();
                int newSequence = maxSequence + 1;

                // Generate the new company_rule_id
                newCompanyComplianceId = $"CC{actId}.{ruleId}.{newSequence:D4}";
            }
            else
            {
                // If no company_rule_id is found, handle accordingly
                // For example, you could generate a default value or throw an exception
            }

            return newCompanyComplianceId;

        }

    }
}
