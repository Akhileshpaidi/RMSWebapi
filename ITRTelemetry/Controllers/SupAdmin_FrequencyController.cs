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
    public class SupAdmin_FrequencyController : Controller
    {
        private CommonDBContext commonDBContext;
        public IConfiguration Configuration { get; }

        public SupAdmin_FrequencyController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }
        [Route("api/SupAdminFrequencyMaster/GetFrequencyMasterDetails")]
        [HttpGet]

        public IEnumerable<SupAdmin_FrequencyModel> GetFrequencyMasterDetails()
        {
            return this.commonDBContext.SupAdmin_FrequencyModels.Where(x => x.status == "Active").ToList();
        }

        [Route("api/SupAdminfrequencymaster/GetnofrequencyIntervals/{frequencyid}")]
        [HttpGet]
        public IEnumerable<int> GetnofrequencyIntervals(int frequencyid)
        {
            var details = (from frequence in commonDBContext.SupAdmin_FrequencyModels
                           where frequence.status == "Active" && frequence.frequencyid == frequencyid
                           select frequence.nooffrequencyintervals)
                         .ToList();
            return details;
        }
        [Route("api/frequencymaster/GetfrequencyDetails")]
        [HttpGet]

        public IEnumerable<object> GetfrequencyDetails()
        {
            var details = (from frequence in commonDBContext.SupAdmin_FrequencyModels
                           where frequence.status == "Active"
                           select new
                           {
                               frequence.frequencyid,
                               frequence.frequencyperiod
                           })
                         .ToList();
            return details;
        }

        [Route("api/SupAdminFrequencymaster/InsertFrequencyMasterDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] SupAdmin_FrequencyModel SupAdmin_FrequencyModels)
        {
            try
            {
                SupAdmin_FrequencyModels.frequencyperiod = SupAdmin_FrequencyModels.frequencyperiod?.Trim();
                var existingDepartment = this.commonDBContext.SupAdmin_FrequencyModels
                    .FirstOrDefault(d => d.frequencyperiod == SupAdmin_FrequencyModels.frequencyperiod && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                // Proceed with the insertion

                var Maxfreqid = this.commonDBContext.SupAdmin_FrequencyModels.Max(d => (int?)d.frequencyid) ?? 5000;

                SupAdmin_FrequencyModels.frequencyid = Maxfreqid + 1;

                var SupAdmin_FrequencyModel = this.commonDBContext.SupAdmin_FrequencyModels;
                SupAdmin_FrequencyModel.Add(SupAdmin_FrequencyModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                SupAdmin_FrequencyModels.createddate = dt1;
                SupAdmin_FrequencyModels.status = "Active";
                SupAdmin_FrequencyModels.frequencymastertablename = "frequencymaster";
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
        [Route("api/SupAdminFrequencymaster/UpdateFrequencyMasterDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] SupAdmin_FrequencyModel SupAdmin_FrequencyModels)
        {
            try
            {
                if (SupAdmin_FrequencyModels.frequencyid == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    SupAdmin_FrequencyModels.frequencyperiod = SupAdmin_FrequencyModels.frequencyperiod?.Trim();
                    var existingDepartment = this.commonDBContext.SupAdmin_FrequencyModels
                  .FirstOrDefault(d => d.frequencyperiod == SupAdmin_FrequencyModels.frequencyperiod && d.frequencyid != SupAdmin_FrequencyModels.frequencyid && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Business Name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.commonDBContext.Attach(SupAdmin_FrequencyModels);
                    this.commonDBContext.Entry(SupAdmin_FrequencyModels).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(SupAdmin_FrequencyModels);

                    Type type = typeof(SupAdmin_FrequencyModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(SupAdmin_FrequencyModels, null) == null || property.GetValue(SupAdmin_FrequencyModels, null).Equals(0))
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

        [Route("api/SupAdminFrequencyMaster/DeleteFrequencyMasterDetails")]
        [HttpDelete]
        public void DeleteentityType(int id)
        {
            var currentClass = new SupAdmin_FrequencyModel { frequencyid = id };
            currentClass.status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }


    }
}
