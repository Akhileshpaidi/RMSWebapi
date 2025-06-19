using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DomainModel;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Threading.Channels;
using System.Linq;
using MySqlConnector;

namespace ITRTelemetry.Controllers
{
    //[Route("api/[controller]")]

    [Produces("application/json")]
    //[ApiController]
    public class ComplianceGroupController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public ComplianceGroupController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        [Route("api/compliancegroup/GetcompliancegroupDetails")]
        [HttpGet]

        public IEnumerable<ComplianceGroupModel> GetComplianceGroup()
        {
            return this.mySqlDBContext.ComplianceGroupModels.Where(x => x.compliance_group_status == "Active").ToList();
        }



        [Route("api/compliancegroup/InsertcompliancegroupDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] ComplianceGroupModel ComplianceGroupModels)
        {
            try
            {
                ComplianceGroupModels.compliance_group_name = ComplianceGroupModels.compliance_group_name.Trim();

                var existingComplianceGroup = this.mySqlDBContext.ComplianceGroupModels
                    .FirstOrDefault(d => d.compliance_group_name == ComplianceGroupModels.compliance_group_name && d.compliance_group_status == "Active");

                if (existingComplianceGroup != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Compliance Group with the same name already exists.");
                }
                var maxcompliancegroupId = this.mySqlDBContext.ComplianceGroupModels
           .Where(d => d.source == "No")
           .Max(d => (int?)d.compliance_group_id) ?? 0; // If no records are found, default to 0

                // Increment the law_type_id by 1
                ComplianceGroupModels.compliance_group_id = maxcompliancegroupId + 1;

                var TypeModel = this.mySqlDBContext.ComplianceGroupModels;

               

                TypeModel.Add(ComplianceGroupModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                ComplianceGroupModels.compliance_group_Create_date = dt1;
                ComplianceGroupModels.compliance_group_status = "Active";
                ComplianceGroupModels.source = "No";

                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Compliance Group with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/compliancegroup/UpdatecompliancegroupDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] ComplianceGroupModel ComplianceGroupModels)
        {

            try
            {
                if (ComplianceGroupModels.compliance_group_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    ComplianceGroupModels.compliance_group_name = ComplianceGroupModels.compliance_group_name?.Trim();
                    var existingComplianceGroup = this.mySqlDBContext.ComplianceGroupModels
                       .FirstOrDefault(d => d.compliance_group_name == ComplianceGroupModels.compliance_group_name && d.compliance_group_id != ComplianceGroupModels.compliance_group_id && d.compliance_group_status == "Active");

                    if (existingComplianceGroup != null)
                    {
                        return BadRequest("Error: Compliance Group with the same name already exists.");
                    }
                    this.mySqlDBContext.Attach(ComplianceGroupModels);
                    this.mySqlDBContext.Entry(ComplianceGroupModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(ComplianceGroupModels);

                    Type type = typeof(ComplianceGroupModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(ComplianceGroupModels, null) == null || property.GetValue(ComplianceGroupModels, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
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
                    return BadRequest("Error: Compliance Group with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

        [Route("api/compliancegroup/DeletecompliancegroupDetails")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new ComplianceGroupModel { compliance_group_id = id };
            currentClass.compliance_group_status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("compliance_group_status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

    }
}

