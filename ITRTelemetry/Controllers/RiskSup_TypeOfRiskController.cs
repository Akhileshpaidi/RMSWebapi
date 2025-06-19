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
    public class RiskSup_TypeOfRiskController : Controller
    {
        private CommonDBContext commonDBContext;
        public IConfiguration Configuration { get; }

        public RiskSup_TypeOfRiskController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }
        [Route("api/SupAdminTypeOfRisk/GetTypeOfRisk")]
        [HttpGet]

        public IEnumerable<RiskSup_TypeOfRisk> GetCatageorylaws()
        {
            return this.commonDBContext.RiskSup_TypeOfRisks.Where(x => x.TypeOfRiskStatus == "Active").ToList();
        }
        [Route("api/SupAdminTypeOfRisk/InsertTypeOfRisk")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] RiskSup_TypeOfRisk RiskSup_TypeOfRisks)
        {
            try
            {
                RiskSup_TypeOfRisks.TypeOfRiskName = RiskSup_TypeOfRisks.TypeOfRiskName?.Trim();

                var existinglaw = this.commonDBContext.RiskSup_TypeOfRisks
                    .FirstOrDefault(d => d.TypeOfRiskName == RiskSup_TypeOfRisks.TypeOfRiskName && d.TypeOfRiskStatus == "Active");

                if (existinglaw != null)
                {
                   return BadRequest("Error: Type Of Risk Name with the same name already exists.");
                }
                var TypeModel = this.commonDBContext.RiskSup_TypeOfRisks;
                TypeModel.Add(RiskSup_TypeOfRisks);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                //RiskSup_TypeOfRisks.TypeOfRiskcreatedby = dt1;
                RiskSup_TypeOfRisks.TypeOfRiskStatus = "Active";
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
        [Route("api/SupAdminTypeOfRisk/UpdateTypeOfRisk")]
        [HttpPut]
        public IActionResult UpdateTypeOfRisk([FromBody] RiskSup_TypeOfRisk RiskSup_TypeOfRisks)
        {

            try
            {
                if (RiskSup_TypeOfRisks.TypeOfRiskID == 0)
                {
                   return Ok("Insertion successful");
                }
                else
                {
                    RiskSup_TypeOfRisks.TypeOfRiskName = RiskSup_TypeOfRisks.TypeOfRiskName?.Trim();
                    var existinglaw = this.commonDBContext.RiskSup_TypeOfRisks
                       .FirstOrDefault(d => d.TypeOfRiskName == RiskSup_TypeOfRisks.TypeOfRiskName && d.TypeOfRiskID != RiskSup_TypeOfRisks.TypeOfRiskID && d.TypeOfRiskStatus == "Active");

                    if (existinglaw != null)
                    {
                       return BadRequest("Error: Entity with the same name already exists.");
                    }
                   
                    this.commonDBContext.Attach(RiskSup_TypeOfRisks);
                    this.commonDBContext.Entry(RiskSup_TypeOfRisks).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(RiskSup_TypeOfRisks);

                    Type type = typeof(RiskSup_TypeOfRisk);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(RiskSup_TypeOfRisks, null) == null || property.GetValue(RiskSup_TypeOfRisks, null).Equals(0))
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

        [Route("api/SupAdminTypeOfRisk/DeleteTypeOfRisk")]
        [HttpDelete]
        public void DeleteTypeOfRisk(int id)
        {
            var currentClass = new RiskSup_TypeOfRisk { TypeOfRiskID = id };
            currentClass.TypeOfRiskStatus = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("TypeOfRiskStatus").IsModified = true;
            this.commonDBContext.SaveChanges();
        }
    }
}
