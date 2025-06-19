using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class SupAdmin_ComplianceRiskClassificationController : Controller
    {
        private CommonDBContext commonDBContext;
        public IConfiguration Configuration { get; }

        public SupAdmin_ComplianceRiskClassificationController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }
        [Route("api/SupAdmin_Complianceriskclassification/GetCompliancerisk")]
        [HttpGet]

        public IEnumerable<SupAdmin_ComplianceRiskClassificationModel> GetComplianceNotifiedStatus()
        {
            return this.commonDBContext.SupAdmin_ComplianceRiskClassificationModels.Where(x => x.compliance_risk_classification_status == "Active").ToList();
        }



        [Route("api/SupAdmin_Complianceriskclassification/InsertCompliance")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] SupAdmin_ComplianceRiskClassificationModel SupAdmin_ComplianceRiskClassificationModels)
        {
            try
            {
                SupAdmin_ComplianceRiskClassificationModels.compliance_risk_classification_name = SupAdmin_ComplianceRiskClassificationModels.compliance_risk_classification_name.Trim();

                var existingComplianceRisk = this.commonDBContext.SupAdmin_ComplianceRiskClassificationModels
                    .FirstOrDefault(d => d.compliance_risk_classification_name == SupAdmin_ComplianceRiskClassificationModels.compliance_risk_classification_name && d.compliance_risk_classification_status == "Active");

                if (existingComplianceRisk != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Compliance Risk Classification with the same name already exists.");
                }

                var Maxriskclasfid = this.commonDBContext.SupAdmin_ComplianceRiskClassificationModels.Max(d => (int?)d.compliance_risk_classification_id) ?? 5000;

                SupAdmin_ComplianceRiskClassificationModels.compliance_risk_classification_id = Maxriskclasfid + 1;

                var TypeModel = this.commonDBContext.SupAdmin_ComplianceRiskClassificationModels;
                TypeModel.Add(SupAdmin_ComplianceRiskClassificationModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                SupAdmin_ComplianceRiskClassificationModels.compliance_risk_classification_date = dt1;
                SupAdmin_ComplianceRiskClassificationModels.compliance_risk_classification_status = "Active";
                this.commonDBContext.SaveChanges();
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

        [Route("api/SupAdmin_Complianceriskclassification/UpdateCompliance")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] SupAdmin_ComplianceRiskClassificationModel SupAdmin_ComplianceRiskClassificationModels)
        {

            try
            {
                if (SupAdmin_ComplianceRiskClassificationModels.compliance_risk_classification_id == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    SupAdmin_ComplianceRiskClassificationModels.compliance_risk_classification_name = SupAdmin_ComplianceRiskClassificationModels.compliance_risk_classification_name?.Trim();
                    var existingCompliancenotifiedstatus = this.commonDBContext.SupAdmin_ComplianceRiskClassificationModels
                       .FirstOrDefault(d => d.compliance_risk_classification_name == SupAdmin_ComplianceRiskClassificationModels.compliance_risk_classification_name && d.compliance_risk_classification_id != SupAdmin_ComplianceRiskClassificationModels.compliance_risk_classification_id && d.compliance_risk_classification_status == "Active");

                    if (existingCompliancenotifiedstatus != null)
                    {
                        return BadRequest("Error: Compliance Risk Classification with the same name already exists.");
                    }
                    this.commonDBContext.Attach(SupAdmin_ComplianceRiskClassificationModels);
                    this.commonDBContext.Entry(SupAdmin_ComplianceRiskClassificationModels).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(SupAdmin_ComplianceRiskClassificationModels);

                    Type type = typeof(SupAdmin_ComplianceRiskClassificationModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(SupAdmin_ComplianceRiskClassificationModels, null) == null || property.GetValue(SupAdmin_ComplianceRiskClassificationModels, null).Equals(0))
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
                    return BadRequest("Error: Compliance Risk Classification with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

        [Route("api/SupAdmin_Complianceriskclassification/DeleteCompliance")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new SupAdmin_ComplianceRiskClassificationModel { compliance_risk_classification_id = id };
            currentClass.compliance_risk_classification_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("compliance_risk_classification_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }

    }
}
