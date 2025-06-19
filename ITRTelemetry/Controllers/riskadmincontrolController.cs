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
    public class riskadmincontrolController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public IConfiguration Configuration { get; }

        public riskadmincontrolController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }

        [Route("api/admincontrol/Getadminadmincontrol")]
        [HttpGet]

        public IEnumerable<riskadmincontrolcontrolermodel> Getadminadmincontrol()
        {

            return this.mySqlDBContext.riskadmincontrolcontrolermodels.Where(x => x.status == "Active").ToList();
        }

        [Route("api/admincontrol/insertadminadmincontrol")]
        [HttpPost]


        public IActionResult insertadminadmincontrol([FromBody] riskadmincontrolcontrolermodel riskadmincontrolcontrolermodels)
        {

            try
            {
                riskadmincontrolcontrolermodels.controlname = riskadmincontrolcontrolermodels.controlname.Trim();

                var exisitingrecord = this.mySqlDBContext.riskadmincontrolcontrolermodels.FirstOrDefault(d => d.controlname == riskadmincontrolcontrolermodels.controlname && d.status == "Active");
                if (exisitingrecord != null)
                {
                    return BadRequest("Error: Control name with the same name already exists.");

                }
                var maxLawTypeId = this.mySqlDBContext.riskadmincontrolcontrolermodels
.Where(d => d.isImported == "No")
.Max(d => (int?)d.controlid) ?? 0;

                riskadmincontrolcontrolermodels.controlid = maxLawTypeId + 1;
                var riskadmincontrolcontrolermodel = this.mySqlDBContext.riskadmincontrolcontrolermodels;
                riskadmincontrolcontrolermodel.Add(riskadmincontrolcontrolermodels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskadmincontrolcontrolermodels.createddate = dt1;
                riskadmincontrolcontrolermodels.status = "Active";
                riskadmincontrolcontrolermodels.isImported = "No";
                this.mySqlDBContext.SaveChanges();
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

        [Route("api/admincontrol/Updateadmincontrol")]
        [HttpPut]
        public IActionResult Updateadmincontrol([FromBody] riskadmincontrolcontrolermodel riskadmincontrolcontrolermodels)
        {
            try
            {
                if (riskadmincontrolcontrolermodels.controlid == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    riskadmincontrolcontrolermodels.controlname = riskadmincontrolcontrolermodels.controlname?.Trim();

                    var existing = this.mySqlDBContext.riskadmincontrolcontrolermodels
                  .FirstOrDefault(d => d.controlname == riskadmincontrolcontrolermodels.controlname && d.controlid != riskadmincontrolcontrolermodels.controlid && d.status == "Active");

                    if (existing != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: control name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(riskadmincontrolcontrolermodels);
                    this.mySqlDBContext.Entry(riskadmincontrolcontrolermodels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(riskadmincontrolcontrolermodels);

                    Type type = typeof(riskadmincontrolcontrolermodel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(riskadmincontrolcontrolermodels, null) == null || property.GetValue(riskadmincontrolcontrolermodels, null).Equals(0))
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
                    return BadRequest("Error: control name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }


        [Route("api/admincontrol/Deleteadmincontrol")]
        [HttpDelete]
        public void Deleteadmincontrol(int id)
        {
            var currentClass = new riskadmincontrolcontrolermodel { controlid = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


    }
}
