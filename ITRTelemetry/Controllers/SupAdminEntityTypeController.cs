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
using Microsoft.Extensions.Configuration;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class SupAdminEntityTypeController : ControllerBase
    {
        private CommonDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }

        public SupAdminEntityTypeController(CommonDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }


        [Route("api/SuperAdminEntityType/GetEntityTypeMasterDetails")]
        [HttpGet]

        public IEnumerable<SupAdminEntityTypeModel> GetentityTypeMasterDetails()
        {
           // MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:commondb"]);
            return this.mySqlDBContext.SupAdminEntityTypeModels.Where(x => x.entitytypestatus == "Active").ToList();
        }



        [Route("api/SuperAdminEntityType/InsertEntityTypeDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] SupAdminEntityTypeModel SupAdminEntityTypeModels)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:commondb"]);

            try
            {
                SupAdminEntityTypeModels.entitytypename = SupAdminEntityTypeModels.entitytypename?.Trim();
                var existingDepartment = this.mySqlDBContext.SupAdminEntityTypeModels
                    .FirstOrDefault(d => d.entitytypename == SupAdminEntityTypeModels.entitytypename && d.entitytypestatus == "Active");

                if (existingDepartment != null)
                {
                   
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                // Proceed with the insertion

               var maxentitytypeid = this.mySqlDBContext.SupAdminEntityTypeModels .Max(d => (int?)d.entitytypeid) ?? 5000;

                SupAdminEntityTypeModels.entitytypeid = maxentitytypeid + 1;
                if (maxentitytypeid >= 19999)
                {
                    // Limit has been exceeded, return an error message
                    return BadRequest("Error: Limit has been exceeded. Cannot insert new record.");
                }
                var SupAdminEntityTypeModel = this.mySqlDBContext.SupAdminEntityTypeModels;
                SupAdminEntityTypeModel.Add(SupAdminEntityTypeModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                SupAdminEntityTypeModels.createddate = dt1;
                SupAdminEntityTypeModels.entitytypestatus = "Active";
                SupAdminEntityTypeModels.entitytypetablename = "entitytypemaster";
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

        [Route("api/SuperAdminEntityType/UpdateEntityTypeDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] SupAdminEntityTypeModel SupAdminEntityTypeModels)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:commondb"]);
            try
            {
                if (SupAdminEntityTypeModels.entitytypeid == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    SupAdminEntityTypeModels.entitytypename = SupAdminEntityTypeModels.entitytypename?.Trim();
                    var existingDepartment = this.mySqlDBContext.SupAdminEntityTypeModels
                  .FirstOrDefault(d => d.entitytypename == SupAdminEntityTypeModels.entitytypename && d.entitytypeid != SupAdminEntityTypeModels.entitytypeid && d.entitytypestatus == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Entity Type Name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(SupAdminEntityTypeModels);
                    this.mySqlDBContext.Entry(SupAdminEntityTypeModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(SupAdminEntityTypeModels);

                    Type type = typeof(SupAdminEntityTypeModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(SupAdminEntityTypeModels, null) == null || property.GetValue(SupAdminEntityTypeModels, null).Equals(0))
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
                    return BadRequest("Error: Entity Type Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }



        [Route("api/SuperAdminEntityType/DeleteEntityTypeDetails")]
        [HttpDelete]
        public void DeleteentityType(int id)

        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:commondb"]);
            var currentClass = new SupAdminEntityTypeModel { entitytypeid = id };
            currentClass.entitytypestatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("entitytypestatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }
    }
}
