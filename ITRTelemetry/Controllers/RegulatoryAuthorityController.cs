using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DomainModel;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Threading.Channels;
using System.Linq;
using MySqlConnector;
namespace ITRTelemetry.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    [Produces("application/json")]
    public class RegulatoryAuthorityController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public RegulatoryAuthorityController(MySqlDBContext mySqlDBContext) 
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/regulatoryauthority/GetregulatoryauthorityDetails")]
        [HttpGet]

        public IEnumerable<RegulatoryAuthorityModel> GetRegulatoryAuthority()
        {
            return this.mySqlDBContext.RegulatoryAuthorityModels.Where(x => x.regulatory_authority_status == "Active").ToList();
        }



        [Route("api/regulatoryauthority/InsertregulatoryauthorityDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] RegulatoryAuthorityModel RegulatoryAuthorityModels)
        {
            try
            {
                RegulatoryAuthorityModels.regulatory_authority_name = RegulatoryAuthorityModels.regulatory_authority_name.Trim();

                var existingRegulatoryAuthority = this.mySqlDBContext.RegulatoryAuthorityModels
                    .FirstOrDefault(d => d.regulatory_authority_name == RegulatoryAuthorityModels.regulatory_authority_name && d.regulatory_authority_status == "Active");

                if (existingRegulatoryAuthority != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Regulatory Authority with the same name already exists.");
                }
                var maxregulatoryId = this.mySqlDBContext.RegulatoryAuthorityModels
          .Where(d => d.source == "No")
          .Max(d => (int?)d.regulatory_authority_id) ?? 0; // If no records are found, default to 0

                // Increment the law_type_id by 1
                RegulatoryAuthorityModels.regulatory_authority_id = maxregulatoryId + 1;

                var TypeModel = this.mySqlDBContext.RegulatoryAuthorityModels;


              
                TypeModel.Add(RegulatoryAuthorityModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                RegulatoryAuthorityModels.regulatory_authority_created_date = dt1;
                RegulatoryAuthorityModels.regulatory_authority_status = "Active";
                RegulatoryAuthorityModels.source = "No";

                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Regulatory Authority with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/regulatoryauthority/UpdateregulatoryauthorityDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] RegulatoryAuthorityModel RegulatoryAuthorityModels)
        {

            try
            {
                if (RegulatoryAuthorityModels.regulatory_authority_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    RegulatoryAuthorityModels.regulatory_authority_name = RegulatoryAuthorityModels.regulatory_authority_name?.Trim();
                    var existingRegulatoryAuthority = this.mySqlDBContext.RegulatoryAuthorityModels
                       .FirstOrDefault(d => d.regulatory_authority_name == RegulatoryAuthorityModels.regulatory_authority_name && d.regulatory_authority_id != RegulatoryAuthorityModels.regulatory_authority_id && d.regulatory_authority_status == "Active");

                    if (existingRegulatoryAuthority != null)
                    {
                        return BadRequest("Error: Regulatory Authority with the same name already exists.");
                    }
                    this.mySqlDBContext.Attach(RegulatoryAuthorityModels);
                    this.mySqlDBContext.Entry(RegulatoryAuthorityModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(RegulatoryAuthorityModels);

                    Type type = typeof(RegulatoryAuthorityModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(RegulatoryAuthorityModels, null) == null || property.GetValue(RegulatoryAuthorityModels, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
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
                    return BadRequest("Error: Regulatory Authority with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

        [Route("api/regulatoryauthority/DeleteregulatoryauthorityDetails")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new RegulatoryAuthorityModel { regulatory_authority_id = id };
            currentClass.regulatory_authority_status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("regulatory_authority_status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

    }
}
