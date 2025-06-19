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
    //[ApiController]
    //[Route("api/[controller]")]
    [Produces("application/json")]
    public class CompliancePeriodController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public CompliancePeriodController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;

        }

        [Route("api/complianceperiod/GetcomplianceperiodDetails")]
        [HttpGet]

        public IEnumerable<CompliancePeriodModel> GetComplianceNotifiedStatus()
        {
            return this.mySqlDBContext.CompliancePeriodModels.Where(x => x.compliance_period_status == "Active").ToList();
        }



        [Route("api/complianceperiod/InsertcomplianceperiodDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] CompliancePeriodModel CompliancePeriodModels)
        {
            try
            {
                CompliancePeriodModels.compliance_period_start = CompliancePeriodModels.compliance_period_start?.Trim();
                CompliancePeriodModels.compliance_period_end = CompliancePeriodModels.compliance_period_end?.Trim();

                // Query to check if there's an existing CompliancePeriodModel
                var existingPenaltyCategory = this.mySqlDBContext.CompliancePeriodModels
                    .FirstOrDefault(d =>
                        d.compliance_period_start == CompliancePeriodModels.compliance_period_start &&
                        d.compliance_period_end == CompliancePeriodModels.compliance_period_end &&
                        d.start_compliance_year_format == CompliancePeriodModels.start_compliance_year_format &&
                        d.end_compliance_year_format == CompliancePeriodModels.end_compliance_year_format &&
                        d.compliance_period_status == "Active"
                    );


                if (existingPenaltyCategory != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Compliance Start Period with the same name already exists.");
                }
                if (CompliancePeriodModels.check_box != false)
                {
                    if (CompliancePeriodModels.compliance_period_start == CompliancePeriodModels.compliance_period_end)
                    {
                        return BadRequest("Error: Start and End year periods should not be the same.");
                    }
                    if (CompliancePeriodModels.start_compliance_year_format != CompliancePeriodModels.end_compliance_year_format)
                    {
                        return BadRequest("Error: Start and End year formats must be the same.");
                    }
                }
                var maxcompliancennotifiedId = this.mySqlDBContext.CompliancePeriodModels
              .Where(d => d.IsImportedData == "No")
               .Max(d => (int?)d.compliance_period_id) ?? 0;

                CompliancePeriodModels.compliance_period_id = maxcompliancennotifiedId + 1;

                var TypeModel = this.mySqlDBContext.CompliancePeriodModels;
                TypeModel.Add(CompliancePeriodModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                CompliancePeriodModels.created_date = dt1;
                CompliancePeriodModels.compliance_period_status = "Active";
                CompliancePeriodModels.IsImportedData = "No";
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

        [Route("api/complianceperiod/UpdatecomplianceperiodDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] CompliancePeriodModel CompliancePeriodModels)
        {

            try
            {
                if (CompliancePeriodModels.compliance_period_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    CompliancePeriodModels.compliance_period_start = CompliancePeriodModels.compliance_period_start?.Trim();
                    CompliancePeriodModels.compliance_period_end = CompliancePeriodModels.compliance_period_end?.Trim();

                    // Query to check if there's an existing CompliancePeriodModel
                    var existingPenaltyCategory = this.mySqlDBContext.CompliancePeriodModels
                        .FirstOrDefault(d =>
                            d.compliance_period_start == CompliancePeriodModels.compliance_period_start &&
                            d.compliance_period_end == CompliancePeriodModels.compliance_period_end &&
                            d.start_compliance_year_format == CompliancePeriodModels.start_compliance_year_format &&
                            d.end_compliance_year_format == CompliancePeriodModels.end_compliance_year_format &&
                            d.compliance_period_id != CompliancePeriodModels.compliance_period_id &&
                            d.compliance_period_status == "Active"
                        );


                    if (existingPenaltyCategory != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Penalty Category with the same name already exists.");
                    }
                    this.mySqlDBContext.Attach(CompliancePeriodModels);
                    this.mySqlDBContext.Entry(CompliancePeriodModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(CompliancePeriodModels);

                    Type type = typeof(CompliancePeriodModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(CompliancePeriodModels, null) == null || property.GetValue(CompliancePeriodModels, null).Equals(0))
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

        [Route("api/complianceperiod/DeletecomplianceperiodDetails")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new CompliancePeriodModel { compliance_period_id = id };
            currentClass.compliance_period_status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("compliance_period_status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

    }
}

