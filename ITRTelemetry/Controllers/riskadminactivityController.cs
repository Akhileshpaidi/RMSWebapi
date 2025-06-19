using DomainModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using System.Data;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace ITRTelemetry.Controllers
{

    [Produces("application/json")]
    public class riskadminactivityController : ControllerBase
    {

        private readonly MySqlDBContext mySqlDBContext;

        public IConfiguration Configuration { get; }

        public riskadminactivityController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }
        [Route("api/adminactivityfrequency/Getadminactivityfrequency")]
        [HttpGet]

        public IEnumerable<riskadminactivityfrequency> Getadminactivityfrequency()
        {

            return this.mySqlDBContext.riskadminactivityfrequencymodels.Where(x => x.status == "Active").ToList();
        }

        [Route("api/adminactivityfrequency/insertadminactivityfrequency")]
        [HttpPost]
        public IActionResult insertadminactivityfrequency([FromBody] riskadminactivityfrequency riskadminactivityfrequencymodels)
        {

            try
            {
                riskadminactivityfrequencymodels.activityname = riskadminactivityfrequencymodels.activityname?.Trim();

                var exisitingrecord = this.mySqlDBContext.riskadminactivityfrequencymodels
                .FirstOrDefault(d => d.activityvalue == riskadminactivityfrequencymodels.activityvalue && d.status == "Active");
                if (exisitingrecord != null)
                {
                    return BadRequest("Error: activity value with the same value already exists.");

                }
                var existingDepartment = this.mySqlDBContext.riskadminactivityfrequencymodels
                .FirstOrDefault(d => d.activityname == riskadminactivityfrequencymodels.activityname  && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Inherent name with the same name already exists.");
                }

                var maxLawTypeId = this.mySqlDBContext.riskadminactivityfrequencymodels
.Where(d => d.isImported == "No")
.Max(d => (int?)d.activityid) ?? 0; // If no records are found, default to 0
                                    // Increment the law_type_id by 1
                riskadminactivityfrequencymodels.activityid = maxLawTypeId + 1;
                var riskadminactivityfrequency = this.mySqlDBContext.riskadminactivityfrequencymodels;
                riskadminactivityfrequency.Add(riskadminactivityfrequencymodels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskadminactivityfrequencymodels.createddate = dt1;
                riskadminactivityfrequencymodels.status = "Active";
                riskadminactivityfrequencymodels.isImported = "No";
                this.mySqlDBContext.SaveChanges();
                return Ok();

            }
            catch (Exception ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: activity name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        [Route("api/adminactivityfrequency/Updateadminactivityfrequency")]
        [HttpPut]
        public IActionResult Updateadminactivityfrequency([FromBody] riskadminactivityfrequency riskadminactivityfrequencymodels)
        {
            try
            {
                if (riskadminactivityfrequencymodels.activityid == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    riskadminactivityfrequencymodels.activityname = riskadminactivityfrequencymodels.activityname?.Trim();

                    var existing = this.mySqlDBContext.riskadminactivityfrequencymodels
                  .FirstOrDefault(d => d.activityname == riskadminactivityfrequencymodels.activityname && d.activityid != riskadminactivityfrequencymodels.activityid && d.status == "Active");

                    if (existing != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: activity name with the same name already exists.");
                    }

                   

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(riskadminactivityfrequencymodels);
                    this.mySqlDBContext.Entry(riskadminactivityfrequencymodels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(riskadminactivityfrequencymodels);

                    Type type = typeof(riskadminactivityfrequency);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(riskadminactivityfrequencymodels, null) == null || property.GetValue(riskadminactivityfrequencymodels, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    return Ok("Update successful");
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: activity name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

        [Route("api/adminactivityfrequency/Deleteadminactivityfrequency")]
        [HttpDelete]
        public void Deleteadminactivityfrequency(int id)
        {
            var currentClass = new riskadminactivityfrequency { activityid = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

    }
}
