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
    public class SupAdmin_CategoryOfLawController : Controller
    {
        private CommonDBContext commonDBContext;
        public IConfiguration Configuration { get; }

        public SupAdmin_CategoryOfLawController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }
        [Route("api/SupAdminCatageorylaw/GetCatageorylaws")]
        [HttpGet]

        public IEnumerable<SupAdmin_CategoryOfLawModel> GetCatageorylaws()
        {
            return this.commonDBContext.SupAdmin_CategoryOfLawModels.Where(x => x.status == "Active").ToList();
        }




        [Route("api/SupAdminCatageorylaw/InsertCatageorylaw")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] SupAdmin_CategoryOfLawModel SupAdmin_CategoryOfLawModels)
        {
            try
            {
                SupAdmin_CategoryOfLawModels.law_Categoryname = SupAdmin_CategoryOfLawModels.law_Categoryname?.Trim();

                var existinglaw = this.commonDBContext.SupAdmin_CategoryOfLawModels
                    .FirstOrDefault(d => d.law_Categoryname == SupAdmin_CategoryOfLawModels.law_Categoryname && d.status == "Active");

                if (existinglaw != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: catageory law with the same name already exists.");
                }


                var Maxcatid = this.commonDBContext.SupAdmin_CategoryOfLawModels.Max(d => (int?)d.category_of_law_ID) ?? 5000;

                SupAdmin_CategoryOfLawModels.category_of_law_ID = Maxcatid + 1;

                var TypeModel = this.commonDBContext.SupAdmin_CategoryOfLawModels;
                TypeModel.Add(SupAdmin_CategoryOfLawModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                SupAdmin_CategoryOfLawModels.category_of_Law_Create_Date = dt1;
                SupAdmin_CategoryOfLawModels.status = "Active";
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
        [Route("api/SupAdminCatageorylaw/UpdateCatageorylaw")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] SupAdmin_CategoryOfLawModel SupAdmin_CategoryOfLawModels)
        {

            try
            {
                if (SupAdmin_CategoryOfLawModels.category_of_law_ID == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    SupAdmin_CategoryOfLawModels.law_Categoryname = SupAdmin_CategoryOfLawModels.law_Categoryname?.Trim();
                    var existinglaw = this.commonDBContext.SupAdmin_CategoryOfLawModels
                       .FirstOrDefault(d => d.law_Categoryname == SupAdmin_CategoryOfLawModels.law_Categoryname && d.category_of_law_ID != SupAdmin_CategoryOfLawModels.category_of_law_ID && d.status == "Active");

                    if (existinglaw != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Entity with the same name already exists.");
                    }
                    // Existing department, update logic
                    this.commonDBContext.Attach(SupAdmin_CategoryOfLawModels);
                    this.commonDBContext.Entry(SupAdmin_CategoryOfLawModels).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(SupAdmin_CategoryOfLawModels);

                    Type type = typeof(SupAdmin_CategoryOfLawModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(SupAdmin_CategoryOfLawModels, null) == null || property.GetValue(SupAdmin_CategoryOfLawModels, null).Equals(0))
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
                    return BadRequest("Error: Entity with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

        [Route("api/SupAdminCatageorylaw/DeleteCatageorylaw")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new SupAdmin_CategoryOfLawModel { category_of_law_ID = id };
            currentClass.status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }
    }
}
