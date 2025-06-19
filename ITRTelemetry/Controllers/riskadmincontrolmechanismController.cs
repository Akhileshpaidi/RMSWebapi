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
    public class riskadmincontrolmechanismController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public IConfiguration Configuration { get; }

        public riskadmincontrolmechanismController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }

        [Route("api/admincontrolmonitoring/Getadminadmincontrolmonitoring")]
        [HttpGet]

        public IEnumerable<riskadmincontrolmonitoringmechmodel> Getadminadmincontrol()
        {

            return this.mySqlDBContext.riskadmincontrolmonitoringmechmodels.Where(x => x.status == "Active").ToList();
        }

        [Route("api/admincontrolmonitoring/insertadminadmincontrolmonitoring")]
        [HttpPost]


        public IActionResult insertadminadmincontrolmonitoring([FromBody] riskadmincontrolmonitoringmechmodel riskadmincontrolmonitoringmechmodels)
        {

            try
            {
                riskadmincontrolmonitoringmechmodels.controlmonitoringname = riskadmincontrolmonitoringmechmodels.controlmonitoringname.Trim();

                var exisitingrecord = this.mySqlDBContext.riskadmincontrolmonitoringmechmodels.FirstOrDefault(d => d.controlmonitoringname == riskadmincontrolmonitoringmechmodels.controlmonitoringname && d.status == "Active");
                if (exisitingrecord != null)
                {
                    return BadRequest("Error: Control Monitoring name with the same name already exists.");

                }
                var maxLawTypeId = this.mySqlDBContext.riskadmincontrolmonitoringmechmodels
.Where(d => d.isImported == "No")
.Max(d => (int?)d.controlmonitoringid) ?? 0;

                riskadmincontrolmonitoringmechmodels.controlmonitoringid = maxLawTypeId + 1;
                var riskadmincontrolmonitoringmechmodel = this.mySqlDBContext.riskadmincontrolmonitoringmechmodels;
                riskadmincontrolmonitoringmechmodel.Add(riskadmincontrolmonitoringmechmodels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskadmincontrolmonitoringmechmodels.createddate = dt1;
                riskadmincontrolmonitoringmechmodels.status = "Active";
                riskadmincontrolmonitoringmechmodels.isImported = "No";
                this.mySqlDBContext.SaveChanges();
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

        [Route("api/admincontrolmonitoring/Updateadmincontrolmonitoring")]
        [HttpPut]
        public IActionResult Updateadmincontrolmonitoring([FromBody] riskadmincontrolmonitoringmechmodel riskadmincontrolmonitoringmechmodels)
        {
            try
            {
                if (riskadmincontrolmonitoringmechmodels.controlmonitoringid == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    riskadmincontrolmonitoringmechmodels.controlmonitoringname = riskadmincontrolmonitoringmechmodels.controlmonitoringname?.Trim();

                    var existing = this.mySqlDBContext.riskadmincontrolmonitoringmechmodels
                  .FirstOrDefault(d => d.controlmonitoringname == riskadmincontrolmonitoringmechmodels.controlmonitoringname && d.controlmonitoringid != riskadmincontrolmonitoringmechmodels.controlmonitoringid && d.status == "Active");

                    if (existing != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: control name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(riskadmincontrolmonitoringmechmodels);
                    this.mySqlDBContext.Entry(riskadmincontrolmonitoringmechmodels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(riskadmincontrolmonitoringmechmodels);

                    Type type = typeof(riskadmincontrolmonitoringmechmodel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(riskadmincontrolmonitoringmechmodels, null) == null || property.GetValue(riskadmincontrolmonitoringmechmodels, null).Equals(0))
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
                    return BadRequest("Error: control Monitoring name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }


        [Route("api/admincontrolmonitoring/Deleteadmincontrolmonitoring")]
        [HttpDelete]
        public void Deleteadmincontrolmonitoring(int id)
        {
            var currentClass = new riskadmincontrolmonitoringmechmodel { controlmonitoringid = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

    }
}
