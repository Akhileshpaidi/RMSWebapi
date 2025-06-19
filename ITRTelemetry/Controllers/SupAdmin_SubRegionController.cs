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
    public class SupAdmin_SubRegionController : Controller
    {
        private CommonDBContext commonDBContext;
        public IConfiguration Configuration { get; }

        public SupAdmin_SubRegionController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }
        [Route("api/SuperAdminSubRegion/GetSubRegionDetails")]
        [HttpGet]

        public IEnumerable<SupAdmin_SubRegionModel> GetSubRegionDetails()
        {
            return this.commonDBContext.SupAdmin_SubRegionModels.Where(x => x.SubRegionStatus == "Active").ToList();
        }

        [Route("api/SuperAdminSubRegion/InsertSubRegionDetails")]
        [HttpPost]

        public IActionResult InsertParameter([FromBody] SupAdmin_SubRegionModel SupAdmin_SubRegionModels)
        {
            try
            {
                SupAdmin_SubRegionModels.Sub_RegionName = SupAdmin_SubRegionModels.Sub_RegionName?.Trim();
                var existingDepartment = this.commonDBContext.SupAdmin_SubRegionModels
                    .FirstOrDefault(d => d.Sub_RegionName == SupAdmin_SubRegionModels.Sub_RegionName && d.SubRegionStatus == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }

                var MaxsubRegid = this.commonDBContext.SupAdmin_SubRegionModels.Max(d => (int?)d.Sub_RegionMasterID) ?? 5000;

                SupAdmin_SubRegionModels.Sub_RegionMasterID = MaxsubRegid + 1;
                var SupAdmin_SubRegionModel = this.commonDBContext.SupAdmin_SubRegionModels;
                SupAdmin_SubRegionModel.Add(SupAdmin_SubRegionModels);
                DateTime dt = DateTime.Now;
                DateTime dt1 = DateTime.ParseExact(dt.ToString("yyyy-MM-dd HH:mm:ss"), "yyyy-MM-dd HH:mm:ss", null);
                SupAdmin_SubRegionModels.createddate = dt1;
                SupAdmin_SubRegionModels.updated_at = dt1;
                SupAdmin_SubRegionModels.SubRegionStatus = "Active";
                SupAdmin_SubRegionModels.subregiontable = "subregions";
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

        [Route("api/SuperAdminSubRegion/UpdateSubRegionDetails")]
        [HttpPut]

        public IActionResult UpdateType([FromBody] SupAdmin_SubRegionModel SupAdmin_SubRegionModels)
        {

            try
            {
                if (SupAdmin_SubRegionModels.Sub_RegionMasterID == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    SupAdmin_SubRegionModels.Sub_RegionName = SupAdmin_SubRegionModels.Sub_RegionName?.Trim();
                    DateTime dt = DateTime.Now;
                    DateTime dt1 = DateTime.ParseExact(dt.ToString("yyyy-MM-dd HH:mm:ss"), "yyyy-MM-dd HH:mm:ss", null);


                    SupAdmin_SubRegionModels.updated_at = dt1;

                    var existingDepartment = this.commonDBContext.SupAdmin_SubRegionModels
                  .FirstOrDefault(d => d.Sub_RegionName == SupAdmin_SubRegionModels.Sub_RegionName && d.Sub_RegionMasterID != SupAdmin_SubRegionModels.Sub_RegionMasterID && d.SubRegionStatus == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: sub region Name with the same name already exists.");
                    }

                    this.commonDBContext.Attach(SupAdmin_SubRegionModels);
                    this.commonDBContext.Entry(SupAdmin_SubRegionModels).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(SupAdmin_SubRegionModels);

                    Type type = typeof(SupAdmin_SubRegionModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(SupAdmin_SubRegionModels, null) == null || property.GetValue(SupAdmin_SubRegionModels, null).Equals(0))
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

        [Route("api/SuperAdminSubRegion/DeleteSubRegionDetails")]
        [HttpDelete]

        public void DeleteSubRegionDetails(int id)
        {
            var currentClass = new SupAdmin_SubRegionModel { Sub_RegionMasterID = id };
            currentClass.SubRegionStatus = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("SubRegionStatus").IsModified = true;
            this.commonDBContext.SaveChanges();
        }
    }
}
