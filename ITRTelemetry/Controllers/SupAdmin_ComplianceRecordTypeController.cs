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
    public class SupAdmin_ComplianceRecordTypeController : Controller
    {
        private CommonDBContext commonDBContext;
        public IConfiguration Configuration { get; }

        public SupAdmin_ComplianceRecordTypeController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }
        [Route("api/SupAdmin_ComplianceRecordType/GetComplianceRecordDetails")]
        [HttpGet]

        public IEnumerable<SupAdmin_ComplianceRecordTypeModel> GetComplianceRecordDetails()
        {
            return this.commonDBContext.SupAdmin_ComplianceRecordTypeModels.Where(x => x.compliance_record_status == "Active").ToList();
        }



        [Route("api/SupAdmin_ComplianceRecordType/InsertComplianceRecordDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] SupAdmin_ComplianceRecordTypeModel SupAdmin_ComplianceRecordTypeModels)
        {
            try
            {
                SupAdmin_ComplianceRecordTypeModels.compliance_record_name = SupAdmin_ComplianceRecordTypeModels.compliance_record_name?.Trim();
                var existingComplianceRecordType = this.commonDBContext.SupAdmin_ComplianceRecordTypeModels
                    .FirstOrDefault(d => d.compliance_record_name == SupAdmin_ComplianceRecordTypeModels.compliance_record_name && d.compliance_record_status == "Active");

                if (existingComplianceRecordType != null)
                {
                    return BadRequest("Error: Compliance Record Type with the same name already exists.");
                }

                var Maxrecordid = this.commonDBContext.SupAdmin_ComplianceRecordTypeModels.Max(d => (int?)d.compliance_record_type_id) ?? 5000;

                SupAdmin_ComplianceRecordTypeModels.compliance_record_type_id = Maxrecordid + 1;

                var SupAdmin_ComplianceRecordTypeModel = this.commonDBContext.SupAdmin_ComplianceRecordTypeModels;
                SupAdmin_ComplianceRecordTypeModel.Add(SupAdmin_ComplianceRecordTypeModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                SupAdmin_ComplianceRecordTypeModels.compliance_record_create_date = dt1;
                SupAdmin_ComplianceRecordTypeModels.compliance_record_status = "Active";
                this.commonDBContext.SaveChanges();
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

        [Route("api/SupAdmin_ComplianceRecordType/UpdateComplianceRecordDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] SupAdmin_ComplianceRecordTypeModel SupAdmin_ComplianceRecordTypeModels)
        {
            try
            {
                if (SupAdmin_ComplianceRecordTypeModels.compliance_record_type_id == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    SupAdmin_ComplianceRecordTypeModels.compliance_record_name = SupAdmin_ComplianceRecordTypeModels.compliance_record_name?.Trim();
                    var existingComplianceRecord = this.commonDBContext.SupAdmin_ComplianceRecordTypeModels
                  .FirstOrDefault(d => d.compliance_record_name == SupAdmin_ComplianceRecordTypeModels.compliance_record_name && d.compliance_record_type_id != SupAdmin_ComplianceRecordTypeModels.compliance_record_type_id && d.compliance_record_status == "Active");

                    if (existingComplianceRecord != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Compliance Record Type with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.commonDBContext.Attach(SupAdmin_ComplianceRecordTypeModels);
                    this.commonDBContext.Entry(SupAdmin_ComplianceRecordTypeModels).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(SupAdmin_ComplianceRecordTypeModels);

                    Type type = typeof(SupAdmin_ComplianceRecordTypeModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(SupAdmin_ComplianceRecordTypeModels, null) == null || property.GetValue(SupAdmin_ComplianceRecordTypeModels, null).Equals(0))
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
                    return BadRequest("Error: Compliance Record Type with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }



        [Route("api/SupAdmin_ComplianceRecordType/DeleteComplianceRecordDetails")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new SupAdmin_ComplianceRecordTypeModel { compliance_record_type_id = id };
            currentClass.compliance_record_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("compliance_record_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }


    }
}
