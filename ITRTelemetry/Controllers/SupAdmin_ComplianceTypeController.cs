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
    public class SupAdmin_ComplianceTypeController : ControllerBase
    {

        private CommonDBContext commonDBContext;
        public IConfiguration Configuration { get; }

        public SupAdmin_ComplianceTypeController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }
        [Route("api/SupAdminComplianceType/GetComplianceTypeDetails")]
        [HttpGet]

        public IEnumerable<SupadminComplianceType> GetComplianceTypeDetails()
        {
            return this.commonDBContext.SupadminComplianceTypes.Where(x => x.compliance_type_status == "Active").ToList();
        }

        [Route("api/c")]
        [HttpGet]

        public IEnumerable<object> GetCompliancetype()
        {
            var details = (from compliancetype in commonDBContext.SupadminComplianceTypes
                           where compliancetype.compliance_type_status == "Active"
                           select new
                           {
                               compliancetype.compliance_type_id,
                               compliancetype.compliance_type_name
                           })
                           .ToList();
            return details;
        }


        [Route("api/SupAdminComplianceType/InsertComplianceTypeDetails")]
        [HttpPost]

        public IActionResult InsertParameter([FromBody] SupadminComplianceType SupadminComplianceTypes)
        {

            try
            {
                SupadminComplianceTypes.compliance_type_name = SupadminComplianceTypes.compliance_type_name.Trim();

                var existingPenaltyCategory = this.commonDBContext.SupadminComplianceTypes
                    .FirstOrDefault(d => d.compliance_type_name == SupadminComplianceTypes.compliance_type_name && d.compliance_type_status == "Active");

                if (existingPenaltyCategory != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Penalty Category with the same name already exists.");
                }


                var Maxtypeid = this.commonDBContext.SupadminComplianceTypes.Max(d => (int?)d.compliance_type_id) ?? 5000;

                SupadminComplianceTypes.compliance_type_id = Maxtypeid + 1;

                var SupadminComplianceType = this.commonDBContext.SupadminComplianceTypes;
                SupadminComplianceType.Add(SupadminComplianceTypes);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                SupadminComplianceTypes.compliance_type_create_date = dt1;
                SupadminComplianceTypes.compliance_type_status = "Active";
                this.commonDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Penalty Category with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/SupAdminComplianceType/UpdateComplianceTypeDetails")]
        [HttpPut]

        public IActionResult UpdateComplianceTypeDetails([FromBody] SupadminComplianceType SupadminComplianceTypes)
        {
            try
            {
                if (SupadminComplianceTypes.compliance_type_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    SupadminComplianceTypes.compliance_type_name = SupadminComplianceTypes.compliance_type_name?.Trim();
                    var existingCompliancenotifiedstatus = this.commonDBContext.SupadminComplianceTypes
                       .FirstOrDefault(d => d.compliance_type_name == SupadminComplianceTypes.compliance_type_name && d.compliance_type_id != SupadminComplianceTypes.compliance_type_id && d.compliance_type_status == "Active");

                    if (existingCompliancenotifiedstatus != null)
                    {
                        return BadRequest("Error: Penalty Category with the same name already exists.");
                    }
                    this.commonDBContext.Attach(SupadminComplianceTypes);
                    this.commonDBContext.Entry(SupadminComplianceTypes).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(SupadminComplianceTypes);

                    Type type = typeof(SupadminComplianceType);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(SupadminComplianceTypes, null) == null || property.GetValue(SupadminComplianceTypes, null).Equals(0))
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
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Penalty Category with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/SupAdminComplianceType/DeleteComplianceTypeDetails")]
        [HttpDelete]

        public void DeleteComplianceDetails(int id)
        {
            var currentClass = new SupadminComplianceType { compliance_type_id = id };
            currentClass.compliance_type_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("compliance_type_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }

    }
}
