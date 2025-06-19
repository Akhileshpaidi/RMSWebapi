using DomainModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Http;
using System.Linq;
using MySqlConnector;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using iText.Kernel.Pdf;
using System.Diagnostics;
using MySqlX.XDevAPI.Common;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Threading.Tasks.Dataflow;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]


    public class compliancelocationController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public compliancelocationController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }


        [Route("api/globalcompanycompliance/Getcompanycompliance")]
        [HttpGet]
        public IActionResult Getcompanycompliance([FromQuery] CreateCompanyComplianceModel payload)
        {
            try
            {
                // Start with the base query
                var query = mySqlDBContext.CreateCompanyComplianceModels.AsQueryable();

           
                if (payload.category_of_law_id != null)
                    query = query.Where(e => e.category_of_law_id == payload.category_of_law_id);

                if (payload.law_type_id != null)
                    query = query.Where(e => e.law_type_id == payload.law_type_id);

                if (payload.rule_id != null)
                    query = query.Where(e => e.rule_id == payload.rule_id);

                if (payload.regulatory_authority_id != null)
                    query = query.Where(e => e.regulatory_authority_id == payload.regulatory_authority_id);

                if (payload.jursdiction_category_id != null)
                    query = query.Where(e => e.jursdiction_category_id == payload.jursdiction_category_id);

                if (payload.country_id != null)
                    query = query.Where(e => e.country_id == payload.country_id);

                if (payload.state_id != null)
                    query = query.Where(e => e.state_id == payload.state_id);

                if (payload.jursdiction_Location_id != null)
                    query = query.Where(e => e.jursdiction_Location_id == payload.jursdiction_Location_id);

               
                var existingrecord = query.FirstOrDefault();

                bool allPayloadFieldsNull =
          payload.category_of_law_id == null &&
          payload.law_type_id == null &&
          payload.rule_id == null &&
          payload.regulatory_authority_id == null &&
          payload.jursdiction_category_id == null &&
          payload.country_id == null &&
          payload.state_id == null &&
          payload.jursdiction_Location_id == null;

                IQueryable<CreateCompanyComplianceModel> complianceQuery;

                if (existingrecord != null)
                {
                  
                    complianceQuery = query.Where(e => e.status == "Active");
                }
                else if (allPayloadFieldsNull)
                {
                 
                    complianceQuery = mySqlDBContext.CreateCompanyComplianceModels
                                                    .Where(e => e.status == "Active");
                }
                else
                {
                    return Ok(new List<object>());
                }

                var compliance = from companycompliance in complianceQuery
                                 join act_rule_regulatory in mySqlDBContext.Rulesandregulatorymodels on companycompliance.rule_id equals act_rule_regulatory.act_rule_regulatory_id
                                 join act_regulator in mySqlDBContext.Actregulatorymodels on act_rule_regulatory.actregulatoryid equals act_regulator.actregulatoryid
                                 join compliance_type in mySqlDBContext.ComplianceModels on companycompliance.compliance_type_id equals compliance_type.compliance_type_id
                                 join catgoryoflaw in mySqlDBContext.catageoryoflawmodels on companycompliance.category_of_law_id equals catgoryoflaw.category_of_law_ID
                                 join RegulatoryAuthority in mySqlDBContext.RegulatoryAuthorityModels on companycompliance.regulatory_authority_id equals RegulatoryAuthority.regulatory_authority_id
                                 join jurisdictor in mySqlDBContext.Jurisdictionmodels on companycompliance.jursdiction_category_id equals jurisdictor.jurisdiction_category_id
                                 join country in mySqlDBContext.CountryModels on companycompliance.country_id equals country.id into countryGroup
                                 from countrynull in countryGroup.DefaultIfEmpty()
                                 join state in mySqlDBContext.StateModels on companycompliance.state_id equals state.id into stateGroup
                                 from statenull in stateGroup.DefaultIfEmpty()
                                 join district in mySqlDBContext.JurisdictionLocationModels on companycompliance.jursdiction_Location_id equals district.jurisdiction_location_id into districtGroup
                                 from districtnull in districtGroup.DefaultIfEmpty()
                                 join compliancrecordtype in mySqlDBContext.ComplianceRecordTypeModels on companycompliance.compliance_record_type_id equals compliancrecordtype.compliance_record_type_id
                                 join compliancgroup in mySqlDBContext.ComplianceGroupModels on companycompliance.compliance_group_id equals compliancgroup.compliance_group_id
                                 join compliancenotified in mySqlDBContext.ComplianceNotifiedStatusModels on companycompliance.compliance_notified_status_id equals compliancenotified.compliance_notified_id
                                 join compliancefreq in mySqlDBContext.Frequencymodels on companycompliance.frequency_period_id equals compliancefreq.frequencyid
                                 join classificationrisk in mySqlDBContext.ComplianceRiskClassificationCriteriaModels on companycompliance.risk_classification_criteria_id equals classificationrisk.compliance_risk_criteria_id
                                 orderby companycompliance.create_company_compliance_id ascending
                                 select new
                                 {
                                     companycompliance.create_company_compliance_id,
                                     companycompliance.company_compliance_id,
                                     companycompliance.compliance_name,
                                     act_rule_regulatory.act_rule_regulatory_id,
                                     act_rule_regulatory.act_rule_name,
                                     act_regulator.actregulatoryname,
                                     companycompliance.subscription_type,
                                     companycompliance.mapping_status,
                                     companycompliance.compliance_description,
                                     compliance_type = compliance_type.compliance_type_name,
                                     keywors_tags = companycompliance.key_words_tags,
                                     compliance_type_id = companycompliance.compliance_type_id,
                                     create_compliance_id = companycompliance.create_company_compliance_id,
                                     law_Categoryname = catgoryoflaw.law_Categoryname,
                                     regulatory_authority_name = RegulatoryAuthority.regulatory_authority_name,
                                     country = (countrynull != null) ? countrynull.name : "N/A",
                                     state = (statenull != null) ? statenull.name : "N/A",
                                     JurisdictionLocationDistrict = (districtnull != null) ? districtnull.jurisdiction_district : "N/A",
                                     compliance_record_name = compliancrecordtype.compliance_record_name,
                                     compliance_group_name = compliancgroup.compliance_group_name,
                                     compliance_notified_name = compliancenotified.compliance_notified_name,
                                     compliance_risk_criteria_name = classificationrisk.compliance_risk_criteria_name,
                                 };

                var result = compliance.ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }



        [Route("api/globalcompanycompliance/GetglobalcomplianceByID/{ruleid}")]
        [HttpGet]
        public IEnumerable<object> GetglobalcomplianceByID(int ruleid)
        {
            try
            {


                var compliance = from companycompliance in mySqlDBContext.CreateCompanyComplianceModels
                                 join act_rule_regulatory in mySqlDBContext.Rulesandregulatorymodels on companycompliance.rule_id equals act_rule_regulatory.act_rule_regulatory_id
                                 join act_regulator in mySqlDBContext.Actregulatorymodels on act_rule_regulatory.actregulatoryid equals act_regulator.actregulatoryid
                                 where companycompliance.status == "Active" && companycompliance.rule_id == ruleid
                                 select new
                                 {
                                     companycompliance.create_company_compliance_id,
                                     companycompliance.company_compliance_id,
                                     companycompliance.compliance_name,
                                     act_rule_regulatory.act_rule_regulatory_id,
                                     act_rule_regulatory.act_rule_name,
                                     act_regulator.actregulatoryname,
                                     companycompliance.subscription_type,
                                     companycompliance.mapping_status,

                                 };
                var result = compliance.ToList();

                return result;
            }
            catch (Exception ex)
            {
                // Log or handle the exception here
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null; // Or any other appropriate action
            }
        }

        [Route("api/compliancelocation/insertcompliancelocation")]
        [HttpPost]

        public IActionResult insertcompliancelocation([FromBody] compliancelocationmappingmodel compliancelocationmappingmodels)
        {

            var locationdepartmentmappingIdString = compliancelocationmappingmodels.locationdepartmentmappingid.ToString();
            var locationdepartmentmappingid = locationdepartmentmappingIdString.Split(',');
            var totalCount = this.mySqlDBContext.compliancelocationmappingmodels.Count();
            bool combinationExists = false;

            foreach (var locationdepartment in locationdepartmentmappingid)
            {
                var compliancesid = compliancelocationmappingmodels.companycomplianceid.ToString();
                var companycomplianceid = compliancesid.Split(',');

                foreach (var compliance in companycomplianceid)
                {
                    var existingrecord = this.mySqlDBContext.compliancelocationmappingmodels.FirstOrDefault(
                           c => c.companycomplianceid == compliance &&
                            c.locationdepartmentmappingid == locationdepartment && c.status == "Mapped");

                    if (existingrecord == null)
                    {
                        totalCount++;
                        var uniqueDefaultKey = GenerateDefaultKey(totalCount);

                        var compliancelocationmappingmodel = new compliancelocationmappingmodel
                        {
                            locationdepartmentmappingid = locationdepartment,
                            companycomplianceid = compliance,
                            createdby = compliancelocationmappingmodels.createdby,
                            createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            status = "Mapped",
                            companycompliancemappingid = uniqueDefaultKey
                        };
                        this.mySqlDBContext.compliancelocationmappingmodels.Add(compliancelocationmappingmodel);
                    }
                    else
                    {
                        Console.WriteLine("Error: Record already exists.");
                    }
                }
            }

            this.mySqlDBContext.SaveChanges();

            return Ok();

        }


        private string GenerateDefaultKey(int currentCount)
        {
            // var currentCount = mySqlDBContext.ActivityWorkgroupModels.Count() + 1;

            var mappingId = currentCount.ToString("000");
            //return $"CC.00.000.00.{mappingId}";
            return $"CCM-{mappingId}";
        }



        [Route("api/compliancelocation/Getcompliancelocation")]
        [HttpGet]

        public IEnumerable<object> Getcompliancelocation()
        {
            var compliancedepartment = (from compliancelocation in mySqlDBContext.compliancelocationmappingmodels
                                        join companycompliance in mySqlDBContext.CreateCompanyComplianceModels on compliancelocation.companycomplianceid equals companycompliance.create_company_compliance_id.ToString()
                                        join actregulatory in mySqlDBContext.Actregulatorymodels on companycompliance.act_id equals actregulatory.actregulatoryid
                                        join actrulerepo in mySqlDBContext.Rulesandregulatorymodels on companycompliance.rule_id equals actrulerepo.act_rule_regulatory_id
                                        join departmentlocation in mySqlDBContext.Departmentlocationmappingmodels on compliancelocation.locationdepartmentmappingid equals (departmentlocation.locationdepartmentmappingid).ToString()
                                        join unitlocation in mySqlDBContext.UnitLocationMasterModels on departmentlocation.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
                                        join departmentmaster in mySqlDBContext.DepartmentModels on departmentlocation.departmentid equals departmentmaster.Department_Master_id.ToString()
                                        join entitymaster in mySqlDBContext.UnitMasterModels on departmentlocation.entityid equals entitymaster.Entity_Master_id
                                        join compliance_type in mySqlDBContext.ComplianceModels on companycompliance.compliance_type_id equals compliance_type.compliance_type_id
                                        join catgoryoflaw in mySqlDBContext.catageoryoflawmodels on companycompliance.category_of_law_id equals catgoryoflaw.category_of_law_ID
                                        join RegulatoryAuthority in mySqlDBContext.RegulatoryAuthorityModels on companycompliance.regulatory_authority_id equals RegulatoryAuthority.regulatory_authority_id
                                        join jurisdictor in mySqlDBContext.Jurisdictionmodels on companycompliance.jursdiction_category_id equals jurisdictor.jurisdiction_category_id
                                        join country in mySqlDBContext.CountryModels on companycompliance.country_id equals country.id into countryGroup
                                        from countrynull in countryGroup.DefaultIfEmpty()
                                        join state in mySqlDBContext.StateModels on companycompliance.state_id equals state.id into stateGroup
                                        from statenull in stateGroup.DefaultIfEmpty()
                                        join district in mySqlDBContext.JurisdictionLocationModels on companycompliance.jursdiction_Location_id equals district.jurisdiction_location_id into districtGroup
                                        from districtnull in districtGroup.DefaultIfEmpty()
                                        join compliancrecordtype in mySqlDBContext.ComplianceRecordTypeModels on companycompliance.compliance_record_type_id equals compliancrecordtype.compliance_record_type_id
                                        join compliancgroup in mySqlDBContext.ComplianceGroupModels on companycompliance.compliance_group_id equals compliancgroup.compliance_group_id
                                        join compliancenotified in mySqlDBContext.ComplianceNotifiedStatusModels on companycompliance.compliance_notified_status_id equals compliancenotified.compliance_notified_id
                                        join compliancefreq in mySqlDBContext.Frequencymodels on companycompliance.frequency_period_id equals compliancefreq.frequencyid
                                        join classificationrisk in mySqlDBContext.ComplianceRiskClassificationCriteriaModels on companycompliance.risk_classification_criteria_id equals classificationrisk.compliance_risk_criteria_id
                                        where compliancelocation.status == "Mapped"
                                        orderby compliancelocation.compliance_location_Mapping_id ascending

                                        select new
                                        {
                                            compliancelocation.compliance_location_Mapping_id,
                                            compliancelocation.companycomplianceid,
                                            companycompliance.company_compliance_id,
                                            companycompliance.compliance_name,
                                            entitymaster.Entity_Master_Name,
                                            unitlocation.Unit_location_Master_name,
                                            departmentmaster.Department_Master_name,
                                        actregulatory.actregulatoryname,
                                           actrulerepo.act_rule_name,
                                            compliancelocation.status,
                                            compliancelocation.companycompliancemappingid,
                                            companycompliance.compliance_description,
                                            compliance_type = compliance_type.compliance_type_name,
                                            keywors_tags = companycompliance.key_words_tags,
                                            compliance_type_id = companycompliance.compliance_type_id,
                                            create_compliance_id = companycompliance.create_company_compliance_id,
                                            law_Categoryname = catgoryoflaw.law_Categoryname,
                                            regulatory_authority_name = RegulatoryAuthority.regulatory_authority_name,
                                            country = (countrynull != null) ? countrynull.name : "N/A",
                                            state = (statenull != null) ? statenull.name : "N/A",
                                            JurisdictionLocationDistrict = (districtnull != null) ? districtnull.jurisdiction_district : "N/A",
                                            compliance_record_name = compliancrecordtype.compliance_record_name,
                                            compliance_group_name = compliancgroup.compliance_group_name,
                                            compliance_notified_name = compliancenotified.compliance_notified_name,
                                            compliance_risk_criteria_name = classificationrisk.compliance_risk_criteria_name,


                                        })
                                        .ToList();

               return compliancedepartment;
        }

        [Route("api/compliancelocation/deletecompliancelocationDetails")]
        [HttpDelete]

        public void deletecompliancelocationDetails(int id)
        {
            var currentClass = new compliancelocationmappingmodel { compliance_location_Mapping_id = id };
            currentClass.status = "UnMapped";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }
        [Route("api/compliancedepartmentmapping/GetcompliancedepartmentmappingDetailsbyid/{compliance_location_Mapping_id}")]
        [HttpGet]

        public IEnumerable<object> GetcompliancedepartmentmappingDetailsbyid(int compliance_location_Mapping_id)
        {
            var details = (from compliancelocation in mySqlDBContext.compliancelocationmappingmodels
                           join companycompliance in mySqlDBContext.CreateCompanyComplianceModels on compliancelocation.companycomplianceid equals companycompliance.create_company_compliance_id.ToString()
                           join departmentmapping in mySqlDBContext.Departmentlocationmappingmodels on compliancelocation.locationdepartmentmappingid equals departmentmapping.locationdepartmentmappingid.ToString()
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on departmentmapping.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
                           join departmentmaster in mySqlDBContext.DepartmentModels on departmentmapping.departmentid equals departmentmaster.Department_Master_id.ToString()
                           join entitymaster in mySqlDBContext.UnitMasterModels on departmentmapping.entityid equals entitymaster.Entity_Master_id
                           where compliancelocation.status == "Mapped" && compliancelocation.compliance_location_Mapping_id == compliance_location_Mapping_id
                           select new
                           {
                               compliancelocation.compliance_location_Mapping_id,
                               compliancelocation.companycomplianceid,
                               compliancelocation.companycompliancemappingid,
                               companycompliance.compliance_name,
                               departmentmapping.locationdepartmentmappingid,
                               departmentmapping.entityid,
                               entitymaster.Entity_Master_Name,
                               departmentmapping.unitlocationid,
                               unitlocation.Unit_location_Master_name,
                               departmentmapping.departmentid,
                               departmentmaster.Department_Master_id,
                               departmentmaster.Department_Master_name,
                               departmentname = $"{departmentmaster.Department_Master_name}<{unitlocation.Unit_location_Master_name}><{entitymaster.Entity_Master_Name}>"
                           })
                            .Distinct()
                .ToList();

            return details;
        }

        [Route("api/locationcompliancemapping/UpdatelocationcompliancemappingDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] UpdateData7 updateData)
        {
            try
            {

                compliancelocationmappingmodel compliancelocationmappingmodels = updateData.compliancelocationmappingmodels;

                bool combinationExists = this.mySqlDBContext.compliancelocationmappingmodels
         .Any(d => d.companycomplianceid == compliancelocationmappingmodels.companycomplianceid &&
                   d.locationdepartmentmappingid == compliancelocationmappingmodels.locationdepartmentmappingid
                  );

                if (combinationExists)
                {
                    return BadRequest("Error: Record already exists with the same combination of same compliance and Department.");
                }
                else
                {

                    if (compliancelocationmappingmodels.compliance_location_Mapping_id == 0)
                    {
                        return Ok("Insertion successful");
                    }
                    else
                    {
                        compliancelocationmappingmodels.createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        compliancelocationmappingmodels.status = "Mapped";
                        // Existing department, update logic
                        this.mySqlDBContext.Attach(compliancelocationmappingmodels);
                        this.mySqlDBContext.Entry(compliancelocationmappingmodels).State = EntityState.Modified;
                        this.mySqlDBContext.SaveChanges();
                        return Ok("Update successful");
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Combination already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }


            }

        
    
}

    }
}
