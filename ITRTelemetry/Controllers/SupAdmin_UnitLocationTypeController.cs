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
    public class SupAdmin_UnitLocationTypeController : ControllerBase
    {
        private CommonDBContext commonDBContext;
        public IConfiguration Configuration { get; }

        public SupAdmin_UnitLocationTypeController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }
        [Route("api/SupAdminUnitLocationType/GetUnitLocationTypeDetails")]
        [HttpGet]

        public IEnumerable<SupAdmin_UnitLocationTypeModel> GetUnitLocationTypeDetails()
        {
            return this.commonDBContext.SupAdmin_UnitLocationTypeModels.Where(x => x.UnitTypeStatus == "Active").ToList();
        }


        [Route("api/SuperAdminUnitLocationType/InsertUnitLocationTypeDetails")]
        [HttpPost]

        public IActionResult InsertParameter([FromBody] SupAdmin_UnitLocationTypeModel SupAdmin_UnitLocationTypeModels)
        {
          
            try
            {
                SupAdmin_UnitLocationTypeModels.UnitTypeName = SupAdmin_UnitLocationTypeModels.UnitTypeName?.Trim();

                var existingDepartment = this.commonDBContext.SupAdmin_UnitLocationTypeModels
                    .FirstOrDefault(d => d.UnitTypeName == SupAdmin_UnitLocationTypeModels.UnitTypeName && d.UnitTypeStatus == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                // Proceed with the insertion

                var maxunitytypeid = this.commonDBContext.SupAdmin_UnitLocationTypeModels.Max(d => (int?)d.UnitTypeID) ?? 5000;

                SupAdmin_UnitLocationTypeModels.UnitTypeID = maxunitytypeid + 1;

                var SupAdmin_UnitLocationTypeModel = this.commonDBContext.SupAdmin_UnitLocationTypeModels;
                SupAdmin_UnitLocationTypeModel.Add(SupAdmin_UnitLocationTypeModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                SupAdmin_UnitLocationTypeModels.createddate = dt1;
                SupAdmin_UnitLocationTypeModels.UnitTypeStatus = "Active";
                SupAdmin_UnitLocationTypeModels.unittypetablename = "unittypemaster";
                this.commonDBContext.SaveChanges();
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

        [Route("api/SuperAdminUnitLocationType/UpdateUnitLocationTypeDetails")]
        [HttpPut]

        public IActionResult UpdateUnitLocationTypeDetails([FromBody] SupAdmin_UnitLocationTypeModel SupAdmin_UnitLocationTypeModels)
        {
            try
            {
                if (SupAdmin_UnitLocationTypeModels.UnitTypeID == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    SupAdmin_UnitLocationTypeModels.UnitTypeName = SupAdmin_UnitLocationTypeModels.UnitTypeName?.Trim();
                    var existingDepartment = this.commonDBContext.SupAdmin_UnitLocationTypeModels
                  .FirstOrDefault(d => d.UnitTypeName == SupAdmin_UnitLocationTypeModels.UnitTypeName && d.UnitTypeID != SupAdmin_UnitLocationTypeModels.UnitTypeID && d.UnitTypeStatus == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: unitTypeName with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.commonDBContext.Attach(SupAdmin_UnitLocationTypeModels);
                    this.commonDBContext.Entry(SupAdmin_UnitLocationTypeModels).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(SupAdmin_UnitLocationTypeModels);

                    Type type = typeof(SupAdmin_UnitLocationTypeModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(SupAdmin_UnitLocationTypeModels, null) == null || property.GetValue(SupAdmin_UnitLocationTypeModels, null).Equals(0))
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
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/SuperadminUnitLocationType/DeleteUnitLocationTypeDetails")]
        [HttpDelete]

        public void DeleteUnitLocationTypeDetails(int id)
        {
            var currentClass = new SupAdmin_UnitLocationTypeModel { UnitTypeID = id };
            currentClass.UnitTypeStatus = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("UnitTypeStatus").IsModified = true;
            this.commonDBContext.SaveChanges();
        }
    }
}
