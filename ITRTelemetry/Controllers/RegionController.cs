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
using Microsoft.AspNetCore.Cors;

namespace ITRTelemetry.Controllers
{
    

    [Produces("application/json")]

    public class RegionController : ControllerBase
    {
        private MySqlDBContext mySqlDBContext;

        public RegionController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/Region/GetRegionDetails")]
        [HttpGet]

        public IEnumerable<RegionModel> GetRegionDetails()
        {
            return this.mySqlDBContext.RegionModels.Where(x => x.RegionStatus == "Active").ToList();
        }


        [Route("api/Region/InsertRegionDetails")]
        [HttpPost]

        public IActionResult InsertParameter([FromBody] RegionModel RegionModel)
        {
            try
            {
                RegionModel.RegionName = RegionModel.RegionName?.Trim();
                var existingDepartment = this.mySqlDBContext.RegionModels
                    .FirstOrDefault(d => d.RegionName == RegionModel.RegionName && d.RegionStatus == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                var maxregionId = this.mySqlDBContext.RegionModels
           .Where(d => d.source == "No")
          .Max(d => (int?)d.RegionMasterID) ?? 0; // If no records are found, default to 0

                // Increment the law_type_id by 1
                RegionModel.RegionMasterID = maxregionId + 1;
                var RegionModels = this.mySqlDBContext.RegionModels;
               
                RegionModels.Add(RegionModel);
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            RegionModel.createddate= dt1;
            RegionModel.RegionStatus = "Active";
                RegionModel.source = "No";
                RegionModel.regiontablename = "RegionMaster";
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

        [Route("api/Region/UpdateRegionDetails")]
        [HttpPut]

        public IActionResult UpdateType([FromBody] RegionModel RegionModels)
        {

            try
            {
                if (RegionModels.RegionMasterID == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    RegionModels.RegionName = RegionModels.RegionName?.Trim();
                    var existingDepartment = this.mySqlDBContext.RegionModels
                  .FirstOrDefault(d => d.RegionName == RegionModels.RegionName && d.RegionMasterID != RegionModels.RegionMasterID && d.RegionStatus == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error:Region Name with the same name already exists.");
                    }

                    this.mySqlDBContext.Attach(RegionModels);
                this.mySqlDBContext.Entry(RegionModels).State = EntityState.Modified;

                var entry = this.mySqlDBContext.Entry(RegionModels);

                Type type = typeof(RegionModel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(RegionModels, null) == null || property.GetValue(RegionModels, null).Equals(0))
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
                    return BadRequest("Error: sub region  with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }


        [Route("api/Region/DeleteRegionDetails")]
        [HttpDelete]

        public void DeleteRegionDetails(int id)
        {
            try
            {                
                
              //  var existingDepartment = this.mySqlDBContext.SubRegionModels
              //.FirstOrDefault(d => d.RegionMasterID != RegionModels.RegionMasterID );

              //  if (existingDepartment != null)
              //  {
              //     return BadRequest("Error:Region Name with the same name already exists.");
              //  }
                var currentClass = new RegionModel { RegionMasterID = id };
                currentClass.RegionStatus = "Inactive";
                this.mySqlDBContext.Entry(currentClass).Property("RegionStatus").IsModified = true;
                this.mySqlDBContext.SaveChanges();
                

            }
            catch
            {
                return;
            }
           
           
        }
    }
}
