using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class ComplianceController : ControllerBase
    {
        private MySqlDBContext mySqlDBContext;

        public ComplianceController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/Compliance/GetComplianceDetails")]
        [HttpGet]

        public IEnumerable<ComplianceModel> GetComplianceDetails()
        {
            return this.mySqlDBContext.ComplianceModels.Where(x => x.compliance_type_status == "Active").ToList();
        }

        [Route("api/Compliance/GetCompliancetypeDetails")]
        [HttpGet]

        public IEnumerable<object> GetCompliancetypeDetails()
        {
            var details = (from compliancetype in mySqlDBContext.ComplianceModels
                           where compliancetype.compliance_type_status == "Active"
                           select new
                           {
                               compliancetype.compliance_type_id,
                               compliancetype.compliance_type_name
                           })
                           .ToList();
            return details;
        }


        [Route("api/Compliance/InsertComplianceDetails")]
        [HttpPost]

        public IActionResult InsertParameter([FromBody] ComplianceModel ComplianceModels)
        {

            try
            {
                ComplianceModels.compliance_type_name = ComplianceModels.compliance_type_name.Trim();

                var existingPenaltyCategory = this.mySqlDBContext.ComplianceModels
                    .FirstOrDefault(d => d.compliance_type_name == ComplianceModels.compliance_type_name && d.compliance_type_status == "Active");

                if (existingPenaltyCategory != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Penalty Category with the same name already exists.");
                }
                var maxcomplianceId = this.mySqlDBContext.ComplianceModels
             .Where(d => d.source == "No")
             .Max(d => (int?)d.compliance_type_id) ?? 0; // If no records are found, default to 0

                // Increment the law_type_id by 1
                ComplianceModels.compliance_type_id = maxcomplianceId + 1;

                var TypeModel = this.mySqlDBContext.ComplianceModels;

              
                TypeModel.Add(ComplianceModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                ComplianceModels.compliance_type_create_date = dt1;
                ComplianceModels.compliance_type_status = "Active";
                ComplianceModels.source = "No";

                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Penalty Category with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/Compliance/UpdateComplianceDetails")]
        [HttpPut]

        public IActionResult UpdateComplianceDetails([FromBody] ComplianceModel ComplianceModels)
        {
            try
            {
                if (ComplianceModels.compliance_type_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    ComplianceModels.compliance_type_name = ComplianceModels.compliance_type_name?.Trim();
                    var existingCompliancenotifiedstatus = this.mySqlDBContext.ComplianceModels
                       .FirstOrDefault(d => d.compliance_type_name == ComplianceModels.compliance_type_name && d.compliance_type_id != ComplianceModels.compliance_type_id && d.compliance_type_status == "Active");

                    if (existingCompliancenotifiedstatus != null)
                    {
                        return BadRequest("Error: Penalty Category with the same name already exists.");
                    }
                    this.mySqlDBContext.Attach(ComplianceModels);
                    this.mySqlDBContext.Entry(ComplianceModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(ComplianceModels);

                    Type type = typeof(ComplianceModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(ComplianceModels, null) == null || property.GetValue(ComplianceModels, null).Equals(0))
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
                    return BadRequest("Error: Penalty Category with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/Compliance/DeleteComplianceDetails")]
        [HttpDelete]

        public void DeleteComplianceDetails(int id)
        {
            var currentClass = new ComplianceModel { compliance_type_id = id };
            currentClass.compliance_type_status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("compliance_type_status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }
    }
}
