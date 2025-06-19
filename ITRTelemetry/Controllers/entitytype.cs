using DomainModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using MySqlConnector;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class entitytype : ControllerBase
    {


        private MySqlDBContext mySqlDBContext;

        public entitytype(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/entityType/GetentityTypeMasterDetails")]
        [HttpGet]

        public IEnumerable<entityTypeModel> GetentityTypeMasterDetails()
        {
            return this.mySqlDBContext.entityTypeModels.Where(x => x.entitytypestatus == "Active").ToList();
        }



        [Route("api/entityType/InsertentityTypeDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] entityTypeModel entityTypeModels)
        {
            try
            {
                entityTypeModels.entitytypename = entityTypeModels.entitytypename?.Trim();
                var existingDepartment = this.mySqlDBContext.entityTypeModels
                    .FirstOrDefault(d => d.entitytypename == entityTypeModels.entitytypename && d.entitytypestatus == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                var maxentitytypeId = this.mySqlDBContext.entityTypeModels
               .Where(d => d.source == "No")
              .Max(d => (int?)d.entitytypeid) ?? 0; // If no records are found, default to 0

              
               

                if (maxentitytypeId >= 4998)
                {
                    var maxAbove20000 = this.mySqlDBContext.entityTypeModels
                 .Where(d => d.entitytypeid >= 20000)
                 .Max(d => (int?)d.entitytypeid) ?? 19998; 

                    entityTypeModels.entitytypeid = maxAbove20000 + 1;
                }
                else
                {
                    // Increment the entitytypeid by 1
                    entityTypeModels.entitytypeid = maxentitytypeId + 1;
                }

                //entityTypeModels.entitytypeid = maxentitytypeId + 1;


                // Proceed with the insertion
                var entityTypeModel = this.mySqlDBContext.entityTypeModels;

              
                entityTypeModel.Add(entityTypeModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                entityTypeModels.createddate = dt1;
                entityTypeModels.entitytypestatus = "Active";
               entityTypeModels.source = "No";
                entityTypeModels.entitytypetablename = "EntittytypeMaster";
                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/entityType/UpdateentityTypeDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] entityTypeModel entityTypeModels)
        {
            try
            {
                if (entityTypeModels.entitytypeid == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    entityTypeModels.entitytypename = entityTypeModels.entitytypename?.Trim();
                    var existingDepartment = this.mySqlDBContext.entityTypeModels
                  .FirstOrDefault(d => d.entitytypename == entityTypeModels.entitytypename && d.entitytypeid!= entityTypeModels.entitytypeid && d.entitytypestatus == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: TypeName with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(entityTypeModels);
                    this.mySqlDBContext.Entry(entityTypeModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(entityTypeModels);

                    Type type = typeof(entityTypeModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(entityTypeModels, null) == null || property.GetValue(entityTypeModels, null).Equals(0))
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
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }



        [Route("api/entityType/DeleteentityTypeDetails")]
        [HttpDelete]
        public void DeleteentityType(int id)
        {
            var currentClass = new entityTypeModel { entitytypeid = id };
            currentClass.entitytypestatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("entitytypestatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


    }
}

