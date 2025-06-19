using DomainModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Threading.Channels;
using Microsoft.AspNetCore.Http;
using System.Linq;
using MySqlConnector;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class JurisdictionController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public JurisdictionController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }



        [Route("api/Jurisdiction/GetJurisdiction")]
        [HttpGet]

        public IEnumerable<Jurisdictionmodel> GetJurisdiction()
        {
            return this.mySqlDBContext.Jurisdictionmodels.Where(x => x.status == "Active").ToList();
        }




        [Route("api/Jurisdiction/InsertJurisdiction")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] Jurisdictionmodel Jurisdictionmodels)
        {
            try
            {
                Jurisdictionmodels.jurisdiction_categoryname = Jurisdictionmodels.jurisdiction_categoryname.Trim();

                var existingJurisdiction = this.mySqlDBContext.Jurisdictionmodels
                    .FirstOrDefault(d => d.jurisdiction_categoryname == Jurisdictionmodels.jurisdiction_categoryname && d.status == "Active");

                if (existingJurisdiction != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Jurisdiction name with the same name already exists.");
                }
                var maxjuridicationId = this.mySqlDBContext.Jurisdictionmodels
              .Where(d => d.source == "No")
             .Max(d => (int?)d.jurisdiction_category_id) ?? 0; // If no records are found, default to 0

                // Increment the law_type_id by 1
                Jurisdictionmodels.jurisdiction_category_id = maxjuridicationId + 1;

                var TypeModel = this.mySqlDBContext.Jurisdictionmodels;
              
                TypeModel.Add(Jurisdictionmodels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                Jurisdictionmodels.jurisdiction_category_create_date = dt1;
                Jurisdictionmodels.status = "Active";
                Jurisdictionmodels.source = "No";

                this.mySqlDBContext.SaveChanges();
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
        [Route("api/Jurisdiction/UpdateJurisdiction")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] Jurisdictionmodel Jurisdictionmodels)
        {

            try
            {
                if (Jurisdictionmodels.jurisdiction_category_id == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    Jurisdictionmodels.jurisdiction_categoryname = Jurisdictionmodels.jurisdiction_categoryname?.Trim();
                    var existingDepartment = this.mySqlDBContext.Jurisdictionmodels
                  .FirstOrDefault(d => d.jurisdiction_categoryname == Jurisdictionmodels.jurisdiction_categoryname && d.jurisdiction_category_id != Jurisdictionmodels.jurisdiction_category_id && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error:jurisdiction name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(Jurisdictionmodels);
                    this.mySqlDBContext.Entry(Jurisdictionmodels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(Jurisdictionmodels);

                    Type type = typeof(Jurisdictionmodel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(Jurisdictionmodels, null) == null || property.GetValue(Jurisdictionmodels, null).Equals(0))
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
                    return BadRequest("Error: jurisdiction name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

        [Route("api/Jurisdiction/DeleteJurisdiction")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new Jurisdictionmodel { jurisdiction_category_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }
    }
}
