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
    public class SupAdmin_PenaltyCategoryController : Controller
    {
        private CommonDBContext commonDBContext;
        public IConfiguration Configuration { get; }

        public SupAdmin_PenaltyCategoryController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }
        [Route("api/SupAdmin_penaltycategory/GetpenaltycategoryDetails")]
        [HttpGet]

        public IEnumerable<SupAdmin_PenaltyCategoryModel> GetComplianceNotifiedStatus()
        {
            return this.commonDBContext.SupAdmin_PenaltyCategoryModels.Where(x => x.penalty_category_status == "Active").ToList();
        }


  

        [Route("api/SupAdmin_penaltycategory/InsertpenaltycategoryDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] SupAdmin_PenaltyCategoryModel SupAdmin_PenaltyCategoryModels)
        {
            try
            {
                SupAdmin_PenaltyCategoryModels.penalty_category_name = SupAdmin_PenaltyCategoryModels.penalty_category_name.Trim();

                var existingPenaltyCategory = this.commonDBContext.SupAdmin_PenaltyCategoryModels
                    .FirstOrDefault(d => d.penalty_category_name == SupAdmin_PenaltyCategoryModels.penalty_category_name && d.penalty_category_status == "Active");

                if (existingPenaltyCategory != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Penalty Category with the same name already exists.");
                }
                var Maxpenaltyid = this.commonDBContext.SupAdmin_PenaltyCategoryModels.Max(d => (int?)d.penalty_category_id) ?? 5000;

                SupAdmin_PenaltyCategoryModels.penalty_category_id = Maxpenaltyid + 1;
                var TypeModel = this.commonDBContext.SupAdmin_PenaltyCategoryModels;
                TypeModel.Add(SupAdmin_PenaltyCategoryModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                SupAdmin_PenaltyCategoryModels.penalty_category_date = dt1;
                SupAdmin_PenaltyCategoryModels.penalty_category_status = "Active";
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

        [Route("api/SupAdmin_penaltycategory/UpdatepenaltycategoryDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] SupAdmin_PenaltyCategoryModel SupAdmin_PenaltyCategoryModels)
        {

            try
            {
                if (SupAdmin_PenaltyCategoryModels.penalty_category_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    SupAdmin_PenaltyCategoryModels.penalty_category_name = SupAdmin_PenaltyCategoryModels.penalty_category_name?.Trim();
                    var existingCompliancenotifiedstatus = this.commonDBContext.SupAdmin_PenaltyCategoryModels
                       .FirstOrDefault(d => d.penalty_category_name == SupAdmin_PenaltyCategoryModels.penalty_category_name && d.penalty_category_id != SupAdmin_PenaltyCategoryModels.penalty_category_id && d.penalty_category_status == "Active");

                    if (existingCompliancenotifiedstatus != null)
                    {
                        return BadRequest("Error: Penalty Category with the same name already exists.");
                    }
                    this.commonDBContext.Attach(SupAdmin_PenaltyCategoryModels);
                    this.commonDBContext.Entry(SupAdmin_PenaltyCategoryModels).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(SupAdmin_PenaltyCategoryModels);

                    Type type = typeof(SupAdmin_PenaltyCategoryModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(SupAdmin_PenaltyCategoryModels, null) == null || property.GetValue(SupAdmin_PenaltyCategoryModels, null).Equals(0))
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

        [Route("api/SupAdmin_penaltycategory/DeletepenaltycategoryDetails")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            try
            {
                var currentClass = new SupAdmin_PenaltyCategoryModel { penalty_category_id = id };
                currentClass.penalty_category_status = "Inactive";
                this.commonDBContext.Entry(currentClass).Property("penalty_category_status").IsModified = true;
                this.commonDBContext.SaveChanges();
            }
            catch
            {
                return;
            }
        }

    }
}
