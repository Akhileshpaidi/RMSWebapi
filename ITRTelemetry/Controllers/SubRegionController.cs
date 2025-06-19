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
    public class SubRegionController : ControllerBase
    {
        private MySqlDBContext mySqlDBContext;

        public SubRegionController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/Sub_Region/GetSub_RegionDetails")]
        [HttpGet]

        public IEnumerable<SubRegionModel> GetSub_RegionDetails()
        {
            return this.mySqlDBContext.SubRegionModels.Where(x => x.SubRegionStatus == "Active").ToList();
        }
        [Route("api/Sub_Region/Sub_RegionDetailsget")]
        [HttpDelete]
        public IActionResult Sub_RegionDetailsget(int id )
        {
            // var sub_RegionMasterID = SubRegionModels.Sub_RegionMasterID;
            var sub_RegionMasterID = id;
            var documents = this.mySqlDBContext.SubRegionModels
                        .Where(x => x.RegionMasterID == sub_RegionMasterID).ToList();
            //var document = this.mySqlDBContext.SubRegionModels.FirstOrDefault(x => x.RegionMasterID == sub_RegionMasterID);

            if (documents != null)
            {
                foreach (var document in documents)
                {
                    document.SubRegionStatus = "Inactive";
                }
                this.mySqlDBContext.SaveChanges();
                return Ok("Record updated successfully");
            }
          
                var regionDocument = this.mySqlDBContext.RegionModels.FirstOrDefault(x => x.RegionMasterID == sub_RegionMasterID);
                if (regionDocument != null)
                {
                    regionDocument.RegionStatus = "Inactive";
                    this.mySqlDBContext.SaveChanges();
                    return Ok("Region inactivated as no sub-region was found");
                }
                else
                {
                    return NotFound("Record not found");
                }
            
        }

        [Route("api/Sub_Region/InsertSub_RegionDetails")]
        [HttpPost]

        public IActionResult InsertParameter([FromBody] SubRegionModel SubRegionModel)
        {
            try
            {
                SubRegionModel.Sub_RegionName = SubRegionModel.Sub_RegionName?.Trim();
                var existingDepartment = this.mySqlDBContext.SubRegionModels
                    .FirstOrDefault(d => d.Sub_RegionName == SubRegionModel.Sub_RegionName && d.SubRegionStatus == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                var maxsubregionId = this.mySqlDBContext.SubRegionModels
          .Where(d => d.source == "No")
         .Max(d => (int?)d.Sub_RegionMasterID) ?? 0;
                SubRegionModel.Sub_RegionMasterID = maxsubregionId + 1;

                var SubRegionModels = this.mySqlDBContext.SubRegionModels;
               
              
                SubRegionModels.Add(SubRegionModel);
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            SubRegionModel.createddate = dt1;
            SubRegionModel.SubRegionStatus = "Active";
                SubRegionModel.source = "No";
            SubRegionModel.subregiontable = "SubRegionmaster";
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



        [Route("api/Sub_Region/UpdateSub_RegionDetails")]
        [HttpPut]

        public IActionResult UpdateType ([FromBody] SubRegionModel SubRegionModels)
        {

            try
            {
                if (SubRegionModels.Sub_RegionMasterID == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    SubRegionModels.Sub_RegionName = SubRegionModels.Sub_RegionName?.Trim();
                    var existingDepartment = this.mySqlDBContext.SubRegionModels
                  .FirstOrDefault(d => d.Sub_RegionName == SubRegionModels.Sub_RegionName && d.Sub_RegionMasterID != SubRegionModels.Sub_RegionMasterID && d.SubRegionStatus == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: sub region Name with the same name already exists.");
                    }

                    this.mySqlDBContext.Attach(SubRegionModels);
                        this.mySqlDBContext.Entry(SubRegionModels).State = EntityState.Modified;

                        var entry = this.mySqlDBContext.Entry(SubRegionModels);

                        Type type = typeof(SubRegionModel);
                        PropertyInfo[] properties = type.GetProperties();
                        foreach (PropertyInfo property in properties)
                        {
                            if (property.GetValue(SubRegionModels, null) == null || property.GetValue(SubRegionModels, null).Equals(0))
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
        [Route("api/Sub_Region/DeleteSub_RegionDetails")]
        [HttpDelete]

        public void DeleteSub_RegionDetails(int id)
        {
            var currentClass = new SubRegionModel { Sub_RegionMasterID = id };
            currentClass.SubRegionStatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("SubRegionStatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }
    }
}
