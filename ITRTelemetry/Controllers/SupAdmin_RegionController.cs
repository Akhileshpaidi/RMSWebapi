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
    public class SupAdmin_RegionController : ControllerBase
    {
        private CommonDBContext commonDBContext;
        public IConfiguration Configuration { get; }

        public SupAdmin_RegionController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }


        [Route("api/SuperAdminRegion/GetRegionDetails")]
        [HttpGet]

        public IEnumerable<SupAdmin_RegionModel> GetRegionDetails()
        {
            return this.commonDBContext.SupAdmin_RegionModels.Where(x => x.RegionStatus == "Active").ToList();
        }

        [Route("api/SuperAdminRegion/InsertRegionDetails")]
        [HttpPost]

        public IActionResult InsertParameter([FromBody] SupAdmin_RegionModel SupAdmin_RegionModels)
        {
            try
            {
                SupAdmin_RegionModels.RegionName = SupAdmin_RegionModels.RegionName?.Trim();
                var existingDepartment = this.commonDBContext.SupAdmin_RegionModels
                    .FirstOrDefault(d => d.RegionName == SupAdmin_RegionModels.RegionName && d.RegionStatus == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                var MaxRegid = this.commonDBContext.SupAdmin_RegionModels.Max(d => (int?)d.RegionMasterID) ?? 5000;

                SupAdmin_RegionModels.RegionMasterID = MaxRegid + 1;

                var SupAdmin_RegionModel = this.commonDBContext.SupAdmin_RegionModels;
                SupAdmin_RegionModel.Add(SupAdmin_RegionModels);
                DateTime dt = DateTime.Now;
                DateTime dt1 = DateTime.ParseExact(dt.ToString("yyyy-MM-dd HH:mm:ss"), "yyyy-MM-dd HH:mm:ss", null);

                SupAdmin_RegionModels.createddate = dt1;
                SupAdmin_RegionModels.updated_at = dt1;
                SupAdmin_RegionModels.RegionStatus = "Active";
                SupAdmin_RegionModels.regiontablename = "regions";
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

     
        
        [Route("api/SuperAdminRegion/UpdateRegionDetails")]
        [HttpPost]
        public IActionResult UpdateType([FromBody] SupAdmin_RegionModel SupAdmin_RegionModels)
        {

            try
            {
                if (SupAdmin_RegionModels.RegionMasterID == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    SupAdmin_RegionModels.RegionName = SupAdmin_RegionModels.RegionName?.Trim();
                    DateTime dt = DateTime.Now;
                    DateTime dt1 = DateTime.ParseExact(dt.ToString("yyyy-MM-dd HH:mm:ss"), "yyyy-MM-dd HH:mm:ss", null);

                 
                    SupAdmin_RegionModels.updated_at = dt1;

                    var existingDepartment = this.commonDBContext.SupAdmin_RegionModels
                  .FirstOrDefault(d => d.RegionName == SupAdmin_RegionModels.RegionName && d.RegionMasterID != SupAdmin_RegionModels.RegionMasterID && d.RegionStatus == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: sub region Name with the same name already exists.");
                    }

                    this.commonDBContext.Attach(SupAdmin_RegionModels);
                    this.commonDBContext.Entry(SupAdmin_RegionModels).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(SupAdmin_RegionModels);

                    Type type = typeof(SupAdmin_RegionModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(SupAdmin_RegionModels, null) == null || property.GetValue(SupAdmin_RegionModels, null).Equals(0))
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
                    return BadRequest("Error: sub region  with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

        [Route("api/SuperAdminRegion/DeleteRegionDetails")]
        [HttpDelete]

        public void DeleteRegionDetails(int id)
        {
            var currentClass = new SupAdmin_RegionModel { RegionMasterID = id };
            currentClass.RegionStatus = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("RegionStatus").IsModified = true;
            this.commonDBContext.SaveChanges();
        }



        [Route("api/Sup_SubRegion/Sup_SubRegionDetailsget")]
        [HttpGet]
        public IActionResult Sup_SubRegionDetailsget(int id)
        {
          
            var sub_RegionMasterID = id;
            var documents = this.commonDBContext.SupAdmin_SubRegionModels
                        .Where(x => x.RegionMasterID == sub_RegionMasterID).ToList();
          //  var document = this.commonDBContext.SupAdmin_SubRegionModels.FirstOrDefault(x => x.RegionMasterID == sub_RegionMasterID);

            if (documents.Count > 0)
            {
                foreach (var document in documents)
                {
                    document.SubRegionStatus = "Inactive";
                }
                this.commonDBContext.SaveChanges();
                return Ok("Record updated successfully");
            }
            
                var regionDocument = this.commonDBContext.SupAdmin_RegionModels.FirstOrDefault(x => x.RegionMasterID == sub_RegionMasterID);
                if (regionDocument != null)
                {
                    regionDocument.RegionStatus = "Inactive";
                    this.commonDBContext.SaveChanges();
                    return Ok("Region inactivated as no sub-region was found");
                }
                else
                {
                    return NotFound("Record not found");
                }
            
        }
    }
}
