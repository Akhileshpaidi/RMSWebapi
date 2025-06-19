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
using Microsoft.Extensions.Configuration;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class SupAdmin_RegulatoryAuthorityController : Controller
    {
        private CommonDBContext commonDBContext;
        public IConfiguration Configuration { get; }

        public SupAdmin_RegulatoryAuthorityController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }
        [Route("api/SupAdmin_RegulatoryAuthority/GetRegulatoryAuthorityDetails")]
        [HttpGet]

        public IEnumerable<SupAdmin_RegulatoryAuthorityModel> GetRegulatoryAuthority()
        {
            return this.commonDBContext.SupAdmin_RegulatoryAuthorityModels.Where(x => x.regulatory_authority_status == "Active").ToList();
        }



        [Route("api/SupAdmin_RegulatoryAuthority/InsertRegulatoryAuthorityDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] SupAdmin_RegulatoryAuthorityModel SupAdmin_RegulatoryAuthorityModels)
        {
            try
            {
                SupAdmin_RegulatoryAuthorityModels.regulatory_authority_name = SupAdmin_RegulatoryAuthorityModels.regulatory_authority_name.Trim();

                var existingRegulatoryAuthority = this.commonDBContext.SupAdmin_RegulatoryAuthorityModels
                    .FirstOrDefault(d => d.regulatory_authority_name == SupAdmin_RegulatoryAuthorityModels.regulatory_authority_name && d.regulatory_authority_status == "Active");

                if (existingRegulatoryAuthority != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Regulatory Authority with the same name already exists.");
                }

                var Maxregulatoryid = this.commonDBContext.SupAdmin_RegulatoryAuthorityModels.Max(d => (int?)d.regulatory_authority_id) ?? 5000;

                SupAdmin_RegulatoryAuthorityModels.regulatory_authority_id = Maxregulatoryid + 1;

                var TypeModel = this.commonDBContext.SupAdmin_RegulatoryAuthorityModels;
                TypeModel.Add(SupAdmin_RegulatoryAuthorityModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                SupAdmin_RegulatoryAuthorityModels.regulatory_authority_created_date = dt1;
                SupAdmin_RegulatoryAuthorityModels.regulatory_authority_status = "Active";
                this.commonDBContext.SaveChanges();
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

        [Route("api/SupAdmin_RegulatoryAuthority/UpdateRegulatoryAuthorityDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] SupAdmin_RegulatoryAuthorityModel SupAdmin_RegulatoryAuthorityModels)
        {

            try
            {
                if (SupAdmin_RegulatoryAuthorityModels.regulatory_authority_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    SupAdmin_RegulatoryAuthorityModels.regulatory_authority_name = SupAdmin_RegulatoryAuthorityModels.regulatory_authority_name?.Trim();
                    var existingRegulatoryAuthority = this.commonDBContext.SupAdmin_RegulatoryAuthorityModels
                       .FirstOrDefault(d => d.regulatory_authority_name == SupAdmin_RegulatoryAuthorityModels.regulatory_authority_name && d.regulatory_authority_id != SupAdmin_RegulatoryAuthorityModels.regulatory_authority_id && d.regulatory_authority_status == "Active");

                    if (existingRegulatoryAuthority != null)
                    {
                        return BadRequest("Error: Regulatory Authority with the same name already exists.");
                    }
                    this.commonDBContext.Attach(SupAdmin_RegulatoryAuthorityModels);
                    this.commonDBContext.Entry(SupAdmin_RegulatoryAuthorityModels).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(SupAdmin_RegulatoryAuthorityModels);

                    Type type = typeof(SupAdmin_RegulatoryAuthorityModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(SupAdmin_RegulatoryAuthorityModels, null) == null || property.GetValue(SupAdmin_RegulatoryAuthorityModels, null).Equals(0))
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
                    return BadRequest("Error: Regulatory Authority with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

        [Route("api/SupAdmin_RegulatoryAuthority/DeleteRegulatoryAuthorityDetails")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new SupAdmin_RegulatoryAuthorityModel { regulatory_authority_id = id };
            currentClass.regulatory_authority_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("regulatory_authority_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }

    }
}
