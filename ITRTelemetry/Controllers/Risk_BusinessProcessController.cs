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

    [Produces("application/json")]
    public class Risk_BusinessProcessController : Controller
    {
        private readonly MySqlDBContext mySqlDBContext;
        public Risk_BusinessProcessController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        //[Route("api/BusinessProcess/GetBusinessProcessDetails")]
        //[HttpGet]

        //public IEnumerable<Risk_BusinessProcess> GetDepartmentMasterDetails()
        //{
        //    return this.mySqlDBContext.Risk_BusinessProcesss.Where(x => x.BuinessProcessStatus == "Active").ToList();
        //}

        [Route("api/BusinessProcess/GetBusinessProcessDetails")]
        [HttpGet]

        public IEnumerable<object> GetDepartmentMasterDetails()
        {

            var details = (from businessprocess in mySqlDBContext.Risk_BusinessProcesss
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on businessprocess.unitlocationid equals unitlocation.Unit_location_Master_id
                           join departmentmaster in mySqlDBContext.DepartmentModels on businessprocess.departmentid equals departmentmaster.Department_Master_id
                           join entitymaster in mySqlDBContext.UnitMasterModels on businessprocess.entityid equals entitymaster.Entity_Master_id
                           join businessfunction in mySqlDBContext.RiskBusinessFunctionModels on businessprocess.riskBusinessfunctionid equals businessfunction.riskBusinessfunctionid
                           where businessprocess.BuinessProcessStatus == "Active"
                           select new
                           {
                              
                               businessprocess.businessprocessID,
                               businessprocess.entityid,
                               entitymaster.Entity_Master_Name,
                               businessprocess.unitlocationid,
                               unitlocation.Unit_location_Master_name,
                               businessprocess.departmentid,
                               departmentmaster.Department_Master_name,
                               businessfunction.riskBusinessfunctionid,
                               businessfunction.riskbusinessname,
                                businessprocess.BusinessProcessName,
                               businessprocess.BusinessProcessDescription,
                              
                           }).ToList();

            return details.Cast<object>();
        }

        [Route("api/BusinessProcess/InsertBusinessProcessDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] Risk_BusinessProcess Risk_BusinessProcesss)
        {
            try
            {
                Risk_BusinessProcesss.BusinessProcessName = Risk_BusinessProcesss.BusinessProcessName?.Trim();

                var existingDepartment = this.mySqlDBContext.Risk_BusinessProcesss
                    .FirstOrDefault(d => d.BusinessProcessName == Risk_BusinessProcesss.BusinessProcessName && d.businessprocessID == Risk_BusinessProcesss.businessprocessID  && d.BuinessProcessStatus == "Active");

                if (existingDepartment != null)
                {
                  return BadRequest("Error:  Business Process with the same name already exists.");
                }
               
                var Risk_BusinessProcess = this.mySqlDBContext.Risk_BusinessProcesss;
                Risk_BusinessProcess.Add(Risk_BusinessProcesss);
                 //DateTime dt = DateTime.Now;
                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                //AuthorityNameModels.Authority_CreatedDate = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                Risk_BusinessProcesss.BuinessProcessStatus = "Active";


                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    return BadRequest("Error:  Business Process with the same name already exists.");
                }
                else
                {
                   return BadRequest($"Error: {ex.Message}");
                }
            }
        }
        [Route("api/BusinessProcess/UpdateBusinessProcessDetails")]
        [HttpPut]
        public IActionResult UpdateBusinessName([FromBody] Risk_BusinessProcess Risk_BusinessProcesss)
        {
            try
            {
                if (Risk_BusinessProcesss.businessprocessID == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    Risk_BusinessProcesss.BusinessProcessName = Risk_BusinessProcesss.BusinessProcessName?.Trim();

                    var existingDepartment = this.mySqlDBContext.Risk_BusinessProcesss
                     .FirstOrDefault(d => d.BusinessProcessName == Risk_BusinessProcesss.BusinessProcessName && d.businessprocessID != Risk_BusinessProcesss.businessprocessID && d.BuinessProcessStatus == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Business Process with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(Risk_BusinessProcesss);
                    this.mySqlDBContext.Entry(Risk_BusinessProcesss).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(Risk_BusinessProcesss);

                    Type type = typeof(Risk_BusinessProcess);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(Risk_BusinessProcesss, null) == null || property.GetValue(Risk_BusinessProcesss, null).Equals(0))
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
                    return BadRequest("Error:  Business Process with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }



        }

        [Route("api/BusinessProcess/DeleteBusinessProcessDetails")]
        [HttpDelete]
        public void DeleteBusinessProcessDetails(int id)
        {
            var currentClass = new Risk_BusinessProcess { businessprocessID = id };
            currentClass.BuinessProcessStatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("BuinessProcessStatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


        [Route("api/DepartmentName/GetDepartmentName")]
        [HttpGet]
        public IEnumerable<object> GetDepartmentName()
        {
            var details = (from businessfunction in mySqlDBContext.RiskBusinessFunctionModels
                           join departmentmaster in mySqlDBContext.DepartmentModels on businessfunction.departmentid equals departmentmaster.Department_Master_id
                           where businessfunction.status == "Active" 
                           select new
                           {
                               businessfunction.entityid,
                               departmentmaster.Department_Master_id,
                               departmentmaster.Department_Master_name,
                               businessfunction.unitlocationid,
                           })
                              .Distinct()
                  .ToList();

            return details;
        }

        [Route("api/BusinessName/GetBusinessName")]
        [HttpGet]
        public IEnumerable<object> GetBusinessName()
        {
            var details = (from businessfunction in mySqlDBContext.RiskBusinessFunctionModels
                           where businessfunction.status == "Active" 
                           select new
                           {

                               businessfunction.riskBusinessfunctionid,
                               businessfunction.riskbusinessname,
                               businessfunction.departmentid,



                           })
                             .Distinct()
                 .ToList();

            return details;
        }
    }
}
