using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class RiskSup_RiskClassificationController : Controller
    {
        private CommonDBContext commonDBContext;
        public IConfiguration Configuration { get; }
        public RiskSup_RiskClassificationController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }
        [Route("api/SupAdminRiskClassification/GetRiskClassification")]
        [HttpGet]
        public IEnumerable<RiskSup_RiskClassification> GetRiskClassification()
        {
            return this.commonDBContext.RiskSup_RiskClassifications.Where(x => x.RiskClassificationStatus == "Active").ToList();
        }

        [Route("api/SupAdminRiskClassification/InsertRiskClassification")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] RiskSup_RiskClassification RiskSup_RiskClassifications)
        {
            try
            {
                RiskSup_RiskClassifications.RiskClassificationName = RiskSup_RiskClassifications.RiskClassificationName?.Trim();

                var existinglaw = this.commonDBContext.RiskSup_RiskClassifications
                    .FirstOrDefault(d => d.RiskClassificationName == RiskSup_RiskClassifications.RiskClassificationName && d.RiskClassificationStatus == "Active");

                if (existinglaw != null)
                {
                    return BadRequest("Error: Risk Classification Name with the same name already exists.");
                }
                var TypeModel = this.commonDBContext.RiskSup_RiskClassifications;
                TypeModel.Add(RiskSup_RiskClassifications);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                //RiskSup_TypeOfRisks.TypeOfRiskcreatedby = dt1;
                RiskSup_RiskClassifications.RiskClassificationStatus = "Active";
                this.commonDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    return BadRequest("Error: Entity with the same name already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }
        [Route("api/SupAdminRiskClassification/UpdateRiskClassification")]
        [HttpPut]
        public IActionResult UpdateRiskClassification([FromBody] RiskSup_RiskClassification RiskSup_RiskClassifications)
        {

            try
            {
                if (RiskSup_RiskClassifications.RiskClassificationID == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    RiskSup_RiskClassifications.RiskClassificationName = RiskSup_RiskClassifications.RiskClassificationName?.Trim();
                    var existinglaw = this.commonDBContext.RiskSup_RiskClassifications
                       .FirstOrDefault(d => d.RiskClassificationName == RiskSup_RiskClassifications.RiskClassificationName && d.RiskClassificationID != RiskSup_RiskClassifications.RiskClassificationID && d.RiskClassificationStatus == "Active");

                    if (existinglaw != null)
                    {
                        return BadRequest("Error: Entity with the same name already exists.");
                    }

                    this.commonDBContext.Attach(RiskSup_RiskClassifications);
                    this.commonDBContext.Entry(RiskSup_RiskClassifications).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(RiskSup_RiskClassifications);

                    Type type = typeof(RiskSup_RiskClassification);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(RiskSup_RiskClassifications, null) == null || property.GetValue(RiskSup_RiskClassifications, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.commonDBContext.SaveChanges();
                    // You may want to return some success response
                    return Ok("Update successful");
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    return BadRequest("Error: Entity with the same name already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

        [Route("api/SupAdminRiskClassification/DeleteRiskClassification")]
        [HttpDelete]
        public void DeleteRiskClassification(int id)
        {
            var currentClass = new RiskSup_RiskClassification { RiskClassificationID = id };
            currentClass.RiskClassificationStatus = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("RiskClassificationStatus").IsModified = true;
            this.commonDBContext.SaveChanges();
        }
    }
}
