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
    public class SupAdmin_HolidayController : Controller
    {
        private CommonDBContext commonDBContext;
        public IConfiguration Configuration { get; }

        public SupAdmin_HolidayController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }

        [Route("api/SupAdminHolidayMaster/GetHolidayMasterDetails")]
        [HttpGet]

        public IEnumerable<SupAdmin_HolidayModel> GetHolidayMasterDetails()

        {
            return this.commonDBContext.SupAdmin_HolidayModels.Where(x => x.status == "Active").ToList();
        }
        [Route("api/SupAdminHolidayMaster/InsertHolidayMasterDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] SupAdmin_HolidayModel SupAdmin_HolidayModels)
        {
            try
            {
                SupAdmin_HolidayModels.holidayname = SupAdmin_HolidayModels.holidayname?.Trim();
                var existingDepartment = this.commonDBContext.SupAdmin_HolidayModels
                    .FirstOrDefault(d => d.holidayname == SupAdmin_HolidayModels.holidayname && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                // Proceed with the insertion
                var Maxhoilid = this.commonDBContext.SupAdmin_HolidayModels.Max(d => (int?)d.holidayid) ?? 5000;

                SupAdmin_HolidayModels.holidayid = Maxhoilid + 1;

                var SupAdmin_HolidayModel = this.commonDBContext.SupAdmin_HolidayModels;
                SupAdmin_HolidayModel.Add(SupAdmin_HolidayModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                SupAdmin_HolidayModels.createddate = dt1;
                SupAdmin_HolidayModels.status = "Active";
                SupAdmin_HolidayModels.holidaymastertablename = "holidaymaster";

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
        [Route("api/SupAdminHolidayMaster/UpdateHolidayMasterDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] SupAdmin_HolidayModel SupAdmin_HolidayModels)
        {
            try
            {
                if (SupAdmin_HolidayModels.holidayid == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    SupAdmin_HolidayModels.holidayname = SupAdmin_HolidayModels.holidayname?.Trim();
                    var existingDepartment = this.commonDBContext.SupAdmin_HolidayModels
                  .FirstOrDefault(d => d.holidayname == SupAdmin_HolidayModels.holidayname && d.holidayid != SupAdmin_HolidayModels.holidayid && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Business Name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.commonDBContext.Attach(SupAdmin_HolidayModels);
                    this.commonDBContext.Entry(SupAdmin_HolidayModels).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(SupAdmin_HolidayModels);

                    Type type = typeof(SupAdmin_HolidayModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(SupAdmin_HolidayModels, null) == null || property.GetValue(SupAdmin_HolidayModels, null).Equals(0))
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
        [Route("api/SupAdminHolidayMaster/DeleteHolidayMasterDetails")]
        [HttpDelete]
        public void DeleteentityType(int id)
        {
            var currentClass = new SupAdmin_HolidayModel { holidayid = id };
            currentClass.status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }
    }
}
