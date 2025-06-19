using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using System.Threading.Channels;
using MySqlConnector;

namespace ITRTelemetry.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    [Produces("application/json")]
    public class ComplianceRiskClassificationCriteriaController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public ComplianceRiskClassificationCriteriaController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        [Route("api/Complianceriskcriteria/GetComplianceriskcriteriaDetails")]
        [HttpGet]

        public IEnumerable<ComplianceRiskClassificationCriteriaModel> GetComplianceNotifiedStatus()
        {
            return this.mySqlDBContext.ComplianceRiskClassificationCriteriaModels.Where(x => x.compliance_risk_criteria_status == "Active").ToList();
        }



        [Route("api/Complianceriskcriteria/InsertComplianceriskcriteriaDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] ComplianceRiskClassificationCriteriaModel ComplianceRiskClassificationCriteriaModels)
        {
            try
            {
                ComplianceRiskClassificationCriteriaModels.compliance_risk_criteria_name = ComplianceRiskClassificationCriteriaModels.compliance_risk_criteria_name.Trim();

                var ComplianceRiskClassificationCriteria = this.mySqlDBContext.ComplianceRiskClassificationCriteriaModels
                    .FirstOrDefault(d => d.compliance_risk_criteria_name == ComplianceRiskClassificationCriteriaModels.compliance_risk_criteria_name && d.compliance_risk_criteria_status == "Active");

                if (ComplianceRiskClassificationCriteria != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Compliance Risk Classification Criteria with the same name already exists.");
                }
                var maxcomplianceriskId = this.mySqlDBContext.ComplianceRiskClassificationCriteriaModels
        .Where(d => d.IsImportedData == "No")
        .Max(d => (int?)d.compliance_risk_criteria_id) ?? 0; // If no records are found, default to 0

                // Increment the law_type_id by 1
                ComplianceRiskClassificationCriteriaModels.compliance_risk_criteria_id = maxcomplianceriskId + 1;

                var TypeModel = this.mySqlDBContext.ComplianceRiskClassificationCriteriaModels;

            
                TypeModel.Add(ComplianceRiskClassificationCriteriaModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                ComplianceRiskClassificationCriteriaModels.compliance_risk_criteria_date = dt1;
                ComplianceRiskClassificationCriteriaModels.compliance_risk_criteria_status = "Active";
                ComplianceRiskClassificationCriteriaModels.IsImportedData = "No";
                this.mySqlDBContext.SaveChanges();
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

        [Route("api/Complianceriskcriteria/UpdateComplianceriskcriteriaDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] ComplianceRiskClassificationCriteriaModel ComplianceRiskClassificationCriteriaModels)
        {

            try
            {
                if (ComplianceRiskClassificationCriteriaModels.compliance_risk_criteria_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    ComplianceRiskClassificationCriteriaModels.compliance_risk_criteria_name = ComplianceRiskClassificationCriteriaModels.compliance_risk_criteria_name.Trim();
                    var existingCompliancenotifiedstatus = this.mySqlDBContext.ComplianceRiskClassificationCriteriaModels
                       .FirstOrDefault(d => d.compliance_risk_criteria_name == ComplianceRiskClassificationCriteriaModels.compliance_risk_criteria_name && d.compliance_risk_criteria_id != ComplianceRiskClassificationCriteriaModels.compliance_risk_criteria_id && d.compliance_risk_criteria_status == "Active");

                    if (existingCompliancenotifiedstatus != null)
                    {
                        return BadRequest("Error: Compliance Risk Classification Criteria with the same name already exists.");
                    }
                    this.mySqlDBContext.Attach(ComplianceRiskClassificationCriteriaModels);
                    this.mySqlDBContext.Entry(ComplianceRiskClassificationCriteriaModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(ComplianceRiskClassificationCriteriaModels);

                    Type type = typeof(ComplianceRiskClassificationCriteriaModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(ComplianceRiskClassificationCriteriaModels, null) == null || property.GetValue(ComplianceRiskClassificationCriteriaModels, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
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

        [Route("api/Complianceriskcriteria/DeleteComplianceriskcriteriaDetails")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new ComplianceRiskClassificationCriteriaModel { compliance_risk_criteria_id = id };
            currentClass.compliance_risk_criteria_status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("compliance_risk_criteria_status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

    }
}


