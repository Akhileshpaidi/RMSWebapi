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
    public class SupAdmin_CompliancePeriodController : Controller
    {
        private CommonDBContext commonDBContext;
        public IConfiguration Configuration { get; }

        public SupAdmin_CompliancePeriodController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }
        [Route("api/SupAdmin_complianceperiod/GetcomplianceperiodDetails")]
        [HttpGet]

        public IEnumerable<SupAdmin_CompliancePeriodModel> GetComplianceNotifiedStatus()
        {
            return this.commonDBContext.SupAdmin_CompliancePeriodModels.Where(x => x.compliance_period_status == "Active").ToList();
        }



        [Route("api/SupAdmin_complianceperiod/InsertcomplianceperiodDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] SupAdmin_CompliancePeriodModel SupAdmin_CompliancePeriodModels)
        {
            try
            {
                SupAdmin_CompliancePeriodModels.compliance_period_start = SupAdmin_CompliancePeriodModels.compliance_period_start?.Trim();
                SupAdmin_CompliancePeriodModels.compliance_period_end = SupAdmin_CompliancePeriodModels.compliance_period_end?.Trim();


                // Query to check if there's an existing CompliancePeriodModel
                var existingPenaltyCategory = this.commonDBContext.SupAdmin_CompliancePeriodModels
                    .FirstOrDefault(d =>
                        d.compliance_period_start == SupAdmin_CompliancePeriodModels.compliance_period_start &&
                        d.compliance_period_end == SupAdmin_CompliancePeriodModels.compliance_period_end &&
                        d.start_compliance_year_format == SupAdmin_CompliancePeriodModels.start_compliance_year_format &&
                        d.end_compliance_year_format == SupAdmin_CompliancePeriodModels.end_compliance_year_format &&
                        d.compliance_period_status == "Active"
                    );


                if (existingPenaltyCategory != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Compliance Start Period with the same name already exists.");
                }
                if (SupAdmin_CompliancePeriodModels.check_box != false)
                {
                    if (SupAdmin_CompliancePeriodModels.compliance_period_start == SupAdmin_CompliancePeriodModels.compliance_period_end)
                    {
                        return BadRequest("Error: Start and End year periods should not be the same.");
                    }
                    if (SupAdmin_CompliancePeriodModels.start_compliance_year_format != SupAdmin_CompliancePeriodModels.end_compliance_year_format)
                    {
                        return BadRequest("Error: Start and End year formats must be the same.");
                    }
                }

                var Maxperiodid = this.commonDBContext.SupAdmin_CompliancePeriodModels.Max(d => (int?)d.compliance_period_id) ?? 5000;

                SupAdmin_CompliancePeriodModels.compliance_period_id = Maxperiodid + 1;

                var TypeModel = this.commonDBContext.SupAdmin_CompliancePeriodModels;
                TypeModel.Add(SupAdmin_CompliancePeriodModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                SupAdmin_CompliancePeriodModels.created_date = dt1;
                SupAdmin_CompliancePeriodModels.compliance_period_status = "Active";
                

                this.commonDBContext.SaveChanges();
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

        [Route("api/SupAdmin_complianceperiod/UpdatecomplianceperiodDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] SupAdmin_CompliancePeriodModel SupAdmin_CompliancePeriodModels)
        {

            try
            {
                if (SupAdmin_CompliancePeriodModels.compliance_period_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    SupAdmin_CompliancePeriodModels.compliance_period_start = SupAdmin_CompliancePeriodModels.compliance_period_start?.Trim();
                    SupAdmin_CompliancePeriodModels.compliance_period_end = SupAdmin_CompliancePeriodModels.compliance_period_end?.Trim();

                    if (SupAdmin_CompliancePeriodModels.compliance_period_start == SupAdmin_CompliancePeriodModels.compliance_period_end)
                    {
                        return BadRequest("Error: Start and End year periods should not be the same.");
                    }
                    if (SupAdmin_CompliancePeriodModels.start_compliance_year_format != SupAdmin_CompliancePeriodModels.end_compliance_year_format)
                    {
                        return BadRequest("Error: Start and End year formats must be the same.");
                    }
                    // Query to check if there's an existing CompliancePeriodModel
                    var existingPenaltyCategory = this.commonDBContext.SupAdmin_CompliancePeriodModels
                        .FirstOrDefault(d =>
                            d.compliance_period_start == SupAdmin_CompliancePeriodModels.compliance_period_start &&
                            d.compliance_period_end == SupAdmin_CompliancePeriodModels.compliance_period_end &&
                            d.start_compliance_year_format == SupAdmin_CompliancePeriodModels.start_compliance_year_format &&
                            d.end_compliance_year_format == SupAdmin_CompliancePeriodModels.end_compliance_year_format &&
                            d.compliance_period_id != SupAdmin_CompliancePeriodModels.compliance_period_id &&
                            d.compliance_period_status == "Active"
                        );


                    if (existingPenaltyCategory != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Penalty Category with the same name already exists.");
                    }
                    this.commonDBContext.Attach(SupAdmin_CompliancePeriodModels);
                    this.commonDBContext.Entry(SupAdmin_CompliancePeriodModels).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(SupAdmin_CompliancePeriodModels);

                    Type type = typeof(SupAdmin_CompliancePeriodModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(SupAdmin_CompliancePeriodModels, null) == null || property.GetValue(SupAdmin_CompliancePeriodModels, null).Equals(0))
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
                    return BadRequest("Error: Penalty Category with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

        [Route("api/SupAdmin_complianceperiod/DeletecomplianceperiodDetails")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new SupAdmin_CompliancePeriodModel { compliance_period_id = id };
            currentClass.compliance_period_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("compliance_period_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }

    }
}
