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
    public class riskadmineventfrequencyController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public IConfiguration Configuration { get; }

        public riskadmineventfrequencyController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }

        [Route("api/admineventfrequency/Getadmineventfrequency")]
        [HttpGet]

        public IEnumerable<riskadmineventmodel> Getadmineventfrequency()
        {

            return this.mySqlDBContext.riskadmineventmodels.Where(x => x.status == "Active").ToList();
        }

        [Route("api/admineventfrequency/insertadmineventfrequency")]
        [HttpPost]


        public IActionResult insertadmineventfrequency([FromBody] riskadmineventmodel riskadmineventmodels)
        {

            try
            {
                riskadmineventmodels.eventfrequencyname = riskadmineventmodels.eventfrequencyname.Trim();

                var exisitingrecord = this.mySqlDBContext.riskadmineventmodels.FirstOrDefault(d => d.eventfrequencyname == riskadmineventmodels.eventfrequencyname && d.status == "Active");
                if (exisitingrecord != null)
                {
                    return BadRequest("Error: Event name with the same name already exists.");

                }
                var maxLawTypeId = this.mySqlDBContext.riskadmineventmodels
.Where(d => d.isImported == "No")
.Max(d => (int?)d.eventfrequencyid) ?? 0; // If no records are found, default to 0
                                          // Increment the law_type_id by 1
                riskadmineventmodels.eventfrequencyid = maxLawTypeId + 1;
                var riskadmineventmodel = this.mySqlDBContext.riskadmineventmodels;
                riskadmineventmodel.Add(riskadmineventmodels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskadmineventmodels.createddate = dt1;
                riskadmineventmodels.status = "Active";
                riskadmineventmodels.isImported = "No";
                this.mySqlDBContext.SaveChanges();
                return Ok();

            }catch (Exception ex)
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

        [Route("api/admineventfrequency/Updateadmineventfrequency")]
        [HttpPut]
        public IActionResult Updateadmineventfrequency([FromBody] riskadmineventmodel riskadmineventmodels)
        {
            try
            {
                if (riskadmineventmodels.eventfrequencyid == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    riskadmineventmodels.eventfrequencyname = riskadmineventmodels.eventfrequencyname?.Trim();

                    var existing = this.mySqlDBContext.riskadmineventmodels
                  .FirstOrDefault(d => d.eventfrequencyname == riskadmineventmodels.eventfrequencyname && d.eventfrequencyid != riskadmineventmodels.eventfrequencyid && d.status == "Active");

                    if (existing != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: control name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(riskadmineventmodels);
                    this.mySqlDBContext.Entry(riskadmineventmodels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(riskadmineventmodels);

                    Type type = typeof(riskadmineventmodel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(riskadmineventmodels, null) == null || property.GetValue(riskadmineventmodels, null).Equals(0))
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
                    return BadRequest("Error: Event name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }


        [Route("api/admineventfrequency/Deleteadmineventfrequency")]
        [HttpDelete]
        public void Deleteadmineventfrequency(int id)
        {
            var currentClass = new riskadmineventmodel { eventfrequencyid = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


    }
}
