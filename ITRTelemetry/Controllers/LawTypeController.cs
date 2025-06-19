using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MySqlConnector;
using System.Threading.Channels;


namespace ITRTelemetry.Controllers
{
    //[Route("api/[controller]")]

    [Produces("application/json")]
    public class LawTypeController : ControllerBase
    {
        private MySqlDBContext mySqlDBContext;

        public LawTypeController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/LawType/GetLawTypeDetails")]
        [HttpGet]
       
        public IEnumerable<LawTypeModel> GetLawTypeDetails()
        {
            return this.mySqlDBContext.LawTypeModels.Where(x => x.law_status == "Active").ToList();
        }


        [Route("api/LawType/InsertLawTypeDetails")]
        [HttpPost]

        public IActionResult InsertLawTypeDetails([FromBody] LawTypeModel LawTypeModels)
        {
            try
            {
                LawTypeModels.type_of_law = LawTypeModels.type_of_law.Trim();

                var existingComplianceGroup = this.mySqlDBContext.LawTypeModels
                    .FirstOrDefault(d => d.type_of_law == LawTypeModels.type_of_law && d.law_status == "Active");

                if (existingComplianceGroup != null)
                {
                    return BadRequest("Error: Law Type with the same name already exists.");
                }

                var maxLawTypeId = this.mySqlDBContext.LawTypeModels
                  .Where(d => d.source == "No")
                 .Max(d => (int?)d.law_type_id) ?? 0; // If no records are found, default to 0

                // Increment the law_type_id by 1
                LawTypeModels.law_type_id = maxLawTypeId + 1;
                var TypeModel = this.mySqlDBContext.LawTypeModels;
                TypeModel.Add(LawTypeModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                LawTypeModels.law_create_date = dt1;
                LawTypeModels.law_status = "Active";
                LawTypeModels.source = "No";
                this.mySqlDBContext.SaveChanges();
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

        [Route("api/LawType/UpdateLawTypeDetails")]
        [HttpPut]

        public IActionResult UpdateType([FromBody] LawTypeModel LawTypeModels)
        {

            try
            {
                if (LawTypeModels.law_type_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    LawTypeModels.type_of_law = LawTypeModels.type_of_law?.Trim();
                    var existingComplianceGroup = this.mySqlDBContext.LawTypeModels
                       .FirstOrDefault(d => d.type_of_law == LawTypeModels.type_of_law && d.law_type_id != LawTypeModels.law_type_id && d.law_status == "Active");

                    if (existingComplianceGroup != null)
                    {
                        return BadRequest("Error: Law Type with the same name already exists.");
                    }
                    this.mySqlDBContext.Attach(LawTypeModels);
                    this.mySqlDBContext.Entry(LawTypeModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(LawTypeModels);

                    Type type = typeof(LawTypeModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(LawTypeModels, null) == null || property.GetValue(LawTypeModels, null).Equals(0))
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
                    return BadRequest("Error: Law Type with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }


        [Route("api/LawType/DeleteLawTypeDetails")]
        [HttpDelete]

        public void DeleteLawTypeDetails(int id)
        {
            var currentClass = new LawTypeModel { law_type_id = id };
            currentClass.law_status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("law_status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }
    }
}
