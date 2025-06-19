using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySQLProvider;
using DomainModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Threading.Tasks;
using System.Linq;
using MySqlConnector;
using Microsoft.Extensions.Configuration;
using System.Configuration;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class SupAdmin_ComplianceRiskCriteriaController : Controller
    {
        private CommonDBContext commonDBContext;
        public IConfiguration Configuration { get; }

        public SupAdmin_ComplianceRiskCriteriaController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }
        [Route("api/SupAdmin_riskcriteria/GetComplianceriskcriteriaDetails")]
        [HttpGet]

        public IEnumerable<SupAdmin_ComplianceRiskCriteriaModel> GetComplianceNotifiedStatus()
        {
            return this.commonDBContext.SupAdmin_ComplianceRiskCriteriaModels.Where(x => x.compliance_risk_criteria_status == "Active").ToList();
        }



        [Route("api/SupAdmin_riskcriteria/InsertComplianceriskcriteriaDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] SupAdmin_ComplianceRiskCriteriaModel SupAdmin_ComplianceRiskCriteriaModels)
        {
            try
            {
                SupAdmin_ComplianceRiskCriteriaModels.compliance_risk_criteria_name = SupAdmin_ComplianceRiskCriteriaModels.compliance_risk_criteria_name.Trim();

                var ComplianceRiskClassificationCriteria = this.commonDBContext.SupAdmin_ComplianceRiskCriteriaModels
                    .FirstOrDefault(d => d.compliance_risk_criteria_name == SupAdmin_ComplianceRiskCriteriaModels.compliance_risk_criteria_name && d.compliance_risk_criteria_status == "Active");

                if (ComplianceRiskClassificationCriteria != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Compliance Risk Classification Criteria with the same name already exists.");
                }


                var Maxriskcriteriaid = this.commonDBContext.SupAdmin_ComplianceRiskCriteriaModels.Max(d => (int?)d.compliance_risk_criteria_id) ?? 5000;

                SupAdmin_ComplianceRiskCriteriaModels.compliance_risk_criteria_id = Maxriskcriteriaid + 1;

                var TypeModel = this.commonDBContext.SupAdmin_ComplianceRiskCriteriaModels;
                TypeModel.Add(SupAdmin_ComplianceRiskCriteriaModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                SupAdmin_ComplianceRiskCriteriaModels.compliance_risk_criteria_date = dt1;
                SupAdmin_ComplianceRiskCriteriaModels.compliance_risk_criteria_status = "Active";
                this.commonDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Compliance Risk Classification Criteria with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/SupAdmin_riskcriteria/UpdateComplianceriskcriteriaDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] SupAdmin_ComplianceRiskCriteriaModel SupAdmin_ComplianceRiskCriteriaModels)
        {

            try
            {
                if (SupAdmin_ComplianceRiskCriteriaModels.compliance_risk_criteria_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    SupAdmin_ComplianceRiskCriteriaModels.compliance_risk_criteria_name = SupAdmin_ComplianceRiskCriteriaModels.compliance_risk_criteria_name?.Trim();
                    var existingCompliancenotifiedstatus = this.commonDBContext.SupAdmin_ComplianceRiskCriteriaModels
                       .FirstOrDefault(d => d.compliance_risk_criteria_name == SupAdmin_ComplianceRiskCriteriaModels.compliance_risk_criteria_name && d.compliance_risk_criteria_id != SupAdmin_ComplianceRiskCriteriaModels.compliance_risk_criteria_id && d.compliance_risk_criteria_status == "Active");

                    if (existingCompliancenotifiedstatus != null)
                    {
                        return BadRequest("Error: Compliance Risk Classification Criteria with the same name already exists.");
                    }
                    this.commonDBContext.Attach(SupAdmin_ComplianceRiskCriteriaModels);
                    this.commonDBContext.Entry(SupAdmin_ComplianceRiskCriteriaModels).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(SupAdmin_ComplianceRiskCriteriaModels);

                    Type type = typeof(SupAdmin_ComplianceRiskCriteriaModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(SupAdmin_ComplianceRiskCriteriaModels, null) == null || property.GetValue(SupAdmin_ComplianceRiskCriteriaModels, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.commonDBContext.SaveChanges();
                    // You may want to return some success response
                    return Ok("Update successful");
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Compliance Risk Classification Criteriay with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

        [Route("api/SupAdmin_riskcriteria/DeleteComplianceriskcriteriaDetails")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new SupAdmin_ComplianceRiskCriteriaModel { compliance_risk_criteria_id = id };
            currentClass.compliance_risk_criteria_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("compliance_risk_criteria_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }
    }
}
