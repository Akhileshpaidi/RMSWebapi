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
    public class SupAdmin_BusinessSectorListController : Controller
    {
        private CommonDBContext commonDBContext;
        public IConfiguration Configuration { get; }

        public SupAdmin_BusinessSectorListController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }

        [Route("api/SuperAdminBusinessSectorList/GetBusinessSectorListDetails")]
        [HttpGet]

        public IEnumerable<SupAdmin_BusinessSectorListModel> GetBusinessSectorListDetails()
        {
            return this.commonDBContext.SupAdmin_BusinessSectorListModels.Where(x => x.status == "Active").ToList();
        }



        [Route("api/SuperAdminBusinessSectorList/InsertBusinessSectorListDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] SupAdmin_BusinessSectorListModel SupAdmin_BusinessSectorListModels)
        {
            try
            {
                SupAdmin_BusinessSectorListModels.businesssectorname = SupAdmin_BusinessSectorListModels.businesssectorname?.Trim();
                var existingDepartment = this.commonDBContext.SupAdmin_BusinessSectorListModels
                    .FirstOrDefault(d => d.businesssectorname == SupAdmin_BusinessSectorListModels.businesssectorname && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                // Proceed with the insertion

                 var Maxbusiid = this.commonDBContext.SupAdmin_BusinessSectorListModels.Max(d => (int?)d.businesssectorid) ?? 5000;

                SupAdmin_BusinessSectorListModels.businesssectorid = Maxbusiid + 1;
                var SupAdmin_BusinessSectorListModel = this.commonDBContext.SupAdmin_BusinessSectorListModels;
                SupAdmin_BusinessSectorListModel.Add(SupAdmin_BusinessSectorListModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                SupAdmin_BusinessSectorListModels.createddate = dt1;
                SupAdmin_BusinessSectorListModels.status = "Active";
                SupAdmin_BusinessSectorListModels.businesssectortable = "businesssector";
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

        [Route("api/SuperAdminBusinessSectorList/UpdateBusinessSectorListDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] SupAdmin_BusinessSectorListModel SupAdmin_BusinessSectorListModels)
        {
            try
            {
                if (SupAdmin_BusinessSectorListModels.businesssectorid == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    SupAdmin_BusinessSectorListModels.businesssectorname = SupAdmin_BusinessSectorListModels.businesssectorname?.Trim();
                    var existingDepartment = this.commonDBContext.SupAdmin_BusinessSectorListModels
                  .FirstOrDefault(d => d.businesssectorname == SupAdmin_BusinessSectorListModels.businesssectorname && d.businesssectorid != SupAdmin_BusinessSectorListModels.businesssectorid && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Business Name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.commonDBContext.Attach(SupAdmin_BusinessSectorListModels);
                    this.commonDBContext.Entry(SupAdmin_BusinessSectorListModels).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(SupAdmin_BusinessSectorListModels);

                    Type type = typeof(SupAdmin_BusinessSectorListModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(SupAdmin_BusinessSectorListModels, null) == null || property.GetValue(SupAdmin_BusinessSectorListModels, null).Equals(0))
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



        [Route("api/SuperAdminBusinessSectorList/DeleteBusinessSectorListDetails")]
        [HttpDelete]
        public void DeleteentityType(int id)
        {
            var currentClass = new SupAdmin_BusinessSectorListModel { businesssectorid = id };
            currentClass.status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }
    }
}
