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
    public class risksuperadmineventfrequencyController : ControllerBase
    {
        private readonly CommonDBContext commonDBContext;

        public IConfiguration Configuration { get; }

        public risksuperadmineventfrequencyController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }
        [Route("api/superadmineventfrequency/Getsuperadmineventfrequency")]
        [HttpGet]

        public IEnumerable<risksuperadmineventfrequencymodel> Getadmineventfrequency()
        {

            return this.commonDBContext.risksuperadmineventfrequencymodels.Where(x => x.status == "Active").ToList();
        }

        [Route("api/superadmineventfrequency/insertsuperadmineventfrequency")]
        [HttpPost]


        public IActionResult insertsuperadmineventfrequency([FromBody] risksuperadmineventfrequencymodel risksuperadmineventfrequencymodels)
        {

            try
            {
                risksuperadmineventfrequencymodels.eventfrequencyname = risksuperadmineventfrequencymodels.eventfrequencyname.Trim();

                var exisitingrecord = this.commonDBContext.risksuperadmineventfrequencymodels.FirstOrDefault(d => d.eventfrequencyname == risksuperadmineventfrequencymodels.eventfrequencyname && d.status == "Active");
                if (exisitingrecord != null)
                {
                    return BadRequest("Error: Event name with the same name already exists.");

                }
                var risksuperadmineventfrequencymodel = this.commonDBContext.risksuperadmineventfrequencymodels;
                risksuperadmineventfrequencymodel.Add(risksuperadmineventfrequencymodels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risksuperadmineventfrequencymodels.createddate = dt1;
                risksuperadmineventfrequencymodels.status = "Active";
                
                this.commonDBContext.SaveChanges();
                return Ok();

            }
            catch (Exception ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Event name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/superadmineventfrequency/Updatesuperadmineventfrequency")]
        [HttpPut]
        public IActionResult Updateadmineventfrequency([FromBody] risksuperadmineventfrequencymodel risksuperadmineventfrequencymodels)
        {
            try
            {
                if (risksuperadmineventfrequencymodels.eventfrequencyid == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    risksuperadmineventfrequencymodels.eventfrequencyname = risksuperadmineventfrequencymodels.eventfrequencyname?.Trim();

                    var existing = this.commonDBContext.risksuperadmineventfrequencymodels
                  .FirstOrDefault(d => d.eventfrequencyname == risksuperadmineventfrequencymodels.eventfrequencyname && d.eventfrequencyid != risksuperadmineventfrequencymodels.eventfrequencyid && d.status == "Active");

                    if (existing != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Event name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.commonDBContext.Attach(risksuperadmineventfrequencymodels);
                    this.commonDBContext.Entry(risksuperadmineventfrequencymodels).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(risksuperadmineventfrequencymodels);

                    Type type = typeof(risksuperadmineventfrequencymodel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risksuperadmineventfrequencymodels, null) == null || property.GetValue(risksuperadmineventfrequencymodels, null).Equals(0))
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
                    return BadRequest("Error: Event name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }


        [Route("api/superadmineventfrequency/Deletesuperadmineventfrequency")]
        [HttpDelete]
        public void Deleteadmineventfrequency(int id)
        {
            var currentClass = new risksuperadmineventfrequencymodel { eventfrequencyid = id };
            currentClass.status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }


    }
}
