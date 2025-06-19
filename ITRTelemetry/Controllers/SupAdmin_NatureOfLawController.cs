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
    public class SupAdmin_NatureOfLawController : ControllerBase
    {
        private CommonDBContext commonDBContext;
        public IConfiguration Configuration { get; }

        public SupAdmin_NatureOfLawController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }
        [Route("api/SupAdminLawType/GetLawTypeDetails")]
        [HttpGet]

        public IEnumerable<SupAdmin_NatureOfLawModel> GetLawTypeDetails()
        {
            return this.commonDBContext.SupAdmin_NatureOfLawModels.Where(x => x.law_status == "Active").ToList();
        }


        [Route("api/SupAdminLawType/InsertLawTypeDetails")]
        [HttpPost]

        public IActionResult InsertLawTypeDetails([FromBody] SupAdmin_NatureOfLawModel SupAdmin_NatureOfLawModels)
        {
            try
            {
                SupAdmin_NatureOfLawModels.type_of_law = SupAdmin_NatureOfLawModels.type_of_law.Trim();

                var existingComplianceGroup = this.commonDBContext.SupAdmin_NatureOfLawModels
                    .FirstOrDefault(d => d.type_of_law == SupAdmin_NatureOfLawModels.type_of_law && d.law_status == "Active");

                if (existingComplianceGroup != null)
                {
                    return BadRequest("Error: Law Type with the same name already exists.");
                }

                var Maxlawtypeid = this.commonDBContext.SupAdmin_NatureOfLawModels.Max(d => (int?)d.law_type_id) ?? 5000;

                SupAdmin_NatureOfLawModels.law_type_id = Maxlawtypeid + 1;

                var TypeModel = this.commonDBContext.SupAdmin_NatureOfLawModels;
                TypeModel.Add(SupAdmin_NatureOfLawModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                SupAdmin_NatureOfLawModels.law_create_date = dt1;
                SupAdmin_NatureOfLawModels.law_status = "Active";
                this.commonDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Law Type with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/SupAdminLawType/UpdateLawTypeDetails")]
        [HttpPut]

        public IActionResult UpdateType([FromBody] SupAdmin_NatureOfLawModel SupAdmin_NatureOfLawModels)
        {

            try
            {
                if (SupAdmin_NatureOfLawModels.law_type_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    SupAdmin_NatureOfLawModels.type_of_law = SupAdmin_NatureOfLawModels.type_of_law?.Trim();
                    var existingComplianceGroup = this.commonDBContext.SupAdmin_NatureOfLawModels
                       .FirstOrDefault(d => d.type_of_law == SupAdmin_NatureOfLawModels.type_of_law && d.law_type_id != SupAdmin_NatureOfLawModels.law_type_id && d.law_status == "Active");

                    if (existingComplianceGroup != null)
                    {
                        return BadRequest("Error: Law Type with the same name already exists.");
                    }
                    this.commonDBContext.Attach(SupAdmin_NatureOfLawModels);
                    this.commonDBContext.Entry(SupAdmin_NatureOfLawModels).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(SupAdmin_NatureOfLawModels);

                    Type type = typeof(SupAdmin_NatureOfLawModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(SupAdmin_NatureOfLawModels, null) == null || property.GetValue(SupAdmin_NatureOfLawModels, null).Equals(0))
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
                    return BadRequest("Error: Law Type with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }


        [Route("api/SupAdminLawType/DeleteLawTypeDetails")]
        [HttpDelete]

        public void DeleteLawTypeDetails(int id)
        {
            var currentClass = new SupAdmin_NatureOfLawModel { law_type_id = id };
            currentClass.law_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("law_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }
    }
}
