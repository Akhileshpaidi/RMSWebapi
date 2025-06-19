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
    public class risksuperadmincontrolController : ControllerBase
    {
        private readonly CommonDBContext commonDBContext;

        public IConfiguration Configuration { get; }

        public risksuperadmincontrolController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }
        [Route("api/superadmincontrol/Getsuperadminadmincontrol")]
        [HttpGet]

        public IEnumerable<risksuperadmincontrolcompmodel> Getsuperadminadmincontrol()
        {

            return this.commonDBContext.risksuperadmincontrolcompmodels.Where(x => x.status == "Active").ToList();
        }

        [Route("api/superadmincontrol/insertsuperadminadmincontrol")]
        [HttpPost]


        public IActionResult insertsuperadminadmincontrol([FromBody] risksuperadmincontrolcompmodel risksuperadmincontrolcompmodels)
        {

            try
            {
                risksuperadmincontrolcompmodels.controlname = risksuperadmincontrolcompmodels.controlname.Trim();

                var exisitingrecord = this.commonDBContext.risksuperadmincontrolcompmodels.FirstOrDefault(d => d.controlname == risksuperadmincontrolcompmodels.controlname && d.status == "Active");
                if (exisitingrecord != null)
                {
                    return BadRequest("Error: Control name with the same name already exists.");

                }
                var risksuperadmincontrolcompmodel = this.commonDBContext.risksuperadmincontrolcompmodels;
                risksuperadmincontrolcompmodel.Add(risksuperadmincontrolcompmodels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risksuperadmincontrolcompmodels.createddate = dt1;
                risksuperadmincontrolcompmodels.status = "Active";
                
                this.commonDBContext.SaveChanges();
                return Ok();

            }
            catch (Exception ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: control name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/superadmincontrol/Updatesuperadmincontrol")]
        [HttpPut]
        public IActionResult Updatesuperadmincontrol([FromBody] risksuperadmincontrolcompmodel risksuperadmincontrolcompmodels)
        {
            try
            {
                if (risksuperadmincontrolcompmodels.controlid == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    risksuperadmincontrolcompmodels.controlname = risksuperadmincontrolcompmodels.controlname?.Trim();

                    var existing = this.commonDBContext.risksuperadmincontrolcompmodels
                  .FirstOrDefault(d => d.controlname == risksuperadmincontrolcompmodels.controlname && d.controlid != risksuperadmincontrolcompmodels.controlid && d.status == "Active");

                    if (existing != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: control name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.commonDBContext.Attach(risksuperadmincontrolcompmodels);
                    this.commonDBContext.Entry(risksuperadmincontrolcompmodels).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(risksuperadmincontrolcompmodels);

                    Type type = typeof(risksuperadmincontrolcompmodel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risksuperadmincontrolcompmodels, null) == null || property.GetValue(risksuperadmincontrolcompmodels, null).Equals(0))
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
                    return BadRequest("Error: control name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }


        [Route("api/superadmincontrol/Deletesuperadmincontrol")]
        [HttpDelete]
        public void Deletesuperadmincontrol(int id)
        {
            var currentClass = new risksuperadmincontrolcompmodel { controlid = id };
            currentClass.status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }


    }
}
