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
    public class SupAdmin_JurisdictionListController : Controller
    {
        private CommonDBContext commonDBContext;
        public IConfiguration Configuration { get; }

        public SupAdmin_JurisdictionListController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }
        [Route("api/SupAdmin_Jurisdiction/GetJurisdiction")]
        [HttpGet]

        public IEnumerable<SupAdmin_JurisdictionListModel> GetJurisdiction()
        {
            return this.commonDBContext.SupAdmin_JurisdictionListModels.Where(x => x.status == "Active").ToList();
        }




        [Route("api/SupAdmin_Jurisdiction/InsertJurisdiction")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] SupAdmin_JurisdictionListModel SupAdmin_JurisdictionListModels)
        {
            try
            {
                SupAdmin_JurisdictionListModels.jurisdiction_categoryname = SupAdmin_JurisdictionListModels.jurisdiction_categoryname.Trim();

                var existingJurisdiction = this.commonDBContext.SupAdmin_JurisdictionListModels
                    .FirstOrDefault(d => d.jurisdiction_categoryname == SupAdmin_JurisdictionListModels.jurisdiction_categoryname && d.status == "Active");

                if (existingJurisdiction != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Jurisdiction name with the same name already exists.");
                }
                var Maxjudcatid = this.commonDBContext.SupAdmin_JurisdictionListModels.Max(d => (int?)d.jurisdiction_category_id) ?? 5000;

                SupAdmin_JurisdictionListModels.jurisdiction_category_id = Maxjudcatid + 1;
                var TypeModel = this.commonDBContext.SupAdmin_JurisdictionListModels;
                TypeModel.Add(SupAdmin_JurisdictionListModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                SupAdmin_JurisdictionListModels.jurisdiction_category_create_date = dt1;
                SupAdmin_JurisdictionListModels.status = "Active";
                this.commonDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Entity with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }
        [Route("api/SupAdmin_Jurisdiction/UpdateJurisdiction")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] SupAdmin_JurisdictionListModel SupAdmin_JurisdictionListModels)
        {

            try
            {
                if (SupAdmin_JurisdictionListModels.jurisdiction_category_id == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    SupAdmin_JurisdictionListModels.jurisdiction_categoryname = SupAdmin_JurisdictionListModels.jurisdiction_categoryname?.Trim();
                    var existingDepartment = this.commonDBContext.SupAdmin_JurisdictionListModels
                  .FirstOrDefault(d => d.jurisdiction_categoryname == SupAdmin_JurisdictionListModels.jurisdiction_categoryname && d.jurisdiction_category_id != SupAdmin_JurisdictionListModels.jurisdiction_category_id && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error:jurisdiction name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.commonDBContext.Attach(SupAdmin_JurisdictionListModels);
                    this.commonDBContext.Entry(SupAdmin_JurisdictionListModels).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(SupAdmin_JurisdictionListModels);

                    Type type = typeof(SupAdmin_JurisdictionListModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(SupAdmin_JurisdictionListModels, null) == null || property.GetValue(SupAdmin_JurisdictionListModels, null).Equals(0))
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
                    return BadRequest("Error: jurisdiction name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

        [Route("api/SupAdmin_Jurisdiction/DeleteJurisdiction")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new SupAdmin_JurisdictionListModel { jurisdiction_category_id = id };
            currentClass.status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }
    }
}
