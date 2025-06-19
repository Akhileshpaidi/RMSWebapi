using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using MySqlConnector;

namespace ITRTelemetry.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    [Produces("application/json")]
    public class ComplianceRiskClassificationController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public ComplianceRiskClassificationController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/Complianceriskclassification/GetComplianceriskclassificationDetails")]
        [HttpGet]

        public IEnumerable<ComplianceRiskClassificationModel> GetComplianceNotifiedStatus()
        {
            return this.mySqlDBContext.ComplianceRiskClassificationModels.Where(x => x.compliance_risk_classification_status == "Active").ToList();
        }



        [Route("api/Complianceriskclassification/InsertComplianceriskclassificationDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] ComplianceRiskClassificationModel ComplianceRiskClassificationModels)
        {
            try
            {
                ComplianceRiskClassificationModels.compliance_risk_classification_name = ComplianceRiskClassificationModels.compliance_risk_classification_name.Trim();

                var existingComplianceRisk = this.mySqlDBContext.ComplianceRiskClassificationModels
                    .FirstOrDefault(d => d.compliance_risk_classification_name == ComplianceRiskClassificationModels.compliance_risk_classification_name && d.compliance_risk_classification_status == "Active");

                if (existingComplianceRisk != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Compliance Risk Classification with the same name already exists.");
                }

                var maxcomplianceriskId = this.mySqlDBContext.ComplianceRiskClassificationModels
            .Where(d => d.source == "No")
            .Max(d => (int?)d.compliance_risk_classification_id) ?? 0; // If no records are found, default to 0

                // Increment the law_type_id by 1
                ComplianceRiskClassificationModels.compliance_risk_classification_id = maxcomplianceriskId + 1;

                var TypeModel = this.mySqlDBContext.ComplianceRiskClassificationModels;



                TypeModel.Add(ComplianceRiskClassificationModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                ComplianceRiskClassificationModels.compliance_risk_classification_date = dt1;
                ComplianceRiskClassificationModels.compliance_risk_classification_status = "Active";
                ComplianceRiskClassificationModels.source = "No";

                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Compliance Risk Classification with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/Complianceriskclassification/UpdateComplianceriskclassificationDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] ComplianceRiskClassificationModel ComplianceRiskClassificationModels)
        {

            try
            {
                if (ComplianceRiskClassificationModels.compliance_risk_classification_id == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    ComplianceRiskClassificationModels.compliance_risk_classification_name = ComplianceRiskClassificationModels.compliance_risk_classification_name?.Trim();
                    var existingCompliancenotifiedstatus = this.mySqlDBContext.ComplianceRiskClassificationModels
                       .FirstOrDefault(d => d.compliance_risk_classification_name == ComplianceRiskClassificationModels.compliance_risk_classification_name && d.compliance_risk_classification_id != ComplianceRiskClassificationModels.compliance_risk_classification_id && d.compliance_risk_classification_status == "Active");

                    if (existingCompliancenotifiedstatus != null)
                    {
                        return BadRequest("Error: Compliance Risk Classification with the same name already exists.");
                    }
                    this.mySqlDBContext.Attach(ComplianceRiskClassificationModels);
                    this.mySqlDBContext.Entry(ComplianceRiskClassificationModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(ComplianceRiskClassificationModels);

                    Type type = typeof(ComplianceRiskClassificationModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(ComplianceRiskClassificationModels, null) == null || property.GetValue(ComplianceRiskClassificationModels, null).Equals(0))
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
                    return BadRequest("Error: Compliance Risk Classification with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

        [Route("api/Complianceriskclassification/DeleteComplianceriskclassificationDetails")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new ComplianceRiskClassificationModel { compliance_risk_classification_id = id };
            currentClass.compliance_risk_classification_status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("compliance_risk_classification_status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

    }
}
