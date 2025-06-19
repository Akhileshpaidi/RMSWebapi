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
    //[Route("api/[controller]")]
    [Produces("application/json")]
    public class ComplianceRecordTypeController : Controller
    {
        private MySqlDBContext mySqlDBContext;

        public ComplianceRecordTypeController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/ComplianceRecordType/GetComplianceRecordDetails")]
        [HttpGet]

        public IEnumerable<ComplianceRecordTypeModel> GetComplianceRecordDetails()
        {
            return this.mySqlDBContext.ComplianceRecordTypeModels.Where(x => x.compliance_record_status == "Active").ToList();
        }



        [Route("api/ComplianceRecordType/InsertComplianceRecordDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] ComplianceRecordTypeModel ComplianceRecordTypeModels )
        {
            try
            {
                ComplianceRecordTypeModels.compliance_record_name = ComplianceRecordTypeModels.compliance_record_name?.Trim();
                var existingComplianceRecordType = this.mySqlDBContext.ComplianceRecordTypeModels
                    .FirstOrDefault(d => d.compliance_record_name == ComplianceRecordTypeModels.compliance_record_name && d.compliance_record_status == "Active");

                if (existingComplianceRecordType != null)
                {
                    return BadRequest("Error: Compliance Record Type with the same name already exists.");
                }
                var maxcompliancerecordId = this.mySqlDBContext.ComplianceRecordTypeModels
             .Where(d => d.source == "No")
             .Max(d => (int?)d.compliance_record_type_id) ?? 0; // If no records are found, default to 0

                // Increment the law_type_id by 1
                ComplianceRecordTypeModels.compliance_record_type_id = maxcompliancerecordId + 1;


                var ComplianceRecordTypeModel = this.mySqlDBContext.ComplianceRecordTypeModels;

               
                ComplianceRecordTypeModel.Add(ComplianceRecordTypeModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                ComplianceRecordTypeModels.compliance_record_create_date = dt1;
                ComplianceRecordTypeModels.compliance_record_status = "Active";
                ComplianceRecordTypeModels.source = "No";

                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Compliance Record Type with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/ComplianceRecordType/UpdateComplianceRecordDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] ComplianceRecordTypeModel ComplianceRecordTypeModels)
        {
            try
            {
                if (ComplianceRecordTypeModels.compliance_record_type_id == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    ComplianceRecordTypeModels.compliance_record_name = ComplianceRecordTypeModels.compliance_record_name?.Trim();
                    var existingComplianceRecord = this.mySqlDBContext.ComplianceRecordTypeModels
                  .FirstOrDefault(d => d.compliance_record_name == ComplianceRecordTypeModels.compliance_record_name && d.compliance_record_type_id != ComplianceRecordTypeModels.compliance_record_type_id && d.compliance_record_status == "Active");

                    if (existingComplianceRecord != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Compliance Record Type with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(ComplianceRecordTypeModels);
                    this.mySqlDBContext.Entry(ComplianceRecordTypeModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(ComplianceRecordTypeModels);

                    Type type = typeof(ComplianceRecordTypeModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(ComplianceRecordTypeModels, null) == null || property.GetValue(ComplianceRecordTypeModels, null).Equals(0))
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
                    return BadRequest("Error: Compliance Record Type with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }



        [Route("api/ComplianceRecordType/DeleteComplianceRecordDetails")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new ComplianceRecordTypeModel { compliance_record_type_id = id };
            currentClass.compliance_record_status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("compliance_record_status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


    }
}

