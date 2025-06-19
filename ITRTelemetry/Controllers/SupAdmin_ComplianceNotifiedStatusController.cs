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
    public class SupAdmin_ComplianceNotifiedStatusController : Controller
    {
        private CommonDBContext commonDBContext;
        public IConfiguration Configuration { get; }

        public SupAdmin_ComplianceNotifiedStatusController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }
        [Route("api/SupAdmin_compliancenotifiedstatus/Getcompliance")]
        [HttpGet]

        public IEnumerable<SupAdmin_ComplianceNotifiedStatusModel> Getcompliance()
        {
            return this.commonDBContext.SupAdmin_ComplianceNotifiedStatusModels.Where(x => x.compliance_notified_status == "Active").ToList();
        }



        [Route("api/SupAdmin_compliancenotifiedstatus/Insertcompliance")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] SupAdmin_ComplianceNotifiedStatusModel SupAdmin_ComplianceNotifiedStatusModels)
        {
            try
            {
                SupAdmin_ComplianceNotifiedStatusModels.compliance_notified_name = SupAdmin_ComplianceNotifiedStatusModels.compliance_notified_name?.Trim();

                var existingCompliancenotifiedstatus = this.commonDBContext.SupAdmin_ComplianceNotifiedStatusModels
                    .FirstOrDefault(d => d.compliance_notified_name == SupAdmin_ComplianceNotifiedStatusModels.compliance_notified_name && d.compliance_notified_status == "Active");

                if (existingCompliancenotifiedstatus != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Compliance Notified Status with the same name already exists.");
                }

                var Maxnotid = this.commonDBContext.SupAdmin_ComplianceNotifiedStatusModels.Max(d => (int?)d.compliance_notified_id) ?? 5000;

                SupAdmin_ComplianceNotifiedStatusModels.compliance_notified_id = Maxnotid + 1;

                var TypeModel = this.commonDBContext.SupAdmin_ComplianceNotifiedStatusModels;
                TypeModel.Add(SupAdmin_ComplianceNotifiedStatusModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                SupAdmin_ComplianceNotifiedStatusModels.compliance_notified_date = dt1;
                SupAdmin_ComplianceNotifiedStatusModels.compliance_notified_status = "Active";
                this.commonDBContext.SaveChanges();
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

        [Route("api/SupAdmin_compliancenotifiedstatus/Updatecompliance")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] SupAdmin_ComplianceNotifiedStatusModel SupAdmin_ComplianceNotifiedStatusModels)
        {

            try
            {
                if (SupAdmin_ComplianceNotifiedStatusModels.compliance_notified_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    SupAdmin_ComplianceNotifiedStatusModels.compliance_notified_name = SupAdmin_ComplianceNotifiedStatusModels.compliance_notified_name?.Trim();
                    var existingCompliancenotifiedstatus = this.commonDBContext.SupAdmin_ComplianceNotifiedStatusModels
                       .FirstOrDefault(d => d.compliance_notified_name == SupAdmin_ComplianceNotifiedStatusModels.compliance_notified_name && d.compliance_notified_id != SupAdmin_ComplianceNotifiedStatusModels.compliance_notified_id && d.compliance_notified_status == "Active");

                    if (existingCompliancenotifiedstatus != null)
                    {
                        return BadRequest("Error: Compliance Notified Status with the same name already exists.");
                    }
                    this.commonDBContext.Attach(SupAdmin_ComplianceNotifiedStatusModels);
                    this.commonDBContext.Entry(SupAdmin_ComplianceNotifiedStatusModels).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(SupAdmin_ComplianceNotifiedStatusModels);

                    Type type = typeof(SupAdmin_ComplianceNotifiedStatusModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(SupAdmin_ComplianceNotifiedStatusModels, null) == null || property.GetValue(SupAdmin_ComplianceNotifiedStatusModels, null).Equals(0))
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
                    return BadRequest("Error: Compliance Notified Status with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

        [Route("api/SupAdmin_compliancenotifiedstatus/Deletecompliance")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new SupAdmin_ComplianceNotifiedStatusModel { compliance_notified_id = id };
            currentClass.compliance_notified_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("compliance_notified_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }
    }
}
