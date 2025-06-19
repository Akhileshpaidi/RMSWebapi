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
using OpenXmlPowerTools.HtmlToWml.CSS;
namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class risksuperadmincontrolmonitoringController : ControllerBase
    {
        private readonly CommonDBContext commonDBContext;

        public IConfiguration Configuration { get; }

        public risksuperadmincontrolmonitoringController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }

        [Route("api/superadmincontrolmonitoring/Getsuperadmincontrolmonitoring")]
        [HttpGet]

        public IEnumerable<risksuperadmincontrolmonitoringmodel> Getsuperadmincontrolmonitoring()
        {

            return this.commonDBContext.risksuperadmincontrolmonitoringmodels.Where(x => x.status == "Active").ToList();
        }

        [Route("api/superadmincontrolmonitoring/insertsupereadminadmincontrolmonitoring")]
        [HttpPost]


        public IActionResult insertsupereadminadmincontrolmonitoring([FromBody] risksuperadmincontrolmonitoringmodel risksuperadmincontrolmonitoringmodels)
        {

            try
            {
                risksuperadmincontrolmonitoringmodels.controlmonitoringname = risksuperadmincontrolmonitoringmodels.controlmonitoringname.Trim();

                var exisitingrecord = this.commonDBContext.risksuperadmincontrolmonitoringmodels.FirstOrDefault(d => d.controlmonitoringname == risksuperadmincontrolmonitoringmodels.controlmonitoringname && d.status == "Active");
                if (exisitingrecord != null)
                {
                    return BadRequest("Error: Control Monitoring name with the same name already exists.");

                }
                var risksuperadmincontrolmonitoringmodel = this.commonDBContext.risksuperadmincontrolmonitoringmodels;
                risksuperadmincontrolmonitoringmodel.Add(risksuperadmincontrolmonitoringmodels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risksuperadmincontrolmonitoringmodels.createddate = dt1;
                risksuperadmincontrolmonitoringmodels.status = "Active";
           
                this.commonDBContext.SaveChanges();
                return Ok();

            }
            catch (Exception ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Control Monitoring name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/superadmincontrolmonitoring/Updatesuperadmincontrolmonitoring")]
        [HttpPut]
        public IActionResult Updatesuperadmincontrolmonitoring([FromBody] risksuperadmincontrolmonitoringmodel risksuperadmincontrolmonitoringmodels)
        {
            try
            {
                if (risksuperadmincontrolmonitoringmodels.controlmonitoringid == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    risksuperadmincontrolmonitoringmodels.controlmonitoringname = risksuperadmincontrolmonitoringmodels.controlmonitoringname?.Trim();

                    var existing = this.commonDBContext.risksuperadmincontrolmonitoringmodels
                  .FirstOrDefault(d => d.controlmonitoringname == risksuperadmincontrolmonitoringmodels.controlmonitoringname && d.controlmonitoringid != risksuperadmincontrolmonitoringmodels.controlmonitoringid && d.status == "Active");

                    if (existing != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: control name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.commonDBContext.Attach(risksuperadmincontrolmonitoringmodels);
                    this.commonDBContext.Entry(risksuperadmincontrolmonitoringmodels).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(risksuperadmincontrolmonitoringmodels);

                    Type type = typeof(risksuperadmincontrolmonitoringmodel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risksuperadmincontrolmonitoringmodels, null) == null || property.GetValue(risksuperadmincontrolmonitoringmodels, null).Equals(0))
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
                    return BadRequest("Error: control Monitoring name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }


        [Route("api/superadmincontrolmonitoring/Deletesuperadmincontrolmonitoring")]
        [HttpDelete]
        public void Deletesuperadmincontrolmonitoring(int id)
        {
            var currentClass = new risksuperadmincontrolmonitoringmodel { controlmonitoringid = id };
            currentClass.status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }

    }
}
