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
    public class ComplianceNotifiedStatusController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public ComplianceNotifiedStatusController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/compliancenotifiedstatus/GetcompliancenotifiedstatusDetails")]
        [HttpGet]

        public IEnumerable<ComplianceNotifiedStatusModel> GetComplianceNotifiedStatus()
        {
            return this.mySqlDBContext.ComplianceNotifiedStatusModels.Where(x => x.compliance_notified_status == "Active").ToList();
        }



        [Route("api/compliancenotifiedstatus/InsertcompliancenotifiedstatusDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] ComplianceNotifiedStatusModel ComplianceNotifiedStatusModels)
        {
            try
            {
                ComplianceNotifiedStatusModels.compliance_notified_name = ComplianceNotifiedStatusModels.compliance_notified_name.Trim();

                var existingCompliancenotifiedstatus = this.mySqlDBContext.ComplianceNotifiedStatusModels
                    .FirstOrDefault(d => d.compliance_notified_name == ComplianceNotifiedStatusModels.compliance_notified_name && d.compliance_notified_status == "Active");

                if (existingCompliancenotifiedstatus != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Compliance Notified Status with the same name already exists.");
                }
                var maxcompliancennotifiedId = this.mySqlDBContext.ComplianceNotifiedStatusModels
              .Where(d => d.source == "No")
               .Max(d => (int?)d.compliance_notified_id) ?? 0;

                ComplianceNotifiedStatusModels.compliance_notified_id = maxcompliancennotifiedId + 1;

                var TypeModel = this.mySqlDBContext.ComplianceNotifiedStatusModels;
                TypeModel.Add(ComplianceNotifiedStatusModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                ComplianceNotifiedStatusModels.compliance_notified_date = dt1;
                ComplianceNotifiedStatusModels.compliance_notified_status = "Active";
                ComplianceNotifiedStatusModels.source = "No";

                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Compliance Notified Status with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/compliancenotifiedstatus/UpdatecompliancenotifiedstatusDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] ComplianceNotifiedStatusModel ComplianceNotifiedStatusModels)
        {

            try
            {
                if (ComplianceNotifiedStatusModels.compliance_notified_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    ComplianceNotifiedStatusModels.compliance_notified_name = ComplianceNotifiedStatusModels.compliance_notified_name.Trim();
                    var existingCompliancenotifiedstatus = this.mySqlDBContext.ComplianceNotifiedStatusModels
                       .FirstOrDefault(d => d.compliance_notified_name == ComplianceNotifiedStatusModels.compliance_notified_name && d.compliance_notified_id != ComplianceNotifiedStatusModels.compliance_notified_id && d.compliance_notified_status == "Active");

                    if (existingCompliancenotifiedstatus != null)
                    {
                        return BadRequest("Error: Compliance Notified Status with the same name already exists.");
                    }
                    this.mySqlDBContext.Attach(ComplianceNotifiedStatusModels);
                    this.mySqlDBContext.Entry(ComplianceNotifiedStatusModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(ComplianceNotifiedStatusModels);

                    Type type = typeof(ComplianceNotifiedStatusModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(ComplianceNotifiedStatusModels, null) == null || property.GetValue(ComplianceNotifiedStatusModels, null).Equals(0))
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
                    return BadRequest("Error: Compliance Notified Status with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

        [Route("api/compliancenotifiedstatus/DeletecompliancenotifiedstatusDetails")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new ComplianceNotifiedStatusModel { compliance_notified_id = id };
            currentClass.compliance_notified_status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("compliance_notified_status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

    }
}

