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
    public class UnitTypeController : ControllerBase
    {
               
        private MySqlDBContext mySqlDBContext;

        public UnitTypeController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/UnitType/GetUnitTypeMasterDetails")]
        [HttpGet]

        public IEnumerable<UnitTypeModel> GetUnitTypeMasterDetails()
        {
            return this.mySqlDBContext.UnitTypeModels.Where(x => x.UnitTypeStatus == "Active").ToList();
        }


        [Route("api/UnitType/InsertUnitTypeMasterDetails")]
        [HttpPost]

        public IActionResult InsertParameter([FromBody] UnitTypeModel UnitTypeModel)
        {
            try
            {
                UnitTypeModel.UnitTypeName = UnitTypeModel.UnitTypeName?.Trim();

                var existingDepartment = this.mySqlDBContext.UnitTypeModels
                    .FirstOrDefault(d => d.UnitTypeName == UnitTypeModel.UnitTypeName && d.UnitTypeStatus == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                var maxunittypeId = this.mySqlDBContext.UnitTypeModels
                 .Where(d => d.source == "No")
                .Max(d => (int?)d.UnitTypeID) ?? 0; // If no records are found, default to 0

                // Increment the law_type_id by 1
                UnitTypeModel.UnitTypeID = maxunittypeId + 1;

                var UnitTypeModels = this.mySqlDBContext.UnitTypeModels;
               
                UnitTypeModels.Add(UnitTypeModel);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                UnitTypeModel.createddate = dt1;
                UnitTypeModel.UnitTypeStatus = "Active";
                UnitTypeModel.source = "No";

                UnitTypeModel.unittypetablename = "unittypemaster";
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

        [Route("api/UnitTypeMaster/UpdateUnitTypeMasterDetails")]
        [HttpPut]

        public IActionResult UpdateUnitMasterDetails([FromBody] UnitTypeModel UnitTypeModels)
        {
            try
            {
                if (UnitTypeModels.UnitTypeID == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    UnitTypeModels.UnitTypeName = UnitTypeModels.UnitTypeName?.Trim();
                    var existingDepartment = this.mySqlDBContext.UnitTypeModels
                  .FirstOrDefault(d => d.UnitTypeName == UnitTypeModels.UnitTypeName && d.UnitTypeID != UnitTypeModels.UnitTypeID && d.UnitTypeStatus == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: unitTypeName with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(UnitTypeModels);
                    this.mySqlDBContext.Entry(UnitTypeModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(UnitTypeModels);

                    Type type = typeof(UnitTypeModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(UnitTypeModels, null) == null || property.GetValue(UnitTypeModels, null).Equals(0))
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

            [Route("api/UnitTypeMaster/DeleteUnitTypeMasterDetails")]
        [HttpDelete]

        public void DeleteUnitTypeMasterDetails(int id)
        {
            var currentClass = new UnitTypeModel { UnitTypeID = id };
            currentClass.UnitTypeStatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("UnitTypeStatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }
    }
}

