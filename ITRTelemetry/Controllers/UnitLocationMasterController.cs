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

    public class UnitLocationMasterController : Controller
    {
        private readonly MySqlDBContext mySqlDBContext;
        public UnitLocationMasterController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        //Getting Type Details by TypeID

        [Route("api/UnitLocationMaster/GetUnitLocationDetails")]
        [HttpGet]

        public IEnumerable<UnitLocationMasterModel> GetUnitLocationDetails()
        {

            return this.mySqlDBContext.UnitLocationMasterModels.Where(x => x.Unit_location_Master_Status == "Active").ToList(); 
        }


        [Route("api/UnitLocationMaster/GetAllUnitLocations")]
        [HttpGet]

        public IEnumerable<UnitLocationMasterModel> GetAllUnitLocations()
        {

            return this.mySqlDBContext.UnitLocationMasterModels.Where(x => x.Unit_location_Master_Status == "Active").ToList();
        }

        [Route("api/UnitLocationMaster/GetUnitLocationDetails/{Entity_Master_id}")]
        [HttpGet]

        public IEnumerable<UnitLocationMasterModel> GetUnitLocationDetails(int Entity_Master_id)
        {

            return this.mySqlDBContext.UnitLocationMasterModels.Where(x => x.Unit_location_Master_Status == "Active" && x.Entity_Master_id == Entity_Master_id).ToList();
        }

        [Route("api/UnitLocationMaster/GetMultipleUnitLocations/{Entity_Master_ids}")]
        [HttpGet]
        public IEnumerable<object> GetUnitLocationDetails(string Entity_Master_ids)
        {
            var ids = Entity_Master_ids.Split(',').Select(int.Parse).ToList();
            var unitLocations = this.mySqlDBContext.UnitLocationMasterModels
                               .Where(x => x.Unit_location_Master_Status == "Active" && ids.Contains(x.Entity_Master_id))
                               .Join(
                                   this.mySqlDBContext.UnitMasterModels, // The table to join.
                                   unitLocation => unitLocation.Entity_Master_id, // Key from the first table.
                                   unitMaster => unitMaster.Entity_Master_id, // Key from the second table.
                                   (unitLocation, unitMaster) => new // Result selector.
                                   {
                                       unitLocation.Unit_location_Master_id,
                                       unitLocation.Unit_location_Master_name,
                                       unitLocation.Unit_location_Master_Desc,
                                       unitLocation.Entity_Master_id,
                                       unitLocation.Unit_location_Master_Status,
                                       unitLocation.Unit_location_Master_createdDate,
                                       Entity_Master_Name = unitMaster.Entity_Master_Name
                                   })
                               .ToList();

            return unitLocations;
        }





        //Insert TypeMaster  Details

        [Route("api/UnitLocationMaster/InsertUnitLocationDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] UnitLocationMasterModel UnitLocationMasterModels)
        {
            try
            {
                UnitLocationMasterModels.Unit_location_Master_name = UnitLocationMasterModels.Unit_location_Master_name?.Trim();

                var existingDepartment = this.mySqlDBContext.UnitLocationMasterModels
                    .FirstOrDefault(d => d.Unit_location_Master_name == UnitLocationMasterModels.Unit_location_Master_name.Trim() && d.Entity_Master_id == UnitLocationMasterModels.Entity_Master_id && d.Unit_location_Master_Status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    // return BadRequest("Entity Unit location combination already exists with the same.");
                    return BadRequest("same Entity and location details already exists in the system.");

                }

                // Proceed with the insertion
                var UnitLocationMasterModel = this.mySqlDBContext.UnitLocationMasterModels;
                UnitLocationMasterModel.Add(UnitLocationMasterModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                UnitLocationMasterModels.Unit_location_Master_createdDate = dt1;
                UnitLocationMasterModels.Unit_location_Master_Status = "Active";
                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: same Entity and location details already exists in the system.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }
            [Route("api/UnitLocationMaster/UpdateUnitLocationDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] UnitLocationMasterModel UnitLocationMasterModels)
        {
            try
            {
                if (UnitLocationMasterModels.Unit_location_Master_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    UnitLocationMasterModels.Unit_location_Master_name = UnitLocationMasterModels.Unit_location_Master_name?.Trim();

                    var existingDepartment = this.mySqlDBContext.UnitLocationMasterModels
                        .FirstOrDefault(d => d.Unit_location_Master_name == UnitLocationMasterModels.Unit_location_Master_name && d.Unit_location_Master_id != UnitLocationMasterModels.Unit_location_Master_id && d.Entity_Master_id != UnitLocationMasterModels.Entity_Master_id && d.Unit_location_Master_Status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error:  same Entity and location details already exists in the system.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(UnitLocationMasterModels);
                    this.mySqlDBContext.Entry(UnitLocationMasterModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(UnitLocationMasterModels);

                    Type type = typeof(UnitLocationMasterModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(UnitLocationMasterModels, null) == null || property.GetValue(UnitLocationMasterModels, null).Equals(0))
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
                    return BadRequest("Error: same Entity and location details already exists in the system.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        [Route("api/UnitLocationMaster/DeleteUnitLocationDetails")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new UnitLocationMasterModel { Unit_location_Master_id = id };
            currentClass.Unit_location_Master_Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("Unit_location_Master_Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

    }
}