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
    public class risksuperadminactivityController : ControllerBase
    {
        private readonly CommonDBContext commonDBContext;

        public IConfiguration Configuration { get; }

        public risksuperadminactivityController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }

        [Route("api/superadminactivityfrequency/Getsuperadminactivityfrequency")]
        [HttpGet]

        public IEnumerable<risksuperadminactivityfrequencymodel> Getsuperadminactivityfrequency()
        {

            return this.commonDBContext.risksuperadminactivityfrequencymodels.Where(x => x.status == "Active").ToList();
        }

        [Route("api/superadminactivityfrequency/insertsuperadminactivityfrequency")]
        [HttpPost]


        public IActionResult insertsuperadminactivityfrequency([FromBody] risksuperadminactivityfrequencymodel risksuperadminactivityfrequencymodels)
        {

            try
            {
                risksuperadminactivityfrequencymodels.activityname = risksuperadminactivityfrequencymodels.activityname.Trim();

                //var exisitingrecord = this.commonDBContext.risksuperadminactivityfrequencymodels.FirstOrDefault(d => d.activityname == risksuperadminactivityfrequencymodels.activityname && d.activityvalue == risksuperadminactivityfrequencymodels.activityvalue && d.status == "Active");
                //if (exisitingrecord != null)
                //{
                //    return BadRequest("Error: activity name with the same name already exists.");

                //}
                var exisitingrecord = this.commonDBContext.risksuperadminactivityfrequencymodels
                .FirstOrDefault(d => d.activityvalue == risksuperadminactivityfrequencymodels.activityvalue && d.status == "Active");
                if (exisitingrecord != null)
                {
                    return BadRequest("Error: activity value with the same value already exists.");

                }
                var existingDepartment = this.commonDBContext.risksuperadminactivityfrequencymodels
                .FirstOrDefault(d => d.activityname == risksuperadminactivityfrequencymodels.activityname && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Inherent name with the same name already exists.");
                }
                var risksuperadminactivityfrequencymodel = this.commonDBContext.risksuperadminactivityfrequencymodels;
                risksuperadminactivityfrequencymodel.Add(risksuperadminactivityfrequencymodels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risksuperadminactivityfrequencymodels.createddate = dt1;
                risksuperadminactivityfrequencymodels.status = "Active";
             
                this.commonDBContext.SaveChanges();
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



        [Route("api/superadminactivityfrequency/Updatesuperadminactivityfrequency")]
        [HttpPut]
        public IActionResult Updatesuperadminactivityfrequency([FromBody] risksuperadminactivityfrequencymodel risksuperadminactivityfrequencymodels)
        {
            try
            {
                if (risksuperadminactivityfrequencymodels.activityid == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    risksuperadminactivityfrequencymodels.activityname = risksuperadminactivityfrequencymodels.activityname?.Trim();

                    var existing = this.commonDBContext.risksuperadminactivityfrequencymodels
                  .FirstOrDefault(d => d.activityname == risksuperadminactivityfrequencymodels.activityname && d.activityid != risksuperadminactivityfrequencymodels.activityid && d.status == "Active");

                    if (existing != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Subject name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.commonDBContext.Attach(risksuperadminactivityfrequencymodels);
                    this.commonDBContext.Entry(risksuperadminactivityfrequencymodels).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(risksuperadminactivityfrequencymodels);

                    Type type = typeof(risksuperadminactivityfrequencymodel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risksuperadminactivityfrequencymodels, null) == null || property.GetValue(risksuperadminactivityfrequencymodels, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.commonDBContext.SaveChanges();
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

        [Route("api/superadminactivityfrequency/Deletesuperadminactivityfrequency")]
        [HttpDelete]
        public void Deletesuperadminactivityfrequency(int id)
        {
            var currentClass = new risksuperadminactivityfrequencymodel { activityid = id };
            currentClass.status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }

    }
}
