using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace ITRTelemetry.Controllers
{
    
    [Produces("application/json")]
    public class SupAdmin_JurisdictionLocationController : ControllerBase
    {
        private CommonDBContext commonDBContext;
        public IConfiguration Configuration { get; }

        public SupAdmin_JurisdictionLocationController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }
        [Route("api/SupAdmin_JurisdictionLocation/GetJurisdictionLocationDetails")]
        [HttpGet]

        public IEnumerable<SupAdmin_JurisdictionLocationModel> GetComplianceNotifiedStatus()
        {
            return this.commonDBContext.SupAdmin_JurisdictionLocationModels.Where(x => x.status == "Active").ToList();
        }



        [Route("api/SupAdmin_JurisdictionLocation/InsertJurisdictionLocationDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] SupAdmin_JurisdictionLocationModel SupAdmin_JurisdictionLocationModels)
        {
            try
            {
                var Maxjudlocid = this.commonDBContext.SupAdmin_JurisdictionLocationModels.Max(d => (int?)d.jurisdiction_location_id) ?? 5000;

                SupAdmin_JurisdictionLocationModels.jurisdiction_location_id = Maxjudlocid + 1;

                var TypeModel = this.commonDBContext.SupAdmin_JurisdictionLocationModels;
                TypeModel.Add(SupAdmin_JurisdictionLocationModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                SupAdmin_JurisdictionLocationModels.jurisdiction_location_create_date = dt1;
                SupAdmin_JurisdictionLocationModels.status = "Active";
              
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

        [Route("api/SupAdmin_JurisdictionLocation/UpdateJurisdictionLocationDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] SupAdmin_JurisdictionLocationModel SupAdmin_JurisdictionLocationModels)
        {

            try
            {
                if (SupAdmin_JurisdictionLocationModels.jurisdiction_location_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                   
                    this.commonDBContext.Attach(SupAdmin_JurisdictionLocationModels);
                    this.commonDBContext.Entry(SupAdmin_JurisdictionLocationModels).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(SupAdmin_JurisdictionLocationModels);

                    Type type = typeof(SupAdmin_JurisdictionLocationModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(SupAdmin_JurisdictionLocationModels, null) == null || property.GetValue(SupAdmin_JurisdictionLocationModels, null).Equals(0))
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

        [Route("api/SupAdmin_JurisdictionLocation/DeleteJurisdictionLocationDetails")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new SupAdmin_JurisdictionLocationModel { jurisdiction_location_id = id };
            currentClass.status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }

    }

}
