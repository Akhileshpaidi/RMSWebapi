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
    public class SupAdmin_ComplianceGroupController : ControllerBase
    {
        private CommonDBContext commonDBContext;
        public IConfiguration Configuration { get; }

        public SupAdmin_ComplianceGroupController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }
        [Route("api/SupAdmin_compliancegroup/GetcompliancegroupDetails")]
        [HttpGet]

        public IEnumerable<SupAdmin_ComplianceGroupModel> GetComplianceGroup()
        {
            return this.commonDBContext.SupAdmin_ComplianceGroupModels.Where(x => x.compliance_group_status == "Active").ToList();
        }



        [Route("api/SupAdmin_compliancegroup/InsertcompliancegroupDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] SupAdmin_ComplianceGroupModel SupAdmin_ComplianceGroupModels)
        {
            try
            {
                SupAdmin_ComplianceGroupModels.compliance_group_name = SupAdmin_ComplianceGroupModels.compliance_group_name.Trim();

                var existingComplianceGroup = this.commonDBContext.SupAdmin_ComplianceGroupModels
                    .FirstOrDefault(d => d.compliance_group_name == SupAdmin_ComplianceGroupModels.compliance_group_name && d.compliance_group_status == "Active");

                if (existingComplianceGroup != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Compliance Group with the same name already exists.");
                }
                var Maxgroupid = this.commonDBContext.SupAdmin_ComplianceGroupModels.Max(d => (int?)d.compliance_group_id) ?? 5000;

                SupAdmin_ComplianceGroupModels.compliance_group_id = Maxgroupid + 1;

                var TypeModel = this.commonDBContext.SupAdmin_ComplianceGroupModels;
                TypeModel.Add(SupAdmin_ComplianceGroupModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                SupAdmin_ComplianceGroupModels.compliance_group_Create_date = dt1;
                SupAdmin_ComplianceGroupModels.compliance_group_status = "Active";
                this.commonDBContext.SaveChanges();
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

        [Route("api/SupAdmin_compliancegroup/UpdatecompliancegroupDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] SupAdmin_ComplianceGroupModel SupAdmin_ComplianceGroupModels)
        {

            try
            {
                if (SupAdmin_ComplianceGroupModels.compliance_group_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    SupAdmin_ComplianceGroupModels.compliance_group_name = SupAdmin_ComplianceGroupModels.compliance_group_name?.Trim();
                    var existingComplianceGroup = this.commonDBContext.SupAdmin_ComplianceGroupModels
                       .FirstOrDefault(d => d.compliance_group_name == SupAdmin_ComplianceGroupModels.compliance_group_name && d.compliance_group_id != SupAdmin_ComplianceGroupModels.compliance_group_id && d.compliance_group_status == "Active");

                    if (existingComplianceGroup != null)
                    {
                        return BadRequest("Error: Compliance Group with the same name already exists.");
                    }
                    this.commonDBContext.Attach(SupAdmin_ComplianceGroupModels);
                    this.commonDBContext.Entry(SupAdmin_ComplianceGroupModels).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(SupAdmin_ComplianceGroupModels);

                    Type type = typeof(SupAdmin_ComplianceGroupModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(SupAdmin_ComplianceGroupModels, null) == null || property.GetValue(SupAdmin_ComplianceGroupModels, null).Equals(0))
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
                    return BadRequest("Error: Compliance Group with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

        [Route("api/SupAdmin_compliancegroup/DeletecompliancegroupDetails")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new SupAdmin_ComplianceGroupModel { compliance_group_id = id };
            currentClass.compliance_group_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("compliance_group_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }
    }
}
